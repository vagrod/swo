using System.Linq;
using System.Collections.Generic;
using Assets.Scripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core;
using SeaWarsOnline.Core.Localization;
using SeaWarsOnline.Core.Multiplayer;

public class BattlefieldScreenBehavior : SwoScriptBase
{

    private const float ScrollingDelayAfterTurn = 0.8f;

    private Animator _battlefieldScrollAnimator;
    private Animator _parallaxScrollAnimator;

    private BattlefieldBehavior _myField;
    private BattlefieldBehavior _opponentField;
    private GameOverPopupBehavior _gameOverScreen;
    private QuickSettingsPopupBehavior _quickSettings;

    private GameObject _spyHint;
    private GameObject _spyButton;

    private RectTransform _battlefieldTransform;
    private Transform _backgroundTransform;
    private Vector3 _zeroBackgroundPosition;

    private GameField _opponentAsISee;

    private int _slideDirection = 1;
    private float _slideDelay;
    private float _slideDelayCounter;
    private bool _isSpyGlassActive;

    private RectTransform HolderMe {get; set;}
    private RectTransform HolderOpponent {get; set;}

    protected override void OnStart() {
        LoadLocalization();

        _battlefieldScrollAnimator = FindInScene<Animator>("Canvas/InnerArea");

        _gameOverScreen = FindInScene<GameOverPopupBehavior>("CanvasGameOver");
        _quickSettings = FindInScene<QuickSettingsPopupBehavior>("CanvasQuickSettings");

        _gameOverScreen.Initialize();
        _quickSettings.Initialize();

        _spyHint = FindInScene("Canvas/SpyGlass/SpyGlassHint");
        _spyButton = FindInScene("Canvas/SpyGlass/SpyGlassButton");

        if (_gc.Settings.IsPortraitMode)
        {
            FindInScene<SwipeDetector>("Canvas").OnSwipeLeftToRight = () => { InvokeSlideToPlayer(); };
            FindInScene<SwipeDetector>("Canvas").OnSwipeRightToLeft = () => { InvokeSlideToOpponent(); };
        }

        _spyHint.SetActive(false);

        if (_gc.Rules.AllowSpyGlass == false)
            _spyButton.SetActive(false);

        _battlefieldTransform = FindInScene<RectTransform>("Canvas/InnerArea");
        _backgroundTransform = FindInScene<Transform>("Background");
        _zeroBackgroundPosition = _backgroundTransform.position;

        ConstructFields();

        _gc.BattleController.DeclareReady();
    }

    private void LoadLocalization(){
        FindInScene<Text>("Canvas/InnerArea/HeaderMyField").text = LocalizationManager.Instance.GetString("Battlefield", "My Field");
        FindInScene<Text>("Canvas/InnerArea/HeaderOpponentField").text = LocalizationManager.Instance.GetString("Battlefield", "Opponent Field");
        FindInScene<Text>("Canvas/InnerArea/GoToMyFieldButton/Text").text = LocalizationManager.Instance.GetString("Battlefield", "To My Field");
        FindInScene<Text>("Canvas/InnerArea/GoToOpponentButton/Text").text = LocalizationManager.Instance.GetString("Battlefield", "To Opponent");

        FindInScene<Text>("Canvas/ExitButton/Text").text = LocalizationManager.Instance.GetString("Battlefield", "Exit");
        FindInScene<Text>("Canvas/SpyGlass/SpyGlassHint").text = LocalizationManager.Instance.GetString("Battlefield", "SpyGlass Hint");
        FindInScene<Text>("Canvas/SpyGlass/SpyGlassButton/Text").text = LocalizationManager.Instance.GetString("Battlefield", "Use SpyGlass") + " >>";
    }

