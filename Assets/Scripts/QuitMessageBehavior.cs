using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core.Localization;

public class QuitMessageBehavior : SwoScriptBase
{

    private GameObject _popup;

    public void Initialize(){
        _popup = FindInScene("QuitMessage");
        
        LoadLocalization();
        
        _popup.SetActive(false);
    }

    public void Show(){
        _popup.SetActive(true);
    }

    public void Hide(){
        _popup.SetActive(false);
    }

    public void LoadLocalization()
    {
        _popup.transform.Find("Text").GetComponent<Text>().text = LocalizationManager.Instance.GetString("Welcome", "Quit Message");
    }

    protected override bool IsPaperDistortionEffectAllowed => false;

}