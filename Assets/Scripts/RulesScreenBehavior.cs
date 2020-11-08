using System.Linq;
using SeaWarsOnline.Core;
using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core.Localization;

public class RulesScreenBehavior : SwoScriptBase
{

    private DensityProfilesRoot _densityProfiles;

    protected override void OnStart(){
        var straightShips = _gc.Rules.StraightShips;
        var densityProfile = _gc.Rules.DensityProfile;

        FindInScene<Text>("Canvas/Settings/FieldSize/FieldSizeText/Value").text = _gc.Rules.GameFieldSize.ToString();
        FindInScene<Toggle>("Canvas/Settings/AllowMines/Toggle").isOn = _gc.Rules.AllowMines;
        FindInScene<Toggle>("Canvas/Settings/AllowSpyGlass/Toggle").isOn = _gc.Rules.AllowSpyGlass;
        FindInScene<Toggle>("Canvas/Settings/ExtraTurn/Toggle").isOn = !_gc.Rules.ChangeTurnEachTime;

        _densityProfiles = JsonUtility.FromJson<DensityProfilesRoot>(Resources.Load<TextAsset>("density-profiles").text);

        LoadLocalization();
        LoadShapeCombo();
        LoadDensityCombo();

        FindInScene<Dropdown>("Canvas/Settings/Shape/ShapeCombo").value = straightShips ? 0 : 1;
        FindInScene<Dropdown>("Canvas/Settings/Density/DensityCombo").value = densityProfile;
    }

    private void LoadLocalization(){
        FindInScene<Text>("Canvas/Title").text = LocalizationManager.Instance.GetString("Rules", "Title");
        FindInScene<Text>("Canvas/NextButton/Text").text = LocalizationManager.Instance.GetString("Rules", "Next") + " >>";

        FindInScene<Text>("Canvas/Settings/FieldSize/FieldSizeText/Label").text = LocalizationManager.Instance.GetString("Rules", "Field Size") + ":";
        FindInScene<Text>("Canvas/Settings/AllowMines/Toggle/Label").text = LocalizationManager.Instance.GetString("Rules", "Allow Mines");
        FindInScene<Text>("Canvas/Settings/AllowSpyGlass/Toggle/Label").text = LocalizationManager.Instance.GetString("Rules", "Allow SpyGlass");
        FindInScene<Text>("Canvas/Settings/ExtraTurn/Toggle/Label").text = LocalizationManager.Instance.GetString("Rules", "Extra Turn");
        FindInScene<Text>("Canvas/Settings/Density/Label").text = LocalizationManager.Instance.GetString("Rules", "Density") + ":";
        FindInScene<Text>("Canvas/Settings/Shape/Label").text = LocalizationManager.Instance.GetString("Rules", "Shape") + ":";
    }

    private void LoadShapeCombo(){
        var combo = FindInScene<Dropdown>("Canvas/Settings/Shape/ShapeCombo");

        combo.options.Clear();

        var items = new[] { 
            "Straight",
            "Any"
        };

        foreach(var item in items){
            combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("Rules", $"Shape-{item}"), Resources.Load<Sprite>($"Skins/{_gc.SkinName}/UI/Shape-{item}")));
        }
    }

    private void LoadDensityCombo(){
        var combo = FindInScene<Dropdown>("Canvas/Settings/Density/DensityCombo");

        combo.options.Clear();

        var items = new[] { 
            "High",
            "Medium",
            "Low",
            "Single"
        };

        foreach(var item in items){
            combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("Rules", $"Density-{item}"), Resources.Load<Sprite>($"Skins/{_gc.SkinName}/UI/Density-{item}")));
        }
    }

    public void OnFieldSizeUpClicked(){
        var n = _gc.Rules.GameFieldSize + 1;

        if (n > 15)
            return;

        _gc.Rules.GameFieldSize = n;

        FindInScene<Text>("Canvas/Settings/FieldSize/FieldSizeText/Value").text = n.ToString();
    }

    public void OnFieldSizeDownClicked(){
        var n = _gc.Rules.GameFieldSize - 1;

        if (n < 7)
            return;

        _gc.Rules.GameFieldSize = n;

        FindInScene<Text>("Canvas/Settings/FieldSize/FieldSizeText/Value").text = n.ToString();
    }

    public void OnGoToArrangementClicked(){
        if (!_gc.InitializeMePlayer())
            return;

        LoadDensityProfile();

        _gc.NavigateTo("Arrangement");
    }

    public void OnShapeValueChanged(int n){
        var ind = FindInScene<Dropdown>("Canvas/Settings/Shape/ShapeCombo").value;

        _gc.Rules.StraightShips = ind == 0;
    }

     public void OnDensityValueChanged(int n){
        var ind = FindInScene<Dropdown>("Canvas/Settings/Density/DensityCombo").value;

        _gc.Rules.DensityProfile = ind;
    }

    public void OnAllowMinesClicked(bool o){
        var newValue = FindInScene<Toggle>("Canvas/Settings/AllowMines/Toggle").isOn;

        _gc.Rules.AllowMines = newValue;
    }

    public void OnAllowSpyGlassClicked(bool o){
        var newValue = FindInScene<Toggle>("Canvas/Settings/AllowSpyGlass/Toggle").isOn;

        _gc.Rules.AllowSpyGlass = newValue;
    }

    public void OnTurnEachTimeClicked(bool o){
        var newValue = FindInScene<Toggle>("Canvas/Settings/ExtraTurn/Toggle").isOn;

        _gc.Rules.ChangeTurnEachTime = !newValue;
    }

    public void OnBackClicked()
    {
        var isNetworkGame = _gc.Rules.GameKind == GameRules.GameKinds.Network;

        _gc.Reset(false);
        _gc.Rules.GameKind = isNetworkGame ? GameRules.GameKinds.Network : GameRules.GameKinds.SinglePlayer;

        if (isNetworkGame)
            _gc.NavigateTo("MultiplayerWelcome");
        else
            _gc.NavigateTo("Welcome");
    }

    private void LoadDensityProfile(){
        var profile = _densityProfiles.profiles.FirstOrDefault(x => x.fieldSize == _gc.Rules.GameFieldSize && x.name == $"profile-{_gc.Rules.DensityProfile}");

        if (profile == null)
            profile = _densityProfiles.profiles.First(x => x.fieldSize == _gc.Rules.GameFieldSize);

        _gc.Rules.CountShipsSmall = (short)profile.countSmall;
        _gc.Rules.CountShipsMedium = (short)profile.countMedium;
        _gc.Rules.CountShipsLarge = (short)profile.countLarge;
        _gc.Rules.CountShipsExtraLarge = (short)profile.countVeryLarge;
        _gc.Rules.CountMines = _gc.Rules.AllowMines ? (short)profile.countMines : (short)0;
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackClicked();
        }
    }

}
