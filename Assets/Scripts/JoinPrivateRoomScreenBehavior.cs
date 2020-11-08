using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using SeaWarsOnline.Core;
using SeaWarsOnline.Core.Localization;
using SeaWarsOnline.Core.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class JoinPrivateRoomScreenBehavior : SwoScriptBase
    {

        private readonly List<RoomInfo> _roomsAvailable = new List<RoomInfo>();

        private Button _joinButton;
        private bool _needLobbyReenter;

        private string RoomId { get; set; }

        protected override void OnStart()
        {
            _joinButton = FindInScene<Button>("Canvas/JoinRoomButton");

            LoadLocalization();

            RefreshRooms();
        }

        private void LoadLocalization()
        {
            FindInScene<Text>("Canvas/Title").text = LocalizationManager.Instance.GetString("JoinPrivateRoom", "Title");
            FindInScene<Text>("Canvas/JoinRoomButton/Text").text = LocalizationManager.Instance.GetString("JoinPrivateRoom", "Join");
            FindInScene<Text>("Canvas/RoomId/Hint").text = LocalizationManager.Instance.GetString("JoinPrivateRoom", "Enter Room ID") + ":";
        }

        private void RefreshRooms()
        {
            _joinButton.interactable = false;
            _needLobbyReenter = true;

            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }
        }

        public void OnBackClicked()
        {
            _gc.Reset(false);
            _gc.Rules.GameKind = GameRules.GameKinds.Network;
            _gc.NavigateTo("MultiplayerWelcome");
        }

        public void OnJoinClicked()
        {
            var noInternet = !_gc.IsInternetConnected();

            if (noInternet)
            {
                MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Network Error"), OnJoinClicked, OnBackClicked);
                return;
            }

            MultiplayerClient.Instance.OpenConnection();

            if (string.IsNullOrEmpty(RoomId) || RoomId.Length != 5)
                return;

            var room = _roomsAvailable.FirstOrDefault(x => x.Name.ToLower() == RoomId.ToLower());

            if (room == null)
            {
                MessageBox.Show(LocalizationManager.Instance.GetString("JoinPrivateRoom", "Room Does Not Exist").Replace("%s", RoomId), OnJoinClicked, null);
                return;
            }

            _gc.IsJoiningRoom = true;
            _gc.JoiningRoomName = room.Name;
            _gc.Rules.GameKind = GameRules.GameKinds.Network;
            _gc.Rules.ReadFromRoom(room);

            if (!_gc.InitializeMePlayer())
                return;

            _gc.NavigateTo("Arrangement");
        }

        public void OnKeypadKeyClick(string s)
        {
            if (s.ToLower() == "del")
            {
                if (string.IsNullOrEmpty(RoomId) || RoomId.Length == 0)
                    return;

                RoomId = RoomId.Substring(0, RoomId.Length - 1);
            }
            else
            {
                if (!string.IsNullOrEmpty(RoomId) && RoomId.Length == 5)
                    return;

                RoomId += s;
            }

            FindInScene<Text>("Canvas/RoomId/Value").text = RoomId;

            Validate();
        }

        private void Validate(){
            _joinButton.interactable = _roomsAvailable.Count > 0 && RoomId?.Length == 5;
        }

        #region Networking

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            _roomsAvailable.Clear();

            _roomsAvailable.AddRange(roomList);

            Validate();
        }

        public override void OnLeftLobby()
        {
            if (_needLobbyReenter)
                PhotonNetwork.JoinLobby(MultiplayerClient.Instance.Lobby);

            _needLobbyReenter = false;
        }

        #endregion

        protected override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackClicked();
            }
        }

    }
}
