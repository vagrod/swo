using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assets.Code.Contracts;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using SeaWarsOnline.Core;
using SeaWarsOnline.Core.Multiplayer;
using UnityEngine;

namespace Assets.Code.PlayersInteraction
{   

    public class NetworkPlayer : PlayerBase, IOnEventCallback
    {

        private const int HitResponseTimeout = 2000; // Milliseconds
        private const int SpyResponseTimeout = 3000; // Milliseconds

        private readonly ManualResetEvent _hitSendEvent = new ManualResetEvent(true);
        private readonly ManualResetEvent _spyRequestSendEvent = new ManualResetEvent(true);

        private GameRules Rules { get; }

        private HitResult _hitResultReceived;
        private List<CellStateInfo> _spyResult;

        public NetworkPlayer(GameRules rules, GameFieldContract gameFieldData) : base(GameField.FromContract(rules.GameFieldSize, gameFieldData))
        {
            Rules = rules;

            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnEvent(EventData e)
        {
            if (e.Code == MessageCodes.HitResultReport)
            {
                // Hit report received from opponent
                var data = JsonUtility.FromJson<HitReportContract>((string)e.CustomData);

                if (data.sender == GameField.FieldId.ToString() || (!string.IsNullOrEmpty(data.initiator) && data.initiator != GameField.FieldId.ToString()))
                    return; // not interested in my own messages or messages I requested initially

                // Mirror hit to local opponent field
                GameField.ProcessHit(new Point(data.cell.x, data.cell.y));

                // Make hit result from received data
                _hitResultReceived = new HitResult
                {
                    HitLocation = new Point(data.cell.x, data.cell.y),
                    ShipDamaged = GameField.Ships.FirstOrDefault(x => x.ShipId.ToString() == data.damagedShipId),
                    ResultKind = (HitResult.HitResults) data.resultKind,
                    FieldCell = GameField.CellAtPoint(new Point(data.cell.x, data.cell.y))
                };

                if (_hitResultReceived.IsSuccessfulHit && _hitResultReceived.ShipDamaged != null && _hitResultReceived.ShipDamaged.IsDestroyed)
                {
                    // Mask-out the ship
                    var buffer = _hitResultReceived.ShipDamaged.GetBufferPoints();

                    foreach (var point in buffer)
                    {
                        var cell = GameField.CellAtPoint(point);

                        cell.State = FieldCellBase.CellStates.Miss;
                    }
                }
                
                _hitSendEvent.Set();
            }

            if (e.Code == MessageCodes.SpyResultReport)
            {
                // Spy report received from opponent
                var data = JsonUtility.FromJson<SpyReportContract>((string)e.CustomData);

                if (data.sender == GameField.FieldId.ToString())
                    return; // not interested in my own messages

                _spyResult = new List<CellStateInfo>();

                foreach (var cell in data.cells)
                {
                    _spyResult.Add(new CellStateInfo
                    {
                        Location = new Point(cell.x, cell.y),
                        State = (FieldCellBase.CellStates)cell.type
                    });
                }

                _spyRequestSendEvent.Set();
            }

            if (e.Code == MessageCodes.HitSend)
            {
                var data = JsonUtility.FromJson<HitSendContract>((string)e.CustomData);

                if (data.sender == GameField.FieldId.ToString())
                    return; // not interested in my own messages

                InvokeHitBack(new HitBackEventArgs
                {
                    CellLocation = new Point(data.cell.x, data.cell.y)
                });
            }
        }

        public override async Task<HitResult> Hit(Point cellLocation, bool isByMine)
        {
            return await Task.Run(() =>
            {
                _hitResultReceived = null;
                _hitSendEvent.Reset();

                var success = PhotonNetwork.RaiseEvent(MessageCodes.HitSend, JsonUtility.ToJson(new HitSendContract
                {
                    sender = GameField.FieldId.ToString(),
                    cell = new GameCellContract
                    {
                        x = cellLocation.X,
                        y = cellLocation.Y
                    }
                }), new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.DoNotCache }, SendOptions.SendReliable);

                if (!success)
                {
                    MultiplayerClient.Instance.ReportDisconnected();

                    return Task.FromResult(new HitResult
                    {
                        IsNotProcessed = true
                    });
                }

                _hitSendEvent.WaitOne(HitResponseTimeout);

                if (_hitResultReceived == null)
                {
                    MultiplayerClient.Instance.ReportDisconnected();

                    return Task.FromResult(new HitResult
                    {
                        IsNotProcessed = true
                    });
                }

                return Task.FromResult(_hitResultReceived);
            });
        }

        public override Task<bool> ReportHitBackResult(HitResult result)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> ReportMineHit(Point p)
        {
            return Task.FromResult(false);
        }

        public override async Task<List<CellStateInfo>> SpyAtPoint(Point p)
        {
            return await Task.Run(() =>
            {
                _spyResult = null;
                _spyRequestSendEvent.Reset();

                PhotonNetwork.RaiseEvent(MessageCodes.SpyRequest, JsonUtility.ToJson(new SpyRequestContract
                {
                    sender = GameField.FieldId.ToString(),
                    x = p.X,
                    y = p.Y
                }), new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.DoNotCache }, SendOptions.SendReliable);

                _spyRequestSendEvent.WaitOne(SpyResponseTimeout);

                if (_spyResult == null)
                {
                    MultiplayerClient.Instance.ReportDisconnected();

                    return Task.FromResult(new List<CellStateInfo>());
                }

                return Task.FromResult(_spyResult);
            });
        }

        public override void Reset()
        {
            base.Reset();

            PhotonNetwork.RemoveCallbackTarget(this);
        }

    }
}