    private void ConstructFields()
    {
        HolderMe = FindInScene<RectTransform>("Canvas/InnerArea/GridHolderMe");
        HolderOpponent = FindInScene<RectTransform>("Canvas/InnerArea/GridHolderOpponent");

        var gridSize1 = Mathf.Min(HolderMe.rect.width, HolderMe.rect.height);
        var gridSize2 = Mathf.Min(HolderOpponent.rect.width, HolderOpponent.rect.height);

        Vector2 margin1, margin2;

        if (!_gc.Settings.IsPortraitMode)
        {
            margin1 = new Vector2(0, 0); // align left
            margin2 = new Vector2(HolderOpponent.rect.width - gridSize2, HolderOpponent.rect.height - gridSize2) / 2f; // align right
        }else {
            margin1 = new Vector2(0, 0);
            margin2 = new Vector2(0, 0);
        }

        _myField = new BattlefieldBehavior(_gc);
        _myField.CellsOverlap = 4;
        _myField.ShowHeader = true;
        _myField.Initialize(_gc.Me.GameField, "Canvas/InnerArea/GridHolderMe", margin1, new Vector2(gridSize1, gridSize1));

        _opponentAsISee = new GameField(_gc.Rules.GameFieldSize);

        _opponentField = new BattlefieldBehavior(_gc);
        _opponentField.CellsOverlap = 4;
        _opponentField.ShowHeader = true;
        _opponentField.Initialize(_opponentAsISee, "Canvas/InnerArea/GridHolderOpponent", margin2, new Vector2(gridSize2, gridSize2));
        _opponentField.OnCellClicked = OnOpponentCellClicked;

        _gc.Opponent.GameField.CellStateChanged += OpponentFieldOnCellStateChanged;

        _gc.Opponent.GameField.CellStateChanging += OpponentFieldOnCellStateChanging;
        _gc.Me.GameField.CellStateChanging += MyFieldOnCellStateChanging;

        _gc.BattleController.OnTurnChanged = BattleOnTurnChanged;

        RefreshStatus();
    }

    private void RefreshStatus(){
        if(_gc.BattleController.IsGameOver)
            return;

        //UnityEngine.Debug.Log("Status UI refresh");
        FindInScene<Text>("Canvas/StatusText").text = $"{(_gc.BattleController.IsMyTurn ? LocalizationManager.Instance.GetString("Battlefield", "Your Turn") : LocalizationManager.Instance.GetString("Battlefield", "Opponent Turn"))}";
        FindInScene<Text>("Canvas/InnerArea/StatusMyField").text = LocalizationManager.Instance.GetString("Battlefield", "Ships Destroyed").Replace("%m", _gc.Me.GameField.Ships.Count(x => x.IsDestroyed).ToString()).Replace("%n", _gc.Me.GameField.Ships.Count.ToString());
        FindInScene<Text>("Canvas/InnerArea/StatusOpponentField").text = LocalizationManager.Instance.GetString("Battlefield", "Ships Destroyed").Replace("%m", _gc.Opponent.GameField.Ships.Count(x => x.IsDestroyed).ToString()).Replace("%n", _gc.Opponent.GameField.Ships.Count.ToString());
    }

    private async void OnOpponentCellClicked(BattlefieldBehavior.CellClickedEventArgs e)
    {
        if (_gc.BattleController.IsGameOver)
            return;

        //UnityEngine.Debug.Log($"Opponent view field is {_opponentAsISee.FieldId}");
        //UnityEngine.Debug.Log($"Opponent field is {_gc.Opponent.GameField.FieldId}");
        //UnityEngine.Debug.Log($"Player field is {_gc.Me.GameField.FieldId}");

        if (_gc.Rules.GameKind == GameRules.GameKinds.Network)
        {
            MultiplayerClient.Instance.OpenConnection();
        }

        if (_gc.BattleController.IsMyTurn && !_gc.BattleController.IsWaiting){
            if (_isSpyGlassActive)
            {
                _spyHint.SetActive(false);
                _isSpyGlassActive = false;

                ScannerWaveAnimation.AttachTo(e.CellTransform, async () => {
                    var spyResult = await _gc.BattleController.SpyAtPoint(e.Location);

                    if (spyResult != null && spyResult.Count > 0)
                    {
                        foreach (var info in spyResult)
                        {
                            var c = _opponentAsISee.CellAtPoint(info.Location);
                            if (info.State == FieldCellBase.CellStates.Ship)
                                c.State = FieldCellBase.CellStates.ForeignShipPart;
                            if (info.State == FieldCellBase.CellStates.Mine)
                                c.State = FieldCellBase.CellStates.ForeignMine;

                            _opponentField.View.HighlightAtPoint(info.Location.X, info.Location.Y);
                        }
                    }
                });
            }
            else
            {
                var isProcessed = await _gc.BattleController.ProcessPlayerTurn(e.Location);

                if (_gc.Rules.GameKind == GameRules.GameKinds.Network && !isProcessed)
                {
                    MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Multiplayer Game Cannot Continue"), ExitBattle);
                    
                    return;
                }
            }

            RefreshStatus();
        }
    }

