using System;
using UnityEngine;
using UnityEngine.UI;

public class ScannerWaveAnimation : SwoScriptBase
{

    private const int WaveGrowCycles = 3;
    private const float WaveGrowTime = 0.5f;
    private const float DelayBetweenWaves = 0.5f;

    public Action OnActivationCompleted { get; set; }

    private GameObject _wave1;
    private GameObject _wave2;
    private Image _wave1Image;
    private Image _wave2Image;
    private RectTransform _wave1Transform;
    private RectTransform _wave2Transform;

    private bool _isActive;
    private int _cyclesCounter;
    private float _wave1Time;
    private float _wave2Time;
    private float _wave1Waiter;
    private float _wave2Waiter;
    private Vector2 _maxWaveSize;

    public static void AttachTo(RectTransform cell, Action onAnimationCompleted){
         if (cell?.gameObject == null)
            return;

        var animation = cell.gameObject.AddComponent<ScannerWaveAnimation>();

        animation.SetWaveSize(cell.rect.width * 4f, cell.rect.height * 4f);
        animation.OnActivationCompleted = () => {
            Destroy(animation);

            onAnimationCompleted?.Invoke();
        };
    }

    protected override void OnStart()
    {
        _wave1 = new GameObject{
            name = "Wave1"
        };
        _wave2 = new GameObject{
            name = "Wave2"
        };

        _wave1Image = _wave1.AddComponent<Image>();
        _wave2Image = _wave2.AddComponent<Image>();
        _wave1Transform = _wave1.GetComponent<RectTransform>();
        _wave2Transform = _wave2.GetComponent<RectTransform>();

        _wave1Image.sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/ScannerWave");
        _wave2Image.sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/ScannerWave");

        _wave1Transform.SetParent(gameObject.GetComponent<RectTransform>());

        _wave1Transform.localScale = new Vector3(0f,0f,0f);
        _wave1Transform.position = new Vector3(0f,0f,0f);
        _wave1Transform.anchoredPosition = new Vector2(0f,0f);

        _wave2Transform.SetParent(gameObject.GetComponent<RectTransform>());

        _wave2Transform.localScale = new Vector3(0f,0f,0f);
        _wave2Transform.position = new Vector3(0f,0f,0f);
        _wave2Transform.anchoredPosition = new Vector2(0f,0f);

        _wave1Time = -1f;
        _wave2Time = -1f;
        _wave1Waiter = DelayBetweenWaves;
        _wave1Waiter = UnityEngine.Random.Range(DelayBetweenWaves / 6f, DelayBetweenWaves / 2f);

        _wave1Transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _maxWaveSize.x);
        _wave1Transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _maxWaveSize.y);
        _wave2Transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _maxWaveSize.x);
        _wave2Transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _maxWaveSize.y);

        _isActive = true;
    }

    private void ResetWave1(){
        _wave1Transform.localScale = new Vector3(0,0,0);
        _wave1Image.color = new Color(255, 255, 255, 0);
    }
    
    private void ResetWave2(){
        _wave2Transform.localScale = new Vector3(0,0,0);
        _wave2Image.color = new Color(255, 255, 255, 0);
    }

    public void SetWaveSize(float w, float h){
        _maxWaveSize = new Vector2(w, h);
    }

    protected override bool IsPaperDistortionEffectAllowed => false;

    protected override void OnUpdate()
    {
        if (!_isActive)
            return;

        if (_wave1Time >= WaveGrowTime){
            ResetWave1();

            _wave1Time = -1f;
            _wave1Waiter = 0f;
        }

        if (_wave2Time >= WaveGrowTime){
             _cyclesCounter++;

            if (_cyclesCounter == WaveGrowCycles)
            {
                Destroy(_wave1);
                Destroy(_wave2);

                OnActivationCompleted?.Invoke();

                return;
            }

            ResetWave2();

            _wave2Time = -1f;
            _wave2Waiter = 0f;
        }

        if(_wave1Time == -1f){
            if (_wave1Waiter < DelayBetweenWaves){
                _wave1Waiter += Time.deltaTime;
            } else {
                if (_cyclesCounter < WaveGrowCycles - 1)
                    _wave1Time = 0f;
            }
        } else {
            _wave1Time += Time.deltaTime;

            // Do actual growth
            var p = 1f - _wave1Time / WaveGrowTime;

            _wave1Image.color = new Color(1f,1f,1f,p);
            _wave1Transform.localScale = new Vector3(1f - p, 1f - p,0);
            _wave1Transform.Rotate(0f, 0f, 10f*p, Space.World);
        }

        if(_wave2Time == -1f){
            if (_wave2Waiter < DelayBetweenWaves){
                _wave2Waiter += Time.deltaTime;
            } else {
                _wave2Time = 0f;
            }
        } else {
            _wave2Time += Time.deltaTime;

            // Do actual growth
            var p = 1f - _wave2Time / WaveGrowTime;

            _wave2Image.color = new Color(1f,1f,1f,p);
            _wave2Transform.localScale = new Vector3(1f - p, 1f - p,0);
            _wave2Transform.Rotate(0f, 0f, 10f*p, Space.World);
        }
    }
}
