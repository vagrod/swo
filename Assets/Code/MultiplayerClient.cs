using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace SeaWarsOnline.Core.Multiplayer
{
    public class MultiplayerClient : LoadBalancingClient
    {

        protected const string PhotonAppId = "<REDACTED>";

        private GameController _gc;

        private bool _isConnected;

        private static MultiplayerClient _instance;

        public static MultiplayerClient Instance => _instance ?? (_instance = new MultiplayerClient());

        private TypedLobby _lobby;

        public TypedLobby Lobby => _lobby ?? (_lobby = new TypedLobby("swo default", LobbyType.Default));

        private MultiplayerClient() { } 

        public void Initialize(GameController gc)
        {
            _gc = gc;
        }

        public void OpenConnection()
        {
            if (_isConnected)
                return;

            AppId = PhotonAppId;
            AppVersion = Application.version;

            var region = "us";

            if (_gc.Settings.Language == "Russian")
            {
                region = "ru";
            }

            if (!ConnectToRegionMaster(region))
            {
                DebugReturn(DebugLevel.ERROR, "Can't connect to: " + CurrentServerAddress);
            }
            else
            {
                Debug.Log("Connected to server");

                PhotonNetwork.LocalPlayer.NickName = $"Player-{Guid.NewGuid()}";

                PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Application.version;
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = Application.version;

                _isConnected = true;
            }
        }

        public void ReportDisconnected()
        {
            _isConnected = false;
        }

        public void Dispose()
        {
            if (_isConnected)
            {
                Disconnect();

                if (PhotonNetwork.InRoom)
                    PhotonNetwork.LeaveRoom();

                if (PhotonNetwork.InLobby)
                    PhotonNetwork.LeaveLobby();

                if (PhotonNetwork.IsConnected)
                    PhotonNetwork.Disconnect();

                _isConnected = false;
            }
        }

        public void GameLoop()
        {
            if (_isConnected)
                Service();
        }

    }
}
