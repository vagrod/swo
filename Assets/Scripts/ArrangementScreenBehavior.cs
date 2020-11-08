using System.Linq;
using System.Collections.Generic;
using Assets.Scripts;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core;
using SeaWarsOnline.Core.GameObjects;
using SeaWarsOnline.Core.Localization;
using SeaWarsOnline.Core.Multiplayer;

public class ArrangementScreenBehavior : SwoScriptBase
{

    private readonly List<DesignTimeShip> _shipsToPlace = new List<DesignTimeShip>();
    private readonly List<DesignTimeMine> _minesToPlace = new List<DesignTimeMine>();
    private readonly List<GameObject> _currentShipUiBlocks = new List<GameObject>();
    private readonly List<GameObject> _nextShipUiBlocks = new List<GameObject>();

    private BattlefieldBehavior _arrangementField;
    private int _currentShipIndex;
    private int _currentMineIndex;
    private bool _shipsDone;
    private bool _minesDone;

    private GameObject _startGameButton;
    private GameObject _arrangeButton;
    private GameObject _backButton;
    private GameObject _currentShipUi;
    private GameObject _nextShipUi;
    private Text _nextShipCount;

    protected override void OnStart(){
        var holder = FindInScene<RectTransform>("Canvas/GridHolder");
        var gridSize = Mathf.Min(holder.rect.width, holder.rect.height);

        _gc.Rules.PlayerMines.Clear();
        _gc.Rules.PlayerShips.Clear();

        LoadLocalization();

        _arrangementField = new BattlefieldBehavior(_gc);

        _currentShipIndex = 0;
        _currentMineIndex = 0;

        _arrangementField.OnCellClicked = e => ProcessCellClick(e.Cell);
        _arrangementField.IsArrangementUi = true;

        _arrangementField.Initialize(new GameField(_gc.Rules.GameFieldSize), "Canvas/GridHolder", new Vector2((holder.rect.width - gridSize) / 2f, (holder.rect.height - gridSize) / 2f), new Vector2(gridSize, gridSize));

        _currentShipUiBlocks.Add(FindInScene("Canvas/CurrentShip/Block1"));
        _currentShipUiBlocks.Add(FindInScene("Canvas/CurrentShip/Block2"));
        _currentShipUiBlocks.Add(FindInScene("Canvas/CurrentShip/Block3"));
        _currentShipUiBlocks.Add(FindInScene("Canvas/CurrentShip/Block4"));

        _nextShipUiBlocks.Add(FindInScene("Canvas/NextShip/Block1"));
        _nextShipUiBlocks.Add(FindInScene("Canvas/NextShip/Block2"));
        _nextShipUiBlocks.Add(FindInScene("Canvas/NextShip/Block3"));
        _nextShipUiBlocks.Add(FindInScene("Canvas/NextShip/Block4"));

        _currentShipUi = FindInScene("Canvas/CurrentShip");
        _nextShipUi = FindInScene("Canvas/NextShip");
        _nextShipCount = FindInScene<Text>("Canvas/NextShip/Count");

        _startGameButton = FindInScene("Canvas/StartGameButton");
        _arrangeButton = FindInScene("Canvas/AutoArrangeButton");
        _backButton = FindInScene("Canvas/BackButton");

        RenderButtonsNotReady();

        _currentShipUiBlocks.ForEach(b => b.SetActive(false));
        _nextShipUiBlocks.ForEach(b => b.SetActive(false));

        for (int i = 1; i <= _gc.Rules.CountShipsExtraLarge; i++)
            _shipsToPlace.Add(new DesignTimeShip(_arrangementField.Field, 4));

        for (int i = 1; i <= _gc.Rules.CountShipsLarge; i++)
            _shipsToPlace.Add(new DesignTimeShip(_arrangementField.Field, 3));
        
        for (int i = 1; i <= _gc.Rules.CountShipsMedium; i++)
            _shipsToPlace.Add(new DesignTimeShip(_arrangementField.Field, 2));
        
        for (int i = 1; i <= _gc.Rules.CountShipsSmall; i++)
            _shipsToPlace.Add(new DesignTimeShip(_arrangementField.Field, 1));

        if (_gc.Rules.AllowMines){
            for (int i = 1; i <= _gc.Rules.CountMines; i++)
                _minesToPlace.Add(new DesignTimeMine(_arrangementField.Field));
        }

        RefreshStatusUi(_shipsToPlace[0], null);
    }

