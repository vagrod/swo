using SeaWarsOnline.Core;
using SeaWarsOnline.Core.Localization;
using SeaWarsOnline.Core.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class JoinPublicRoomScreenBehavior : SwoScriptBase
    {

        private RoomsListBehavior _roomsList;
        private GameObject _joinRoomButton;

        protected override void OnStart()
        {
            LoadLocalization();

            _roomsList = FindInScene<RoomsListBehavior>("Canvas/RoomsList");
            _joinRoomButton = FindInScene("Canvas/JoinRoomButton");

            _joinRoomButton.GetComponent<Button>().interactable = false;

            _roomsList.OnRoomsListChanged = OnRoomsListChanged;
            _roomsList.OnSelectedRoomChanged = OnSelectedRoomChanged;

            _roomsList.RefreshRooms();
        }

        private void LoadLocalization()
        {
            FindInScene<Text>("Canvas/Title").text = LocalizationManager.Instance.GetString("JoinPublicRoom", "Title");
            FindInScene<Text>("Canvas/JoinRoomButton/Text").text = LocalizationManager.Instance.GetString("JoinPublicRoom", "Join");
        }

        private void OnRoomsListChanged()
        {

        }

        private void OnSelectedRoomChanged()
        {
            _joinRoomButton.GetComponent<Button>().interactable = _roomsList.SelectedRoom != null;
        }

        public void OnBackClicked()
        {
            _gc.Reset(false);
            _gc.Rules.GameKind = GameRules.GameKinds.Network;
            _gc.NavigateTo("MultiplayerWelcome");
        }

        public void OnJoinRoomClicked()
        {
            if (_roomsList.SelectedRoom == null)
                return;

            var noInternet = !_gc.IsInternetConnected();

            if (noInternet)
            {
                MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Network Error"), OnJoinRoomClicked, OnBackClicked);
                return;
            }

            MultiplayerClient.Instance.OpenConnection();

            _gc.IsJoiningRoom = true;
            _gc.JoiningRoomName = _roomsList.SelectedRoom.Name;
            _gc.Rules.GameKind = GameRules.GameKinds.Network;
            _gc.Rules.ReadFromRoom(_roomsList.SelectedRoom);

            if (!_gc.InitializeMePlayer())
                return;

            _gc.NavigateTo("Arrangement");
        }

        public void OnRefreshRoomsClicked()
        {
            _roomsList.RefreshRooms();
        }

        protected override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackClicked();
            }
        }

    }
}