    private void OpponentFieldOnCellStateChanged(object sender, CellStateEventArgs e)
    {
        if (_gc?.BattleController == null)
            return;

        if (!_gc.BattleController.IsGameReady)
            return;

        // Mirror changes to UI
        //UnityEngine.Debug.Log($"Opponent cell state changed. Mirroring to UI new state {e.NewState}");

        _opponentAsISee.CellAtPoint(e.CellLocation).State = e.NewState;
    }

    private void OpponentFieldOnCellStateChanging(object sender, CellStateEventArgs e)
    {
        if (_gc?.BattleController == null)
            return;

        if (!_gc.BattleController.IsGameReady)
            return;

        var cell = _opponentField.View.GetAtPoint<RectTransform>(e.CellLocation.X, e.CellLocation.Y);

        if (_gc.Settings.CellAnimation == "Flip")
            StateChangeAnimationFlip.AttachTo(cell, Resources.Load<Sprite>($"Skins/{_gc.SkinName}/Cells/{_opponentField.Field.CellAtPoint(e.CellLocation).UsedImageResource}"), _opponentField.View.GetSpriteForState(e.NewState));

        if (_gc.Settings.CellAnimation == "Wave")
            StateChangeAnimationWave.AttachTo(cell);
    }

    private void MyFieldOnCellStateChanging(object sender, CellStateEventArgs e)
    {
        if (_gc?.BattleController == null)
            return;
            
        if (!_gc.BattleController.IsGameReady)
            return;

        var cell = _myField.View.GetAtPoint<RectTransform>(e.CellLocation.X, e.CellLocation.Y);

        if (_gc.Settings.CellAnimation == "Flip")
            StateChangeAnimationFlip.AttachTo(cell, Resources.Load<Sprite>($"Skins/{_gc.SkinName}/Cells/{_myField.Field.CellAtPoint(e.CellLocation).UsedImageResource}"), _myField.View.GetSpriteForState(e.NewState));

        if (_gc.Settings.CellAnimation == "Wave")
            StateChangeAnimationWave.AttachTo(cell);
    }

    private void BattleOnTurnChanged(){
        if (_gc.Settings.AutoScroll)
        {
            if (!_gc.BattleController.IsMyTurn)
                InvokeSlideToPlayer(ScrollingDelayAfterTurn);
            else 
                InvokeSlideToOpponent(ScrollingDelayAfterTurn*1.5f);
        }

        CheckForGameOver();
        RefreshStatus();    
    }

    private void CheckForGameOver(){
        if (_gc.BattleController.IsGameOver){
            var cellsIntact = new List<FieldCellBase>();

            _gc.Opponent.GameField.Iterate(cell => {
                if (cell.State == FieldCellBase.CellStates.Ship || cell.State == FieldCellBase.CellStates.Mine){
                    cellsIntact.Add(cell);
                }
            });

            foreach(var cell in cellsIntact){
                _opponentAsISee.CellAtPoint(cell.Location).State = cell.State;
            }

            FindInScene<Text>("Canvas/StatusText").text = null;
            FindInScene("Canvas/InnerArea/StatusMyField").SetActive(false);
            FindInScene("Canvas/InnerArea/StatusOpponentField").SetActive(false);
            
            _spyButton.SetActive(false);
            _spyHint.SetActive(false);

            _gameOverScreen.Show();
        }
    }
    
    public void GoToOpponentButtonOnClick(){
        InvokeSlideToOpponent();
    }

    public void GoToPlayerButtonOnClick(){
        InvokeSlideToPlayer();
    }

    public void SpyGlassButtonOnClick(){
        _isSpyGlassActive = true;

        _spyButton.SetActive(false);
        _spyHint.SetActive(true);
    }

