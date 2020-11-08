using System;
using Assets.Scripts;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using SeaWarsOnline.Core.Localization;
using SeaWarsOnline.Core.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class RoomScreenBehavior : SwoScriptBase
{

    private GameObject _roomInfo;

    private string RoomId { get; set; }

    protected override void OnStart()
    {
        LoadLocalization();
        TryCreateRoom();
    }

    private void LoadLocalization()
    {
        _roomInfo = FindInScene("Canvas/Info/RoomId");

        FindInScene<Text>("Canvas/Title").text = LocalizationManager.Instance.GetString("Room", "Title");
        FindInScene<Text>("Canvas/BackButton/Text").text = "<< " + LocalizationManager.Instance.GetString("Room", "Back");
        FindInScene<Text>("Canvas/Info/Hint").text = LocalizationManager.Instance.GetString("Room", "Wait Message");
        FindInScene<Text>("Canvas/Info/RoomId/Label").text = LocalizationManager.Instance.GetString("Room", "Room ID Is") + ":";
        FindInScene<Text>("Canvas/Info/RoomId/CopyToClipboardButton/Text").text = LocalizationManager.Instance.GetString("Room", "Copy To Clipboard");

        if (_gc.IsCreatingPrivateRoom)
            FindInScene<Text>("Canvas/Info/PrivateRoomHint").text = LocalizationManager.Instance.GetString("Room", "Private Room Hint");
        else
            FindInScene("Canvas/Info/PrivateRoomHint").SetActive(false);

        _roomInfo.SetActive(false);
    }

    private void TryCreateRoom()
    {
        var noInternet = !_gc.IsInternetConnected();

        if (noInternet)
        {
            MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Network Error"), TryCreateRoom, OnBackClicked);
            return;
        }

        MultiplayerClient.Instance.OpenConnection();

        var options = new RoomOptions();
        var rules = new Hashtable();

        rules["fieldSize"] = _gc.Rules.GameFieldSize;
        rules["allowMines"] = _gc.Rules.AllowMines?1:0;
        rules["allowSpyGlass"] = _gc.Rules.AllowSpyGlass?1:0;
        rules["straightShips"] = _gc.Rules.StraightShips?1:0;
        rules["oneByOneTurns"] = _gc.Rules.ChangeTurnEachTime?1:0;
        rules["countMines"] = _gc.Rules.CountMines;
        rules["countSmall"] = _gc.Rules.CountShipsSmall;
        rules["countMedium"] = _gc.Rules.CountShipsMedium;
        rules["countLarge"] = _gc.Rules.CountShipsLarge;
        rules["countExtraLarge"] = _gc.Rules.CountShipsExtraLarge;
        rules["densityProfile"] = _gc.Rules.DensityProfile;
        rules["isPrivate"] = _gc.IsCreatingPrivateRoom?1:0;

        options.CustomRoomPropertiesForLobby = new[]
        {
            "fieldSize", 
            "allowMines", 
            "allowSpyGlass", 
            "straightShips", 
            "oneByOneTurns", 
            "countMines", 
            "countSmall",
            "countMedium", 
            "countLarge", 
            "countExtraLarge", 
            "densityProfile",
            "isPrivate"
        };
        options.CustomRoomProperties = rules;
        options.MaxPlayers = 2;
        options.IsVisible = true;
        options.IsOpen = true;

        var player = new Hashtable();

        player["role"] = "owner";

        var gameFieldContract = GameFieldContract.FromGameField(_gc.Me.GameField);

        player["gameField"] = JsonUtility.ToJson(gameFieldContract);

        RoomId = Guid.NewGuid().ToString().Substring(0, 5);

        PhotonNetwork.SetPlayerCustomProperties(player);

        var success = PhotonNetwork.CreateRoom(RoomId, options, MultiplayerClient.Instance.Lobby);
        
        if (success)
        {
            _roomInfo.SetActive(true);

            FindInScene<Text>("Canvas/Info/RoomId/Value").text = RoomId.ToUpper();
        }
        else
        {
            MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Network Error"), TryCreateRoom, OnBackClicked);
        }
    }

    public void OnBackClicked()
    {
        _gc.Reset();
        _gc.NavigateTo("Welcome");
    }

    public void OnCopyToClipboardClicked()
    {
        if (string.IsNullOrEmpty(RoomId))
            return;

        ClipboardManager.SetText(RoomId.ToUpper());

        MessageBox.Show(LocalizationManager.Instance.GetString("Room", "ID Copied"));
    }

    #region Networking

    public override void OnPlayerEnteredRoom(Player opponent)
    {
        var contract = JsonUtility.FromJson<GameFieldContract>((string) opponent.CustomProperties["gameField"]);

        _gc.StartMultiplayerGameSession(isMyFirstTurn:true, contract);
        _gc.NavigateTo("Battlefield");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Opponent Left The Room"), () =>
        {
            _gc.Reset();
            _gc.NavigateTo("Welcome");
        });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (returnCode == 32757) //	MaxCcuReached 
        {
            MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Max Online Players Reached"), TryCreateRoom, OnBackClicked);

            return;
        }

        MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Create Room Failed"), TryCreateRoom, OnBackClicked);
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
