using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core.Localization;
using SeaWarsOnline.Core.Multiplayer;

public class MultiplayerWelcomeScreenBehavior : SwoScriptBase
{

    private GameObject _connectingMessage;

    protected override void OnStart()
    {
        LoadLocalization();

        _connectingMessage = FindInScene("ConnectingCanvas");

        _connectingMessage.SetActive(!PhotonNetwork.InLobby);
    }

    private void LoadLocalization()
    {
        FindInScene<Text>("Canvas/Title").text = LocalizationManager.Instance.GetString("MultiplayerWelcome", "Title");
        FindInScene<Text>("Canvas/NewGameGroup/NewPublicRoomButton/Text").text = LocalizationManager.Instance.GetString("MultiplayerWelcome", "Create Public Room");
        FindInScene<Text>("Canvas/NewGameGroup/NewPrivateRoomButton/Text").text = LocalizationManager.Instance.GetString("MultiplayerWelcome", "Create Private Room");
        FindInScene<Text>("Canvas/JoinGameGroup/JoinPublicRoomButton/Text").text = LocalizationManager.Instance.GetString("MultiplayerWelcome", "Join Public Room");
        FindInScene<Text>("Canvas/JoinGameGroup/JoinPrivateRoomButton/Text").text = LocalizationManager.Instance.GetString("MultiplayerWelcome", "Join Private Room");
        FindInScene<Text>("Canvas/BackButton/Text").text = "<< " + LocalizationManager.Instance.GetString("MultiplayerWelcome", "Back");

        FindInScene<Text>("Canvas/NewGameGroup/NewGameLabel").text = LocalizationManager.Instance.GetString("MultiplayerWelcome", "New Game") + ":";
        FindInScene<Text>("Canvas/JoinGameGroup/JoinGameLabel").text = LocalizationManager.Instance.GetString("MultiplayerWelcome", "Join Game") + ":";

        FindInScene<Text>("ConnectingCanvas/Text").text = LocalizationManager.Instance.GetString("MultiplayerWelcome", "Connecting") + "...";
    }

    public void OnNewPublicRoomClicked()
    {
        _gc.IsCreatingPrivateRoom = false;
        _gc.NavigateTo("Rules");
    }

    public void OnNewPrivateRoomClicked()
    {
        _gc.IsCreatingPrivateRoom = true;
        _gc.NavigateTo("Rules");
    }

    public void OnJoinPublicRoomClicked()
    {
        _gc.NavigateTo("JoinPublicRoom");
    }

    public void OnJoinPrivateRoomClicked()
    {
        _gc.NavigateTo("JoinPrivateRoom");
    }

    public void OnBackClicked()
    {
        _gc.Reset();
        _gc.NavigateTo("Welcome");
    }

    #region Networking

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby(MultiplayerClient.Instance.Lobby);
        }
    }

    public override void OnJoinedLobby()
    {
        _connectingMessage.SetActive(false);
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