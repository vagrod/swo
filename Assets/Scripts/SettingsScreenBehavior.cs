using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core.Localization;

public class SettingsScreenBehavior : SwoScriptBase{

    private bool _isLoaded;

    private readonly List<string> _skinsAvailable = new List<string>(new[]{
        "default",
        "dark"
    });

    protected override void OnStart(){
        var skin = _gc.Settings.SkinName;
        var animation = _gc.Settings.CellAnimation;
        var language = _gc.Settings.Language;

        FindInScene<Toggle>("Canvas/Settings/AutoScroll/Toggle").isOn = _gc.Settings.AutoScroll;

        LoadUI();

        FindInScene<Dropdown>("Canvas/Settings/Skin/SkinCombo").value = _skinsAvailable.IndexOf(skin);
        FindInScene<Dropdown>("Canvas/Settings/Orientation/OrientationCombo").value = _gc.Settings.IsPortraitMode ? 1 : 0;
        FindInScene<Dropdown>("Canvas/Settings/CellAnimation/AnimationCombo").value = _gc.AnimationsAvailable.IndexOf(animation);
        FindInScene<Dropdown>("Canvas/Settings/Language/LanguageCombo").value = _gc.LanguagesAvailable.IndexOf(language);
        FindInScene<Dropdown>("Canvas/Settings/PaperEffect/PaperCombo").value = (int)_gc.Settings.PaperEffect;

        FindInScene("Canvas/Settings/PaperEffect").SetActive(Input.touchSupported); // Not visible for non-touch devices
        FindInScene("Canvas/Settings/Orientation").SetActive(Input.touchSupported); // Not visible for non-touch devices

        _isLoaded = true;
    }

    private void LoadUI(){
        LoadLocalization();
        LoadOrientationCombo();
        LoadSkinCombo();
        LoadAnimationCombo();
        LoadLanguageCombo();
        LoadPaperEffectCombo();
    }

    private void LoadLocalization(){
        FindInScene<Text>("Canvas/Title").text = LocalizationManager.Instance.GetString("Settings", "Title");
        FindInScene<Text>("Canvas/BackButton/Text").text = "<< " + LocalizationManager.Instance.GetString("Settings", "Back");

        FindInScene<Text>("Canvas/Settings/Language/Label").text = LocalizationManager.Instance.GetString("Settings", "Language") + ":";
        FindInScene<Text>("Canvas/Settings/Skin/Label").text = LocalizationManager.Instance.GetString("Settings", "Skin") + ":";
        FindInScene<Text>("Canvas/Settings/Orientation/Label").text = LocalizationManager.Instance.GetString("Settings", "Orientation") + ":";
        FindInScene<Text>("Canvas/Settings/CellAnimation/Label").text = LocalizationManager.Instance.GetString("Settings", "Cell Animation") + ":";
        FindInScene<Text>("Canvas/Settings/AutoScroll/Toggle/Label").text = LocalizationManager.Instance.GetString("Settings", "Auto Scroll");
        FindInScene<Text>("Canvas/Settings/PaperEffect/Label").text = LocalizationManager.Instance.GetString("Settings", "Paper Effect") + ":";
    }