    private void LoadLocalization(){
        FindInScene<Text>("Canvas/Title/Text").text = LocalizationManager.Instance.GetString("Arrangement", "Title");
        FindInScene<Text>("Canvas/Title/Shadow").text = LocalizationManager.Instance.GetString("Arrangement", "Title");

        FindInScene<Text>("Canvas/NextShip/Title").text = LocalizationManager.Instance.GetString("Arrangement", "Next Ship") + ":";
        FindInScene<Text>("Canvas/AutoArrangeButton/Text").text = LocalizationManager.Instance.GetString("Arrangement", "Auto Arrange");
        FindInScene<Text>("Canvas/StartGameButton/Text").text = LocalizationManager.Instance.GetString("Arrangement", "Start Game");
    }

    private void RefreshStatusUi(DesignTimeShip currentShip, DesignTimeMine currentMine){
        _currentShipUiBlocks.ForEach(b => b.SetActive(false));
        _nextShipUiBlocks.ForEach(b => b.SetActive(false));

        if (!_shipsDone){
            if (currentShip != null){
                for (int i = 0; i < currentShip.Size; i++)
                {
                    _currentShipUiBlocks[i].GetComponent<Image>().sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/Cells/CellShip.arrangement");
                    _currentShipUiBlocks[i].SetActive(true);
                }

                var nextIncomplete = _shipsToPlace.Where(x => x.ShipId != currentShip.ShipId).FirstOrDefault(x => x.IsIncomplete);

                if (nextIncomplete != null){
                    var incompleteCount = _shipsToPlace.Count(x => x.IsIncomplete && x.Size == nextIncomplete.Size && x.ShipId != currentShip.ShipId);
                    
                    _nextShipCount.text = $"{incompleteCount}x";

                    for (int i = 0; i < nextIncomplete.Size; i++)
                    {
                        _nextShipUiBlocks[i].GetComponent<Image>().sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/Cells/CellShip.arrangement");
                        _nextShipUiBlocks[i].SetActive(true);
                    }
                } else {
                    if (_gc.Rules.AllowMines){
                        var nextIncompleteMine = _minesToPlace.FirstOrDefault(x => x.IsIncomplete);

                        if (nextIncompleteMine != null)
                        {
                            var incompleteCount = _minesToPlace.Count(x => x.IsIncomplete);
                    
                            _nextShipCount.text = $"{incompleteCount}x";

                            _nextShipUiBlocks[0].SetActive(true);
                            _nextShipUiBlocks[0].GetComponent<Image>().sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/Cells/CellMine.arrangement");
                        } else {
                            // No next
                            _nextShipUi.SetActive(false);
                        }
                    } else {
                        // No next
                        _nextShipUi.SetActive(false);
                    }
                }
            }
        }
        else
        {
            if (currentMine != null){
                _currentShipUiBlocks[0].SetActive(true);
                _currentShipUiBlocks[0].GetComponent<Image>().sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/Cells/CellMine.arrangement");

                var nextIncomplete = _minesToPlace.Where(x => x.MineId != currentMine.MineId).FirstOrDefault(x => x.Cell == null);

                if (nextIncomplete != null)
                {
                    var incompleteCount = _minesToPlace.Count(x => x.Cell == null && x.MineId != currentMine.MineId);

                    _nextShipCount.text = $"{incompleteCount}x";

                    _nextShipUiBlocks[0].SetActive(true);
                    _nextShipUiBlocks[0].GetComponent<Image>().sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/Cells/CellMine.arrangement");
                } else {
                    // No next
                    _nextShipUi.SetActive(false);
                }
            }
        }
    }

