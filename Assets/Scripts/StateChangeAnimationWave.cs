using System;
using UnityEngine;
using UnityEngine.UI;

public class StateChangeAnimationWave : SwoScriptBase {

    private const float AnimationDuration = 0.4f;

    public Action OnCompleted { get; set; }

    private float _delay;
    private float _delayCounter;
    private float _animationCounter;
    private bool _isActive;
    private RectTransform _waveTransform;
    private Image _waveImage;
    private GameObject _waveObject;

    public static void AttachTo(RectTransform cell){
        if (cell?.gameObject == null)
            return;
            
        var animation = cell.gameObject.AddComponent<StateChangeAnimationWave>();

        animation.OnCompleted = () => {
            animation.OnCompleted = null;

            // Remove the component from the cell
            Destroy(animation);
        };

        animation.enabled = true;
    }

    protected override void OnStart(){
        var cellRect = gameObject.GetComponent<RectTransform>().rect;

        _waveObject = new GameObject();

        _waveTransform = _waveObject.AddComponent<RectTransform>();

        _waveImage = _waveObject.AddComponent<Image>();

        _waveImage.sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/ExplosionWave");
        _waveImage.color = new Color(1f,1f,1f,1f); // White opaque

        _waveTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellRect.width + cellRect.width/2f);
        _waveTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellRect.height  + cellRect.height/2f);

        _waveTransform.SetParent(gameObject.GetComponent<RectTransform>());

        _waveTransform.localScale = new Vector3(0f,0f,0f);
        _waveTransform.position = new Vector3(0f,0f,0f);
        _waveTransform.anchoredPosition = new Vector2(0f,0f);

        _delay = UnityEngine.Random.Range(0f, 100f) / 1000f;

        _isActive = true;
    }

    protected override bool IsPaperDistortionEffectAllowed => false;

    protected override void OnUpdate(){
        if (!_isActive)
            return;

        if (_delayCounter < _delay){
            _delayCounter += Time.deltaTime;

            return;
        }

        _animationCounter += Time.deltaTime;

        if (_animationCounter >= AnimationDuration){
            _isActive = false;

            Destroy(_waveObject);

            OnCompleted?.Invoke();

            return;
        }

        var p = 1f - _animationCounter / AnimationDuration;

        _waveImage.color = new Color(1f,1f,1f,p);
        _waveTransform.localScale = new Vector3(1f - p, 1f - p,0);
    }

}