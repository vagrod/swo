using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core.Localization;

public class AboutScreenBehavior : SwoScriptBase {

    protected override void OnStart() {
        LoadLocalization();
    }

    private void LoadLocalization() {
        FindInScene<Text>("Canvas/Title").text = LocalizationManager.Instance.GetString("About", "Title");
        FindInScene<Text>("Canvas/BackButton/Text").text = "<< " + LocalizationManager.Instance.GetString("About", "Back");

        FindInScene<Text>("Canvas/Content/Version").text = $"{LocalizationManager.Instance.GetString("About", "Version")} {Application.version}";
        FindInScene<Text>("Canvas/Content/Author/Caption").text = LocalizationManager.Instance.GetString("About", "Author");
        FindInScene<Text>("Canvas/Content/Author/Name").text = LocalizationManager.Instance.GetString("About", "Name");
        FindInScene<Text>("Canvas/Content/MadeWith/Unity/Text").text = LocalizationManager.Instance.GetString("About", "Made With Unity");
        FindInScene<Text>("Canvas/Content/MadeWith/Krita/Text").text = "... " + LocalizationManager.Instance.GetString("About", "And Krita");
    }

    public void OnBackClick() {
        _gc.NavigateTo("Welcome");
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackClick();
        }
    }

}