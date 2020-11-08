using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using Assets.Code.Contracts;
using Assets.Code.PlayersInteraction;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace SeaWarsOnline.Core.PlayersInteraction
{   
    public class BattleController : IOnEventCallback
    {

        #region Private Properties

        private MePlayer Me { get; }
        private PlayerBase Opponent { get; }
        private GameRules Rules { get; }

        #endregion

        #region Public Properties

        public bool IsMyTurn { get; private set; }

        public bool IsWaiting { get; private set; }

        public bool IsGameReady { get; private set; }

        public Action OnTurnChanged { get; set; }

        public bool IsGameOver => !Me.GameField.AnyShipsAlive || !Opponent.GameField.AnyShipsAlive;

        public bool? PlayerWon{
            get{
                if (!IsGameOver)
                    return null;

                if (Me.GameField.AnyShipsAlive)
                    return true;

                return false;
            }
        }

        #endregion

        #region Constructor

        public BattleController(GameRules options, MePlayer me, PlayerBase opponent, bool isMyTurnFirst)
        {
            Rules = options;
            IsMyTurn = isMyTurnFirst;
            Me = me;
            Opponent = opponent;

            Opponent.HitBack += OpponentOnHitBack;

            if (Rules.GameKind == GameRules.GameKinds.Network)
                PhotonNetwork.AddCallbackTarget(this);
        }

        #endregion

        #region Events Handling

        private async void OpponentOnHitBack(object sender, HitBackEventArgs e)
        {
            if (!IsGameReady)
                return;

            //UnityEngine.Debug.Log($"BattleController is processing hit back: chosen point is [{e.CellLocation.X};{e.CellLocation.Y}]");

            var result = await Me.Hit(e.CellLocation, isByMine: false);

            if (IsGameOver || result == null || Rules.ChangeTurnEachTime || (!Rules.ChangeTurnEachTime && !result.IsSuccessfulHit))
            {
                IsWaiting = false;
                IsMyTurn = true;
                //UnityEngine.Debug.Log($"OpponentOnHitBack: IsMyTurn is now {IsMyTurn}");
                
                UnityMainThreadDispatcher.Instance().Enqueue(
                    InvokeTurnChanged()
                );
            }

            await Opponent.ReportHitBackResult(result);

            MaskOutShipBufferIfNeeded(result, Me);

            if (result != null && result.ResultKind == HitResult.HitResults.MineActivated){
                //UnityEngine.Debug.Log($"Mine activated: hitting opponent as point [{result.HitLocation.X};{result.HitLocation.Y}]");
                // Special case for mine. Need to blow up our cell
                await Opponent.ReportMineHit(result.HitLocation);
                var mineResult = await Opponent.Hit(result.HitLocation, isByMine: true);

                MaskOutShipBufferIfNeeded(mineResult, Opponent);

                await Me.ReportHitBackResult(mineResult);
            }

            //UnityEngine.Debug.Log($"OpponentOnHitBack Result is {result.ResultKind.ToString()}. It's player turn");
        }

        private void MaskOutShipBufferIfNeeded(HitResult hitResult, PlayerBase player){
             if (hitResult?.ShipDamaged != null && hitResult.ShipDamaged.IsDestroyed){
                var buffer = hitResult.ShipDamaged.GetBufferPoints();

                foreach(var point in buffer){
                    player.GameField.CellAtPoint(point).State = FieldCellBase.CellStates.Miss;
                }
            }
        }

        #endregion

        #region Private Methods

        private IEnumerator InvokeTurnChanged(){
            if (!IsGameReady)
                yield return null;

            OnTurnChanged?.Invoke();

            yield return null;
        }

        #endregion  

        #region Public Methods

        public void DeclareReady(){
            IsGameReady = true;
        }

        public async Task<List<CellStateInfo>> SpyAtPoint(Point p){
             if (!IsGameReady)
                return null;
            
            IsMyTurn = false;
            IsWaiting = true;

            UnityMainThreadDispatcher.Instance().Enqueue(
                InvokeTurnChanged()
            );

            return await Opponent.SpyAtPoint(p);
        }

        public async Task<bool> ProcessPlayerTurn(Point hitPoint){
            if (!IsGameReady)
                return true;

            var c = Opponent.GameField.CellAtPoint(hitPoint);

            if (c.State == FieldCellBase.CellStates.Miss ||
                c.State == FieldCellBase.CellStates.Damaged ||
                c.State == FieldCellBase.CellStates.MineExploded)
                return true; // Do not process misclicks

            var hitResult = await Opponent.Hit(hitPoint, isByMine: false);

            if (hitResult.IsNotProcessed)
                return false;

            if (IsGameOver || Rules.ChangeTurnEachTime || (!Rules.ChangeTurnEachTime && !hitResult.IsSuccessfulHit))
            {
                IsMyTurn = false;
                IsWaiting = true;

                //UnityEngine.Debug.Log($"ProcessPlayerTurn: IsMyTurn is now {IsMyTurn}");

                UnityMainThreadDispatcher.Instance().Enqueue(
                    InvokeTurnChanged()
                );
            }

            if (hitResult.ResultKind == HitResult.HitResults.MineActivated){
                //UnityEngine.Debug.Log($"Mine activated: hitting player as point [{hitResult.HitLocation.X};{hitResult.HitLocation.Y}]");
                // Mine special case. Need to blow our cell up
                await Me.ReportMineHit(hitResult.HitLocation);
                var mineResult = await Me.Hit(hitResult.HitLocation, isByMine: true);

                MaskOutShipBufferIfNeeded(mineResult, Me);

                await Opponent.ReportHitBackResult(mineResult);
            }

            return true;
        }

        public void Reset(){
            if (Rules.GameKind == GameRules.GameKinds.Network)
                PhotonNetwork.RemoveCallbackTarget(this);

            IsMyTurn = false;
            IsWaiting = false;
            IsGameReady = false;
            OnTurnChanged = null;

            Opponent.Reset();
            Me.Reset();
        }

        #endregion 

        #region Network Stuff

        public void OnEvent(EventData e)
        {
            if (e.Code == MessageCodes.HitSend)
            {
                var data = JsonUtility.FromJson<HitSendContract>((string)e.CustomData);

                if (data.sender == Opponent.GameField.FieldId.ToString())
                    return; // not interested in my own messages

                // Hit request by opponent. Need to send hit response
                var hit = Me.GameField.ProcessHit(new Point(data.cell.x, data.cell.y));

                PhotonNetwork.RaiseEvent(MessageCodes.HitResultReport, JsonUtility.ToJson(new HitReportContract
                {
                    initiator = data.sender,
                    sender = Me.GameField.FieldId.ToString(),
                    cell = new GameCellContract
                    {
                        x = hit.HitLocation.X,
                        y = hit.HitLocation.Y
                    },
                    damagedShipId = hit.ShipDamaged?.ShipId.ToString(),
                    resultKind = (int)hit.ResultKind
                }), new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.DoNotCache }, SendOptions.SendReliable);
            }

            if (e.Code == MessageCodes.SpyRequest)
            {
                var data = JsonUtility.FromJson<SpyRequestContract>((string)e.CustomData);

                if (data.sender == Opponent.GameField.FieldId.ToString())
                    return; // not interested in my own messages

                // Spy request by opponent. Need to send response

                var spyResult = Me.GameField.SpyAtPoint(new Point(data.x, data.y));

                PhotonNetwork.RaiseEvent(MessageCodes.SpyResultReport, JsonUtility.ToJson(new SpyReportContract
                {
                    sender = Me.GameField.FieldId.ToString(),
                    cells = spyResult.ConvertAll(x => new GameCellContract
                    {
                        x = x.Location.X,
                        y = x.Location.Y,
                        type = (int)x.State
                    })
                }), new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.DoNotCache }, SendOptions.SendReliable);

                IsMyTurn = true;
                IsWaiting = false;

                UnityMainThreadDispatcher.Instance().Enqueue(
                    InvokeTurnChanged()
                );
            }
        }

        #endregion 

    }
}
