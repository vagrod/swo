using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core;
using SeaWarsOnline.Core.Localization;

public class WelcomeScreenBehavior : SwoScriptBase
{

    private const float QuitTapsDuration = 1.0f;

    private bool _isBackAlreadyPressed;
    private float _quitResetCounter;
    private QuitMessageBehavior _quitMessage;

    protected override void OnStart(){
        MessageBox.Initialize(_gc);

        _quitMessage = FindInScene<QuitMessageBehavior>("QuitMessage");
        _quitMessage.Initialize();

        LoadLocalization();
        LoadLanguages();
    }

    private void LoadLocalization(){
        FindInScene<Text>("Canvas/Title").text = LocalizationManager.Instance.GetString("Common", "App Name");
        FindInScene<Text>("Canvas/SinglePlayerButton/Text").text = LocalizationManager.Instance.GetString("Welcome", "New Single Player");
        FindInScene<Text>("Canvas/MultiplayerButton/Text").text = LocalizationManager.Instance.GetString("Welcome", "Multiplayer");

        _quitMessage.LoadLocalization();
    }

    private void LoadLanguages(){
        var combo = FindInScene<Dropdown>("Canvas/LanguagesCombo");

        combo.options.Clear();

        foreach(var lang in _gc.LanguagesAvailable){
            combo.options.Add(new Dropdown.OptionData(lang, Resources.Load<Sprite>($"Localization/Flag{lang}")));
        }

        combo.value = _gc.LanguagesAvailable.IndexOf(_gc.Settings.Language);
    }

    public void OnLanguageValueChanged(int n){
        var combo = FindInScene<Dropdown>("Canvas/LanguagesCombo");
        var ind = combo.value;

        if (ind < 0 || ind >= combo.options.Count)
            return;

        // Write to app settings
        _gc.Settings.Language = combo.options[ind].text;

        // Change UI language
        LocalizationManager.Instance.Language = combo.options[ind].text;

        // Save app settings 
        _gc.Settings.Save();

        // Reload UI text
        LoadLocalization();
    }

    public void OnSettingsClick(){
        _gc.NavigateTo("Settings");
    }

    public void OnAboutClick(){
        _gc.NavigateTo("About");
    }

    public void OnStartSinglePlayerGameClick(){
        _gc.Rules.GameKind = GameRules.GameKinds.SinglePlayer;
        _gc.NavigateTo("Rules");
    }

    public async void OnStartMultiplayerGameClick(){
        if (!_gc.IsInternetConnected())
        {
            MessageBox.Show(LocalizationManager.Instance.GetString("Networking", "Network Error"), OnStartMultiplayerGameClick, null);
            return;
        }

        _gc.Rules.GameKind = GameRules.GameKinds.Network;

        await _gc.StartGameSession();

        _gc.NavigateTo("MultiplayerWelcome");
    }

    protected override bool IsPaperDistortionEffectAllowed {
        get {
            var isPopupOpened = FindInScene<RectTransform>("Canvas/LanguagesCombo").childCount == 5;
            
            var languageSquareSize = _gc.ScreenSize.width / 5f;

            if (Input.mousePosition.x > _gc.ScreenSize.width - languageSquareSize && Input.mousePosition.y < languageSquareSize * (isPopupOpened ? 2f : 1f))
                return false;

            return true;
        }
    }

    protected override void OnUpdate(){
        if (Input.GetKeyDown(KeyCode.Escape) && !_isBackAlreadyPressed)
        {
            _quitMessage.Show();

            _isBackAlreadyPressed = true;

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && _isBackAlreadyPressed){
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif

            return;
        }

        if (_isBackAlreadyPressed){
            _quitResetCounter += Time.deltaTime;

            if (_quitResetCounter >= QuitTapsDuration)
            {
                _quitMessage.Hide();

                _isBackAlreadyPressed = false;
                _quitResetCounter = 0f;
            }
        }
    }

}
