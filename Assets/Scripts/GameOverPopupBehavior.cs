using SeaWarsOnline.Core;
using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core.Localization;

public class GameOverPopupBehavior : SwoScriptBase {

    private GameObject _overlayLayer;
    private GameObject _gameOverLayer;
    private Text _resultText;
    private bool _isGameOverShown;
    private bool _wasDismissed;

    protected override void OnStart()
    {
        _resultText = FindInScene<Text>("CanvasGameOver/Content/GameResult");
        _overlayLayer = FindInScene("CanvasOverlay");
        _gameOverLayer = FindInScene("CanvasGameOver");

        LoadLocalization();

        FindInScene("CanvasGameOver/Content/PlayAgainButton").SetActive(_gc.Rules.GameKind == GameRules.GameKinds.SinglePlayer);
    }

    public bool IsShown => _isGameOverShown;

    private void LoadLocalization()
    {
        FindInScene<Text>("CanvasGameOver/Content/PlayAgainButton/Text").text = LocalizationManager.Instance.GetString("GameOver", "Play Again");
        FindInScene<Text>("CanvasGameOver/Content/ExitButton/Text").text = LocalizationManager.Instance.GetString("GameOver", "Exit");
    }

    public void Initialize(){
         _overlayLayer.SetActive(false);
         _gameOverLayer.SetActive(false);
    }

    public void Show(){
        if(_isGameOverShown || _wasDismissed) 
            return;

        _isGameOverShown = true;

        _overlayLayer.SetActive(true);
        _gameOverLayer.SetActive(true);

        _resultText.text = _gc.BattleController.PlayerWon == true ? LocalizationManager.Instance.GetString("GameOver", "You Win") : LocalizationManager.Instance.GetString("GameOver", "Opponent Wins");
    }

    public void ExitButtonOnClick(){
        _gc.Reset();
        _gc.NavigateTo("Welcome");
    }

    public void CloseButtonOnClick(){
        _isGameOverShown = false;
        _wasDismissed = true;
        
         _overlayLayer.SetActive(false);
         _gameOverLayer.SetActive(false);
    }

    public void PlayAgainButtonOnClick(){
        if (_gc.Rules.GameKind == GameRules.GameKinds.Network)
        {
            ExitButtonOnClick();

            return;
        }

        _gc.InitializeMePlayer();
        _gc.NavigateTo("Arrangement");
    }

    protected override bool IsPaperDistortionEffectAllowed => false;

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseButtonOnClick();
        }
    }

}