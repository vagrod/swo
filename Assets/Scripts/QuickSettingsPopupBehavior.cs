using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core.Localization;

public class QuickSettingsPopupBehavior : SwoScriptBase
{

    private const float AnimationDuration = 0.3f;

    private GameObject _popup;
    private RectTransform _animatedContentTransform;

    private float _animationCounter;
    private bool _isActive;
    private float _expandedTop;
    private float _collapsedTop;
    private float _panelHeight;
    private int _animationDirection; // 0-down,1-up

    protected override void OnStart(){
        _popup = FindInScene("CanvasQuickSettings");
        _animatedContentTransform = FindInScene<RectTransform>("CanvasQuickSettings/Settings");

        _panelHeight = _animatedContentTransform.rect.height;

        _expandedTop = _animatedContentTransform.anchoredPosition.y;
        _collapsedTop = _expandedTop + _panelHeight;

        _animatedContentTransform.anchoredPosition = new Vector2(_animatedContentTransform.anchoredPosition.x, _collapsedTop);
    }

    public void Initialize(){
        var anim = _gc.Settings.CellAnimation;

        LoadLocalization();
        LoadAnimationCombo();

        FindInScene<Toggle>("CanvasQuickSettings/Settings/AutoScroll/Toggle").isOn = _gc.Settings.AutoScroll;
        FindInScene<Dropdown>("CanvasQuickSettings/Settings/CellAnimation/AnimationCombo").value = _gc.AnimationsAvailable.IndexOf(anim);

        _popup.SetActive(false);
    }

    private void LoadLocalization(){
        FindInScene<Text>("CanvasQuickSettings/Settings/CellAnimation/Label").text = LocalizationManager.Instance.GetString("Settings", "Cell Animation") + ":";
        FindInScene<Text>("CanvasQuickSettings/Settings/AutoScroll/Toggle/Label").text = LocalizationManager.Instance.GetString("Settings", "Auto Scroll");
    }

    private void LoadAnimationCombo(){
        var combo = FindInScene<Dropdown>("CanvasQuickSettings/Settings/CellAnimation/AnimationCombo");

        combo.options.Clear();

        foreach(var item in _gc.AnimationsAvailable){
            combo.options.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetString("Animations", item)));
        }
    }

    public void OnAutoScrollClicked(bool o){
        var newValue = FindInScene<Toggle>("CanvasQuickSettings/Settings/AutoScroll/Toggle").isOn;

        _gc.Settings.AutoScroll = newValue;
        _gc.Settings.Save();
    }

    public void OnCellAnimationValueChanged(int n){
        var ind = FindInScene<Dropdown>("CanvasQuickSettings/Settings/CellAnimation/AnimationCombo").value;

        if (ind < 0 || ind >= _gc.AnimationsAvailable.Count)
            return;

        _gc.Settings.CellAnimation = _gc.AnimationsAvailable[ind];
        _gc.Settings.Save();
    }

    public void Show(){
        _animationDirection = 0;

        _animatedContentTransform.anchoredPosition = new Vector2(_animatedContentTransform.anchoredPosition.x, _collapsedTop);

        _popup.SetActive(true);

        _isActive = true;
    }

    public void CloseButtonOnClick(){
        _animationDirection = 1;

        _animatedContentTransform.anchoredPosition = new Vector2(_animatedContentTransform.anchoredPosition.x, _expandedTop);

        _isActive = true;
    }

    protected override bool IsPaperDistortionEffectAllowed => false;

    protected override void OnUpdate(){
        if (!_isActive)
            return;

        _animationCounter += Time.deltaTime;

        if (_animationCounter >= AnimationDuration){
            _isActive = false;

            _animationCounter = 0f;

            if (_animationDirection == 0)
            {
                _animatedContentTransform.anchoredPosition = new Vector2(_animatedContentTransform.anchoredPosition.x, _expandedTop);
            } else {
                _animatedContentTransform.anchoredPosition = new Vector2(_animatedContentTransform.anchoredPosition.x, _collapsedTop);

                _popup.SetActive(false);
            }

            return;
        }

        var p = _animationCounter / AnimationDuration;
        var origin = _animationDirection == 0 ? _collapsedTop : _expandedTop;
        var multiplier = _animationDirection == 0 ? -1 : 1;

        _animatedContentTransform.anchoredPosition = new Vector2(_animatedContentTransform.anchoredPosition.x, origin + _panelHeight*p*multiplier*Mathf.Sin(p));
    }

}