    private void ProcessCellClick(FieldCellBase cell){
        if (cell.State != FieldCellBase.CellStates.Ship)
        {
            if (cell.State != FieldCellBase.CellStates.Mine && cell.State != FieldCellBase.CellStates.Available && _shipsDone && !_minesDone)
            {
                TryAddMine(cell.Location);
            } else {
                if (cell.State == FieldCellBase.CellStates.Mine)
                    TryRemoveMine(cell.Location);
                else 
                    TryAddCellToCurrentShip(cell.Location);
            }
        }
        else
        {
            TryRemoveCellFromCurrentShip(cell.Location);
        }
    }

    private void TryAddMine(Point p){
        if (_currentMineIndex >= _minesToPlace.Count) 
            return;

        var currentMine = _minesToPlace[_currentMineIndex];
        currentMine.AddCell(p);

        SelectFirstIncompleteShipOrMine();
        HighlightAvailableCells();
        CheckForCompletion();
    }

    private void TryRemoveMine(Point p) {
        var mineAtPoint = _minesToPlace.FirstOrDefault(x =>x.Cell != null && x.Location == p);

        if (mineAtPoint == null) 
            return;

        _currentMineIndex = _minesToPlace.IndexOf(mineAtPoint);

        if (_currentMineIndex >= _minesToPlace.Count || _currentMineIndex < 0) 
            return;

        mineAtPoint.Clear();

        _minesDone = false;

        SelectFirstIncompleteShipOrMine();
        HighlightAvailableCells();
        CheckForCompletion();
    }

    private void TryAddCellToCurrentShip(Point p){
        if (_currentShipIndex >= _shipsToPlace.Count) 
            return;

        if (!CanAddPieceToShip(p)) 
            return;

        var currentShip = _shipsToPlace[_currentShipIndex];

        if (currentShip.Cells.Count == currentShip.Size) 
            return;

        currentShip.AddCell(p, _gc.Rules.StraightShips);

        _shipsToPlace.ForEach(s => s.RecalculateAvailableNewCellLocation(_gc.Rules.StraightShips));

        SelectFirstIncompleteShipOrMine();
        HighlightAvailableCells();
        CheckForCompletion();
    }

    private void TryRemoveCellFromCurrentShip(Point p){
        var currentShip = _shipsToPlace.FirstOrDefault(x => x.Cells.Any(c => c.Location == p));

        if (currentShip == null) 
            return;

        if (currentShip.Cells.Count == 0) 
            return;

        if (CreatingGap(currentShip, p)) 
            return;

        currentShip.RemoveCell(p, _gc.Rules.StraightShips);

        _shipsDone = false;

        _shipsToPlace.ForEach(s => s.RecalculateAvailableNewCellLocation(_gc.Rules.StraightShips));

        SelectFirstIncompleteShipOrMine();
        HighlightAvailableCells();
        CheckForCompletion();
    }

    private void HighlightAvailableCells(){
        _arrangementField.Field.Iterate(c => {
            if (c.State == FieldCellBase.CellStates.Available)
                c.State = FieldCellBase.CellStates.Empty;
        });

        if (_currentShipIndex >= _shipsToPlace.Count)
            return;

        var currentShip = _shipsToPlace[_currentShipIndex];

        foreach(var ap in currentShip.AvailableNewCellLocations){
            if (_minesToPlace.All(m => m.Location != ap))
                _arrangementField.Field.CellAtPoint(ap).State = FieldCellBase.CellStates.Available;
        }
    }

    private void SelectFirstIncompleteShipOrMine(){
        if (!_shipsDone){
            var i = 0;

            var currentShip = _shipsToPlace[i];

            while(currentShip.Cells.Count == currentShip.Size){
                i++;

                if (i >= _shipsToPlace.Count)
                    break;

                currentShip = _shipsToPlace[i];
            }

            _currentShipIndex = i;
        } else {
            if (!_minesDone){
                var i = 0;

                var currentMine = _minesToPlace[i];

                while(currentMine.Cell != null){
                    i++;

                    if (i >= _minesToPlace.Count)
                        break;

                    currentMine = _minesToPlace[i];
                }

                _currentMineIndex = i;
            }
        }
    }

