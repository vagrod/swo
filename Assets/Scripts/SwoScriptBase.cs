using Photon.Pun;
using UnityEngine;

public class SwoScriptBase : MonoBehaviourPunCallbacks
{

    public SwoScriptBase(){
        IsPaperDistortionEffectAllowed = true;
    }

    protected GameController _gc { get; set; }

    protected CameraDistortByMaterialEffect PaperDistortionEffect {get; set;}
    protected virtual bool IsPaperDistortionEffectAllowed {get;}

    protected GameObject FindInScene(string path){
        var s = path.Split('/');

        if (s.Length == 0)
            return null;

        var current = GameObject.Find(s[0]);

        if (s.Length == 1)
            return current;

        if(current == null)
            return null;

        for (int i = 1; i < s.Length; i++){
            var p = s[i];

            current = current.transform?.Find(p)?.gameObject;

            if(current == null)
                return null;
        }

        return current;
    }

    protected T FindInScene<T>(string path) where T : class{
        var c = FindInScene(path);

        if (c == null)
            return (T)null;

        return c.GetComponent<T>();
    }

    void Update(){
        if(Input.GetMouseButtonUp(0)){
            if (PaperDistortionEffect != null && PaperDistortionEffect.enabled){
                PaperDistortionEffect.Deactivate();
            }
        }

        if (_gc.Settings.PaperEffect != SeaWarsOnline.Core.AppSettings.PaperEffectTypes.NoEffect){
            var isEffectAllowed = _gc.Settings.PaperEffect == SeaWarsOnline.Core.AppSettings.PaperEffectTypes.FullEffect || IsPaperDistortionEffectAllowed;
            
            if(Input.GetMouseButtonDown(0)){
                if (PaperDistortionEffect != null && isEffectAllowed)
                    PaperDistortionEffect.Activate(_gc);
            }
            if (PaperDistortionEffect != null && PaperDistortionEffect.enabled){
                PaperDistortionEffect.Update();
            }
        }

        OnUpdate();
    }

    void Start()
    {
        _gc = GameObject.Find("GameController").GetComponent<GameController>();

        if (!_gc.Settings.IsPortraitMode)
            Screen.orientation = ScreenOrientation.Landscape;
        else 
            Screen.orientation = ScreenOrientation.Portrait;

        _gc.UpdateResolution();

        var cam = FindInScene("Main Camera");

        if (cam != null){
            var check = cam.GetComponent<CameraDistortByMaterialEffect>();

            if(check == null)
                PaperDistortionEffect = cam.AddComponent<CameraDistortByMaterialEffect>();
            else 
                PaperDistortionEffect = check;

            PaperDistortionEffect.SetUpEffect(_gc);

            PaperDistortionEffect.enabled = false;
        }

        OnStart();
    }

    protected virtual void OnUpdate(){

    }

    protected virtual void OnStart(){

    }

}