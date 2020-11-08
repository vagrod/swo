using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using SeaWarsOnline.Core.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class RoomsListBehavior : SwoScriptBase
    {

        private GameObject _roomItemTemplate;
        private RectTransform _contentTransform;

        private Color _highlightedColor;
        private Color _normalColor;

        private bool _ignoreCheckedChanged;
        private bool _needLobbyReenter;

        public RoomInfo SelectedRoom { get; private set; }
        public int RoomsCount { get; private set; }

        public Action OnRoomsListChanged { get; set; }
        public Action OnSelectedRoomChanged { get; set; }

        private List<GameObject> Items { get; } = new List<GameObject>();
        private List<RoomInfo> ItemsData { get; } = new List<RoomInfo>();

        protected override void OnStart()
        {
            _roomItemTemplate = FindInScene("Canvas/RoomsList/RoomItemTemplate");
            _roomItemTemplate.SetActive(false);

            _normalColor = FindInScene<Image>("Canvas/RoomsList/RoomItemTemplate/Item Background").color;
            _highlightedColor = _roomItemTemplate.GetComponent<Toggle>().colors.disabledColor;

            _contentTransform = FindInScene<RectTransform>("Canvas/RoomsList/Viewport/Content");

            RefreshRooms();
        }

        #region Networking

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Items.ForEach(Destroy);

            ItemsData.Clear();
            Items.Clear();

            var i = 0;
            var margin = new Vector2(10, 5);
            foreach (var room in roomList)
            {
                if (room.CustomProperties.Count == 0)
                    continue; // "broken" room

                var isPrivate = (int)room.CustomProperties["isPrivate"] == 1;

                if (isPrivate)
                    continue; // private room

                var item = Instantiate(_roomItemTemplate);
                var t = item.GetComponent<RectTransform>();

                item.name = $"item-{i + 1}";

                t.SetParent(_contentTransform);
                t.pivot = new Vector2(0, 1);
                t.anchoredPosition = new Vector2(margin.x, -i * t.rect.height - margin.y);
                t.localScale = new Vector3(1, 1, 1);
                t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _contentTransform.rect.width - margin.x * 2);

                var fieldSize = room.CustomProperties["fieldSize"];
                var allowMines = room.CustomProperties["allowMines"].Equals(1);
                var allowSpyGlass = room.CustomProperties["allowSpyGlass"].Equals(1);
                var straightShips = room.CustomProperties["straightShips"].Equals(1);
                var oneByOneTurns = room.CustomProperties["oneByOneTurns"].Equals(1);
                var density = (int) room.CustomProperties["densityProfile"];

                t.Find("Item Background").GetComponent<Image>().color = _normalColor;
                t.Find("RoomId").transform.Find("Value").GetComponent<Text>().text = $"{room.Name.ToUpper()}";
                t.Find("RoomSize").GetComponent<Text>().text = $"{fieldSize}x{fieldSize}";
                t.Find("Mines").gameObject.SetActive(allowMines);
                t.Find("SpyGlass").gameObject.SetActive(allowSpyGlass);
                t.Find("ExtraTurn").gameObject.SetActive(!oneByOneTurns);
                t.Find("ShipsShape").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/UI/{(straightShips ? "Shape-Straight" : "Shape-Any")}");

                if (density == 0)
                    t.Find("Density").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/UI/Density-High");
                if (density == 1)
                    t.Find("Density").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/UI/Density-Medium");
                if (density == 2)
                    t.Find("Density").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/UI/Density-Low");
                if (density == 3)
                    t.Find("Density").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/UI/Density-Single");

                var toggle = t.GetComponent<Toggle>();

                toggle.isOn = false;
                toggle.onValueChanged.AddListener(b => OnRoomCheckedChanged(item));

                item.SetActive(true);

                Items.Add(item);
                ItemsData.Add(room);

                i++;
            }

            SelectedRoom = null;
            RoomsCount = Items.Count;

            _contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (_roomItemTemplate.GetComponent<RectTransform>().rect.height + margin.y) * i);

            OnRoomsListChanged?.Invoke();
            OnSelectedRoomChanged?.Invoke();
        }

        public override void OnLeftLobby()
        {
            if (_needLobbyReenter)
                PhotonNetwork.JoinLobby(MultiplayerClient.Instance.Lobby);

            _needLobbyReenter = false;
        }

        #endregion 

        public void RefreshRooms()
        {
            SelectedRoom = null;
            OnSelectedRoomChanged?.Invoke();

            _needLobbyReenter = true;

            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }
        }

        private void OnRoomCheckedChanged(GameObject sender)
        {
            if (_ignoreCheckedChanged)
                return;

            var toggle = sender.GetComponent<Toggle>();

            if (toggle.isOn)
            {
                SelectedRoom = ItemsData[Items.IndexOf(sender)];
                OnSelectedRoomChanged?.Invoke();

                _ignoreCheckedChanged = true;

                foreach (var item in Items)
                {
                    sender.transform.Find("Item Background").GetComponent<Image>().color = _normalColor;
                    item.GetComponent<Toggle>().isOn = false;
                }

                toggle.isOn = true;

                sender.transform.Find("Item Background").GetComponent<Image>().color = _highlightedColor;

                _ignoreCheckedChanged = false;
            }
            else
            {
                sender.transform.Find("Item Background").GetComponent<Image>().color = _normalColor;

                SelectedRoom = null;
                OnSelectedRoomChanged?.Invoke();
            }
        }

    }
}