    private void CheckForCompletion(){
        _shipsDone = _shipsToPlace.All(x => x.Cells.Count == x.Size);
        _minesDone = !_gc.Rules.AllowMines || _minesToPlace.All(x => x.Cell != null);

        if (_shipsDone && _minesDone){
            if (_gc.Rules.PlayerShips.Count == 0){
                _gc.Rules.PlayerShips = _shipsToPlace.Select(x=>x as Ship).ToList(); // Copy
            }

            if (_gc.Rules.PlayerMines.Count == 0){
                _gc.Rules.PlayerMines = _minesToPlace.Select(x => x as Mine).ToList(); // Copy
            }

            RenderButtonsReady();

            _currentShipUi.SetActive(false);
            _nextShipUi.SetActive(false);

            return;
        } else {
            RenderButtonsNotReady();

            _currentShipUi.SetActive(true);
            _nextShipUi.SetActive(true);
        }

        var currentShip = _shipsDone ? null : _shipsToPlace[_currentShipIndex];
        var currentMine = _minesDone ? null : _minesToPlace[_currentMineIndex];

        RefreshStatusUi(currentShip, currentMine);
    }

    private void RenderButtonsNotReady(){
        _backButton.transform.Find("Text").GetComponent<Text>().text = "<< " + LocalizationManager.Instance.GetString("Arrangement", "Back");
        _backButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 489);