    public void ExitBattleButtonOnClick(){
        if (_gc.BattleController.IsGameOver)
            ExitBattle();
        else
            MessageBox.Show(LocalizationManager.Instance.GetString("Battlefield", "Exit Battle Prompt"), ExitBattle, showCancel: true);
    }

    private void ExitBattle(){
        if (_gc.Rules.GameKind == GameRules.GameKinds.Network)
        {
            _gc.Reset();
            _gc.NavigateTo("Welcome");

            return;
        }

        _gc.InitializeMePlayer();
        _gc.NavigateTo("Arrangement");
    }

    public void QuickSettingsButtonOnClick(){
        _quickSettings.Show();
    }

    private void InvokeSlideToOpponent(float delay = 0f){
        if (_gc.Settings.IsPortraitMode)
        {
            if (_parallaxScrollAnimator == null)
                _parallaxScrollAnimator = FindInScene("Background").GetComponent<Animator>();

            _slideDelayCounter = 0f;
            _slideDelay = delay;

            _slideDirection = 1;

            if (delay <= 0f)
            {
                _battlefieldScrollAnimator.ResetTrigger("ScrollToPlayer");
                _battlefieldScrollAnimator.SetTrigger("ScrollToOpponent");

                _parallaxScrollAnimator.ResetTrigger("ScrollToPlayer");
                _parallaxScrollAnimator.SetTrigger("ScrollToOpponent");
            }
        }
    }

    private void InvokeSlideToPlayer(float delay = 0f){
        if (_gc.Settings.IsPortraitMode)
        {
            if (_parallaxScrollAnimator == null)
                _parallaxScrollAnimator = FindInScene("Background").GetComponent<Animator>();

            _slideDelayCounter = 0f;
            _slideDelay = delay;

            _slideDirection = -1;

            if (delay <= 0f)
            {
                _battlefieldScrollAnimator.ResetTrigger("ScrollToOpponent");
                _battlefieldScrollAnimator.SetTrigger("ScrollToPlayer");

                _parallaxScrollAnimator.ResetTrigger("ScrollToOpponent");
                _parallaxScrollAnimator.SetTrigger("ScrollToPlayer");
            }
        }
    }

    void OnDestroy(){
        //UnityEngine.Debug.Log("Screen Cleanup");

        // Clean Up
        if (_opponentField?.Field != null)
            _opponentField.Field.CellStateChanged -= OpponentFieldOnCellStateChanged;

        _opponentField.OnCellClicked = null;
        _myField.Destroy();
        _opponentField.Destroy();
        _opponentAsISee.Clear();
    }

    protected override bool IsPaperDistortionEffectAllowed {
        get {
            if (Input.mousePosition.y > 75f)
                return false;

            return true;
        }
    }

    protected override void OnUpdate(){
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_gameOverScreen.IsShown)
                return; 
                
            ExitBattleButtonOnClick();
        }

        if (_slideDelay > 0f){
            _slideDelayCounter += Time.deltaTime;

            if (_slideDelayCounter >= _slideDelay){
                _slideDelay = 0f;
                _slideDelayCounter = 0f;

                if (_slideDirection == -1){
                    _battlefieldScrollAnimator.ResetTrigger("ScrollToOpponent");
                    _battlefieldScrollAnimator.SetTrigger("ScrollToPlayer");   

                    _parallaxScrollAnimator.ResetTrigger("ScrollToOpponent");
                    _parallaxScrollAnimator.SetTrigger("ScrollToPlayer");
                }
                else if (_slideDirection == 1){
                    _battlefieldScrollAnimator.ResetTrigger("ScrollToPlayer");
                    _battlefieldScrollAnimator.SetTrigger("ScrollToOpponent");

                    _parallaxScrollAnimator.ResetTrigger("ScrollToPlayer");
                    _parallaxScrollAnimator.SetTrigger("ScrollToOpponent");
                }

                _slideDirection = 0;
            }
        }
    }

#region Networking

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (_gc.BattleController.IsGameOver)
            return;

        MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Opponent Left The Room"), () =>
        {
            _gc.Reset();
            _gc.NavigateTo("Welcome");
        });
    }

#endregion

}
