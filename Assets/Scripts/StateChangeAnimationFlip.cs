using System;
using UnityEngine;
using UnityEngine.UI;

public class StateChangeAnimationFlip : SwoScriptBase
{

    private const float AnimationDuration = 0.6f;

    public Action OnCompleted { get; set; }
    public Sprite TargetStateSprite { get; set; }
    public Sprite InitialStateSprite { get; set; }

    private float _animationCounter;
    private bool _isActive;
    private float _delay;
    private float _delayCounter;
    private bool _isImageFlipped;
    private GameObject _rotatingCell;
    private RectTransform _rotatingTransform;
    private Image _rotatingImage;
    private Image _bgImage;
    private Image _cellImage;
    private GameObject _bgCell;

    public static void AttachTo(RectTransform cell, Sprite initialStateSprite, Sprite targetStateSprite){
        if (cell?.gameObject == null)
            return;
            
        var animation = cell.gameObject.AddComponent<StateChangeAnimationFlip>();

        animation.TargetStateSprite = targetStateSprite;
        animation.InitialStateSprite = initialStateSprite;
        animation.OnCompleted = () => {
            animation.OnCompleted = null;

            // Remove the component from the cell
            Destroy(animation);
        };

        animation.enabled = true;
    }

    protected override void OnStart(){
        // Prepare rotating cell object
        var cellRect = gameObject.GetComponent<RectTransform>().rect;
        
        _cellImage = gameObject.GetComponent<Image>();

        _bgCell = new GameObject() { 
            name = "backgroung-cell"
        };
        
        var bgTran = _bgCell.AddComponent<RectTransform>();

        bgTran.SetParent(gameObject.transform);

        bgTran.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellRect.width);
        bgTran.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellRect.height);

        _bgImage = _bgCell.AddComponent<Image>();
        
        var emptySprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/Cells/CellEmpty");

        if (emptySprite == null)
            emptySprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/Cells/CellEmpty.0");

        _bgImage.sprite = emptySprite;

        bgTran.localScale = new Vector3(1f,1f,1f);
        bgTran.position = new Vector3(0f,0f,0f);
        bgTran.anchoredPosition = new Vector2(0f,0f);

        _rotatingCell = new GameObject() { 
            name = "rotating-cell"
        };
        
        _rotatingTransform = _rotatingCell.AddComponent<RectTransform>();

        _rotatingTransform.SetParent(gameObject.transform);

        _rotatingTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellRect.width);
        _rotatingTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellRect.height);

        _rotatingImage = _rotatingCell.AddComponent<Image>();
        _rotatingImage.sprite = InitialStateSprite;
        _rotatingImage.color = new Color(_cellImage.color.r, _cellImage.color.g, _cellImage.color.b, 1f);

        _rotatingTransform.localScale = new Vector3(-1f,1f,1f);
        _rotatingTransform.position = new Vector3(0f,0f,0f);
        _rotatingTransform.anchoredPosition = new Vector2(0f,0f);
        _rotatingTransform.rotation = Quaternion.Euler(new Vector3(0f,-180f,0f));

        _cellImage.sprite = null;
        _cellImage.color = new Color(_cellImage.color.r, _cellImage.color.g, _cellImage.color.b, 0f); // Transparent

        _delay = UnityEngine.Random.Range(0f, 100f) / 1000f;

        _isActive = true;
    }

    protected override bool IsPaperDistortionEffectAllowed => false;

    protected override void OnUpdate() {
        if (!_isActive)
            return;

        if (_delayCounter < _delay){
            _delayCounter += Time.deltaTime;

            return;
        }

        _animationCounter += Time.deltaTime;

        if(_animationCounter >= AnimationDuration){
            _isActive = false;

            _cellImage.sprite = TargetStateSprite;
            _cellImage.color = new Color(_cellImage.color.r, _cellImage.color.g, _cellImage.color.g, 1f);

            Destroy(_bgCell);
            Destroy(_rotatingCell);

            OnCompleted?.Invoke();
            return;
        }

        var p = _animationCounter / AnimationDuration;

        var deg = 180f * p;

        if (p >= 0.5f && !_isImageFlipped){
            // Change image to target

            _rotatingImage.sprite = TargetStateSprite;

            _rotatingTransform.localScale = new Vector3(1f,1f,1f);

            _isImageFlipped = true;
        }

        _rotatingTransform.rotation = Quaternion.Euler(new Vector3(0f,-180f + deg,0f));
    }

}