        _startGameButton.SetActive(false);
    }

    private void RenderButtonsReady(){
        _backButton.transform.Find("Text").GetComponent<Text>().text = "<<";
        _backButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 117);

        _startGameButton.SetActive(true);
    }

    private bool CanAddPieceToShip(Point p){
        if (_currentShipIndex >= _shipsToPlace.Count)
            return false;

        var ship = _shipsToPlace[_currentShipIndex];

        if (p.HasObstacleNearPoint(_arrangementField.Field, ship))
            return false;

        if (ship.Cells.Count == 0)
            return true;

        if (ship.AvailableNewCellLocations.All(x => x != p))
            return false;

        if (_minesToPlace.Any(m => m.Location == p))
            return false;

        return true;
    }

    private bool CreatingGap(Ship ship, Point p){
        if (ship.Cells.Count == 1 || ship.Cells.Count == 2)
            return false;

        var cellsAfter = ship.Cells.Where(x => x.Location != p).ToList();

        return !cellsAfter.All(cell => cellsAfter.Any(v => v.Location == new Point(cell.Location.X-1,cell.Location.Y)) ||
                                    cellsAfter.Any(v => v.Location == new Point(cell.Location.X+1,cell.Location.Y)) ||
                                    cellsAfter.Any(v => v.Location == new Point(cell.Location.X,cell.Location.Y-1)) ||
                                    cellsAfter.Any(v => v.Location == new Point(cell.Location.X,cell.Location.Y+1)));
    }

    public async void OnStartGameClick(){
        if (_gc.Rules.GameKind == GameRules.GameKinds.Network)
        {
            if (!_gc.IsInternetConnected())
            {
                MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Network Error"), OnStartGameClick, OnBackClick);
                return;
            }

            MultiplayerClient.Instance.OpenConnection();
        }

        foreach (var ship in _gc.Rules.PlayerShips){
            var o = new Ship(_gc.Me.GameField, ship.Size);

            foreach(var c in ship.Cells){
                o.Cells.Add(_gc.Me.GameField.CellAtPoint(c.Location));
            }

            _gc.Me.GameField.AddShip(o);
        }

        foreach(var mine in _gc.Rules.PlayerMines){
            var o = new Mine(_gc.Me.GameField);

            o.AddCell(mine.Location);

            _gc.Me.GameField.AddMine(o);
        }

        await _gc.StartGameSession();

        if (_gc.IsJoiningRoom)
        {
            TryJoinGameRoom();
        }
        else
        {
            _startGameButton.SetActive(false);
            _arrangeButton.SetActive(false);
            _backButton.SetActive(false);

            if (_gc.Rules.GameKind == GameRules.GameKinds.SinglePlayer)
                _gc.NavigateTo("Battlefield");
            else // Creating a room
                _gc.NavigateTo("Room");
        }
    }

    private void TryJoinGameRoom()
    {
        var player = new Hashtable();

        player["role"] = "guest";

        var gameFieldContract = GameFieldContract.FromGameField(_gc.Me.GameField);

        player["gameField"] = JsonUtility.ToJson(gameFieldContract);

        PhotonNetwork.SetPlayerCustomProperties(player);
        if (!PhotonNetwork.JoinRoom(_gc.JoiningRoomName))
        {
            MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Network Error"), TryJoinGameRoom, OnBackClick);
        }
    }

    public async void OnAutoArrangeClick(){
        FindInScene<Button>("Canvas/AutoArrangeButton").enabled = false;

        var result = await GameFieldArranger.Instance.Arrange(_gc.Rules);

        FindInScene<Button>("Canvas/AutoArrangeButton").enabled = true;

        _gc.Rules.PlayerShips.Clear();
        _gc.Rules.PlayerMines.Clear();
        _arrangementField.Field.Clear();
        _shipsToPlace.Clear();
        _minesToPlace.Clear();

        foreach(var ship in result.ShipsArranged){
            var newShip = new DesignTimeShip(_arrangementField.Field, ship.Size);

            foreach(var cell in ship.Cells){
                newShip.AddCell(cell.Location, _gc.Rules.StraightShips);
                _arrangementField.Field.CellAtPoint(cell.Location).State = FieldCellBase.CellStates.Ship;
            }

            _shipsToPlace.Add(newShip);
        }

        if (_gc.Rules.AllowMines){
            foreach(var mine in result.MinesArranged){
                var newMine = new DesignTimeMine(_arrangementField.Field);

                newMine.AddCell(mine.Location);
                _arrangementField.Field.CellAtPoint(mine.Location).State = FieldCellBase.CellStates.Mine;

                _minesToPlace.Add(newMine);
            }
        }

        CheckForCompletion();
    }

    public void OnBackClick(){
        _gc.Rules.PlayerShips.Clear();
        _gc.Rules.PlayerMines.Clear();

        if (_gc.IsJoiningRoom)
        {
            _gc.Reset(false);
            _gc.Rules.GameKind = GameRules.GameKinds.Network;

            _gc.NavigateTo("MultiplayerWelcome");
            
            return;
        }

        _gc.NavigateTo("Rules");
    }

    #region Networking

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(returnCode + ": " + message);

        if (returnCode == 32765) // GameFull
        {
            MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Room Is Full"), OnBackClick);

            return;
        }

        if (returnCode == 32758 || returnCode == 32764) // GameDoesNotExist, GameClosed
        {
            MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Room Does Not Exist"), OnBackClick);

            return;
        }

        if (returnCode == 32757) //	MaxCcuReached 
        {
            MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Max Online Players Reached"), OnBackClick);

            return;
        }

        MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Join Room Failed"), TryJoinGameRoom, OnBackClick);
    }

    public override void OnJoinedRoom()
    {
        var opponent = PhotonNetwork.PlayerList.FirstOrDefault(x => (string)x.CustomProperties["role"] == "owner");

        if (opponent != null)
        {
            var contract = JsonUtility.FromJson<GameFieldContract>((string)opponent.CustomProperties["gameField"]);

            _gc.StartMultiplayerGameSession(isMyFirstTurn: false, contract);
            _gc.NavigateTo("Battlefield");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Opponent Left The Room"), () =>
        {
            _gc.Reset();
            _gc.NavigateTo("Welcome");
        });
    }

    #endregion 

    protected override bool IsPaperDistortionEffectAllowed {
        get {
            if (Input.mousePosition.y > 75f)
                return false;

            return true;
        }
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackClick();
        }
    }

}