     private void LoadPaperEffectCombo(){
        var combo = FindInScene<Dropdown>("Canvas/Settings/PaperEffect/PaperCombo");

        combo.options.Clear();

        combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("PaperEffects", SeaWarsOnline.Core.AppSettings.PaperEffectTypes.PartialEffect.ToString())));
        combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("PaperEffects", SeaWarsOnline.Core.AppSettings.PaperEffectTypes.FullEffect.ToString())));
        combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("PaperEffects", SeaWarsOnline.Core.AppSettings.PaperEffectTypes.NoEffect.ToString())));
    }

     private void LoadSkinCombo(){
        var combo = FindInScene<Dropdown>("Canvas/Settings/Skin/SkinCombo");

        combo.options.Clear();

        foreach(var item in _skinsAvailable){
            combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("Skins", item)));
        }
    }

    private void LoadOrientationCombo(){
        var combo = FindInScene<Dropdown>("Canvas/Settings/Orientation/OrientationCombo");

        combo.options.Clear();

        combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("Orientations", "Landscape")));
        combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("Orientations", "Portrait")));
    }

    private void LoadLanguageCombo(){
        var combo = FindInScene<Dropdown>("Canvas/Settings/Language/LanguageCombo");

        combo.options.Clear();

        foreach(var item in _gc.LanguagesAvailable){
            combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("Common", item), Resources.Load<Sprite>($"Localization/Flag{item}")));
        }
    }

    private void LoadAnimationCombo(){
        var combo = FindInScene<Dropdown>("Canvas/Settings/CellAnimation/AnimationCombo");

        combo.options.Clear();

        foreach(var item in _gc.AnimationsAvailable){
            combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("Animations", item)));
        }
    }

    public void OnAutoScrollClicked(bool o){
        if (!_isLoaded)
            return;

        var newValue = FindInScene<Toggle>("Canvas/Settings/AutoScroll/Toggle").isOn;

        _gc.Settings.AutoScroll = newValue;
        _gc.Settings.Save();
    }

    public void OnCellAnimationValueChanged(int n){
        if (!_isLoaded)
            return;

        var ind = FindInScene<Dropdown>("Canvas/Settings/CellAnimation/AnimationCombo").value;

        if (ind < 0 || ind >= _gc.AnimationsAvailable.Count)
            return;

        _gc.Settings.CellAnimation = _gc.AnimationsAvailable[ind];
        _gc.Settings.Save();
    }

    public void OnOrientationValueChanged(int n){
        if (!_isLoaded)
            return;

        var ind = FindInScene<Dropdown>("Canvas/Settings/Orientation/OrientationCombo").value;

        _gc.Settings.IsPortraitMode = ind == 1;
        _gc.Settings.Save();
        _gc.NavigateTo("Settings");
    }

     public void OnPaperEffectValueChanged(int n){
        if (!_isLoaded)
            return;

        var ind = FindInScene<Dropdown>("Canvas/Settings/PaperEffect/PaperCombo").value;

        _gc.Settings.PaperEffect = (SeaWarsOnline.Core.AppSettings.PaperEffectTypes)ind;
        _gc.Settings.Save();
    }

    public void OnLanguageValueChanged(int n){
        if (!_isLoaded)
            return;

        var ind = FindInScene<Dropdown>("Canvas/Settings/Language/LanguageCombo").value;

        if (ind < 0 || ind >= _gc.LanguagesAvailable.Count)
            return;

        _gc.Settings.Language = _gc.LanguagesAvailable[ind];
        _gc.Settings.Save();

        LocalizationManager.Instance.Language = _gc.Settings.Language;

        LoadUI();

        var combo = FindInScene<Dropdown>("Canvas/Settings/Language/LanguageCombo");
        FindInScene<Text>("Canvas/Settings/Language/LanguageCombo/Label").text = combo.options[combo.value].text;

        combo = FindInScene<Dropdown>("Canvas/Settings/Skin/SkinCombo");
        FindInScene<Text>("Canvas/Settings/Skin/SkinCombo/Label").text = combo.options[combo.value].text;

        combo = FindInScene<Dropdown>("Canvas/Settings/CellAnimation/AnimationCombo");
        FindInScene<Text>("Canvas/Settings/CellAnimation/AnimationCombo/Label").text = combo.options[combo.value].text;

        combo = FindInScene<Dropdown>("Canvas/Settings/Orientation/OrientationCombo");
        FindInScene<Text>("Canvas/Settings/Orientation/OrientationCombo/Label").text = combo.options[combo.value].text;

        combo = FindInScene<Dropdown>("Canvas/Settings/PaperEffect/PaperCombo");
        FindInScene<Text>("Canvas/Settings/PaperEffect/PaperCombo/Label").text = combo.options[combo.value].text;
    }

    public void OnSkinValueChanged(int n){
        if (!_isLoaded)
            return;

        var ind = FindInScene<Dropdown>("Canvas/Settings/Skin/SkinCombo").value;

        if (ind < 0 || ind >= _skinsAvailable.Count)
            return;

        _gc.Settings.SkinName = _skinsAvailable[ind];
        _gc.Settings.Save();
        _gc.NavigateTo("Settings");
    }

    public void OnBackClicked(){
        _gc.NavigateTo("Welcome");
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackClicked();
        }
    }

}