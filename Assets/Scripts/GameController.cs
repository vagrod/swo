using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using SeaWarsOnline.Core;
using SeaWarsOnline.Core.PlayersInteraction;
using SeaWarsOnline.Core.Localization;
using SeaWarsOnline.Core.Multiplayer;

public class GameController : SwoScriptBase
{

    private static GameController Instance;

    private List<string> _animationsAvailable;
    private List<string> _languagesAvailable;

    public List<string> AnimationsAvailable => _animationsAvailable ?? (_animationsAvailable =  new List<string>(new[]{
        "None",
        "Flip",
        "Wave"
    }));

    public List<string> LanguagesAvailable => _languagesAvailable ?? (_languagesAvailable =  new List<string>(new[]{
        "English",
        "Russian"
    }));

    public Rect ScreenSize { get; set; }
    public Rect PortraitScreenSize { get; set; }

    public GameRules Rules { get; private set; }

    public AppSettings Settings { get; private set; }

    public bool IsJoiningRoom { get; set; }
    public bool IsCreatingPrivateRoom { get; set; }
    public string JoiningRoomName { get; set; }

    public string SkinName => Settings.SkinName;

    public MePlayer Me { get; private set; }
    public PlayerBase Opponent { get; private set; }

    public BattleController BattleController{get; private set;}

    public GameController() {
        Reset();
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        MultiplayerClient.Instance.GameLoop();
    }

    public bool InitializeMePlayer(){
        if (Rules.GameFieldSize <= 0)
            return false;

        Me = new MePlayer(new GameField(Rules.GameFieldSize));

        return true;
    }

    void Start(){
        Settings = new AppSettings();
        Settings.Load();

        LocalizationManager.Instance.Language = Settings.Language;

        UpdateResolution();
    }

    public void UpdateResolution(){
        // Loader scene always starts in portrait
        if (PortraitScreenSize == default(Rect)) {
            var side1 = Display.main.systemWidth;

            if (side1 < 1)
                side1 = Screen.currentResolution.width;

            var side2 = Display.main.systemHeight;

            if (side2 < 1)
                side2 = Screen.currentResolution.height;

            PortraitScreenSize = new Rect(0, 0, Mathf.Min(side1, side2), Mathf.Max(side1, side2));
        }

        // Unity bug: screen size not changing after orientation change
        if(Settings.IsPortraitMode)
            ScreenSize = new Rect(0, 0, Mathf.Min(PortraitScreenSize.width, PortraitScreenSize.height), Mathf.Max(PortraitScreenSize.width, PortraitScreenSize.height));
        else 
            ScreenSize = new Rect(0, 0, Mathf.Max(PortraitScreenSize.width, PortraitScreenSize.height), Mathf.Min(PortraitScreenSize.width, PortraitScreenSize.height));
    }

    public void Reset(bool closeMultiplayer = true)
    {
        if (closeMultiplayer)
            MultiplayerClient.Instance.Dispose();

        IsCreatingPrivateRoom = false;
        IsJoiningRoom = false;
        JoiningRoomName = null;

        BattleController?.Reset();

        Rules = GameRules.Default();
        Me = null;
        Opponent = null;
        BattleController = null;
    }

    public async Task StartGameSession(){
        if (Rules.GameKind == GameRules.GameKinds.SinglePlayer)
            await StartOfflineGameSession();

        if (Rules.GameKind == GameRules.GameKinds.Network)
        {
            MultiplayerClient.Instance.Initialize(this);
            MultiplayerClient.Instance.OpenConnection();
        }
    }

    public void NavigateTo(string sceneName){
        if(!Settings.IsPortraitMode)
            SceneManager.LoadScene($"{sceneName}.{SkinName}.tablet");
        else
            SceneManager.LoadScene($"{sceneName}.{SkinName}");
    }

    public void StartMultiplayerGameSession(bool isMyFirstTurn, GameFieldContract opponentFieldData)
    {
        Opponent = new Assets.Code.PlayersInteraction.NetworkPlayer(Rules, opponentFieldData);

        BattleController = new BattleController(Rules, Me, Opponent, isMyFirstTurn);
    }

    private async Task StartOfflineGameSession()
    {
        Opponent = new AIPlayer(new GameField(Rules.GameFieldSize), Rules);

        Opponent.GameField.IsUnderConstruction = true;

        var opponentArrangement = await GameFieldArranger.Instance.Arrange(Rules);

        foreach (var ship in opponentArrangement.ShipsArranged)
        {
            var o = new SeaWarsOnline.Core.GameObjects.Ship(Opponent.GameField, ship.Size);

            foreach (var c in ship.Cells)
            {
                o.Cells.Add(Opponent.GameField.CellAtPoint(c.Location));
            }

            Opponent.GameField.AddShip(o);
        }

        if (Rules.AllowMines)
        {
            foreach (var mine in opponentArrangement.MinesArranged)
            {
                var o = new SeaWarsOnline.Core.GameObjects.Mine(Opponent.GameField);

                o.AddCell(mine.Location);

                //UnityEngine.Debug.Log($"Opponent's mine at [{(char)(65+ mine.Location.X)}{mine.Location.Y + 1}]");

                Opponent.GameField.AddMine(o);
            }
        }

        BattleController = new BattleController(Rules, Me, Opponent, true); // offline game always prioritize player

        Opponent.GameField.IsUnderConstruction = false;
    }

    public bool IsInternetConnected()
    {
        var req = (HttpWebRequest)WebRequest.Create("http://google.com");

        req.Timeout = 1000;

        try
        {
            using (var resp = (HttpWebResponse)req.GetResponse())
            {
                var isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;

                if (isSuccess)
                {
                    using (var reader =
                        new StreamReader(resp.GetResponseStream() ?? throw new Exception("Http Response Failed")))
                    {
                        var cs = new char[4];

                        reader.Read(cs, 0, cs.Length);
                    }
                }
                else
                    throw new Exception("Http StatusCode does not indicate success");
            }
        }
        catch
        {
            MultiplayerClient.Instance.ReportDisconnected();

            return false;
        }

        return true;
    }

}