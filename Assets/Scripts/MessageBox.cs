using System;
using SeaWarsOnline.Core.Localization;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
    public static class MessageBox
    {

        private static GameController _gc;
        private static Text _messageTextTemplate;
        private static Text _buttonTextTemplate;

        public static void Initialize(GameController gc)
        {
            _gc = gc;

            _messageTextTemplate = GameObject.Find("Canvas/MessageBoxMessageTextTemplate").GetComponent<Text>();
            _buttonTextTemplate = GameObject.Find("Canvas/MessageBoxButtonTextTemplate").GetComponent<Text>();
        }

        private static bool _isShown;

        private static GameObject _messageCanvas;

        public static void Show(string text, Action okAction, bool showCancel = false)
        {
            if (_isShown)
                return;

            Show();

            var rectTransform = CreateBody(text);

            var buttonOk = CreateButton("OkButton", LocalizationManager.Instance.GetString("MessageBox", "OK"));
            buttonOk.SetParent(rectTransform);
            buttonOk.localScale = new Vector3(1f, 1f, 1f);

            if (!showCancel)
                buttonOk.anchoredPosition = new Vector2(0f, -rectTransform.rect.height / 2f + buttonOk.rect.height / 2f + 20f);
            else  {
                buttonOk.anchoredPosition = new Vector2(-buttonOk.rect.width/2f, -rectTransform.rect.height / 2f + buttonOk.rect.height / 2f + 20f);

                var buttonCancel= CreateButton("CancelButton", LocalizationManager.Instance.GetString("MessageBox", "Cancel"));
                buttonCancel.SetParent(rectTransform);
                buttonCancel.localScale=new Vector3(1f,1f,1f);
                buttonCancel.anchoredPosition = new Vector2(buttonOk.rect.width/2f+5f, -rectTransform.rect.height / 2f + buttonCancel.rect.height / 2f + 20f);

                buttonCancel.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Close();
                });
            }

            buttonOk.gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                Close();
                okAction?.Invoke();
            });
        }

        public static void Show(string text, Action retryAction, Action cancelAction)
        {
            if (_isShown)
                return;

            Show();

            var rectTransform = CreateBody(text);

            var buttonRetry = CreateButton("RetryButton", LocalizationManager.Instance.GetString("MessageBox", "Retry"));
            buttonRetry.SetParent(rectTransform);
            buttonRetry.localScale = new Vector3(1f, 1f, 1f);
            buttonRetry.anchoredPosition = new Vector2(-buttonRetry.rect.width/2f, -rectTransform.rect.height / 2f + buttonRetry.rect.height / 2f + 20f);

            buttonRetry.gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                Close();
                retryAction?.Invoke();
            });

            var buttonCancel= CreateButton("CancelButton", LocalizationManager.Instance.GetString("MessageBox", "Cancel"));
            buttonCancel.SetParent(rectTransform);
            buttonCancel.localScale=new Vector3(1f,1f,1f);
            buttonCancel.anchoredPosition = new Vector2(buttonRetry.rect.width/2f+5f, -rectTransform.rect.height / 2f + buttonCancel.rect.height / 2f + 20f);

            buttonCancel.gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                Close();
                cancelAction?.Invoke();
            });
        }

        public static void Show(string text)
        {
            Show(text, null);
        }

        private static RectTransform CreateBody(string message)
        {
            var parentTransform = GameObject.Find("Canvas").GetComponent<RectTransform>();

            _messageCanvas = new GameObject("MessageBoxCanvas");
            _messageCanvas.AddComponent<Canvas>();
            _messageCanvas.AddComponent<GraphicRaycaster>();
            _messageCanvas.AddComponent<MessageBoxAppearAnimation>();

            _messageCanvas.transform.SetParent(parentTransform);

            var rectTransform = _messageCanvas.GetComponent<RectTransform>();

            rectTransform.localScale = new Vector3(1f,1f,1f);

            if(_gc.Settings.IsPortraitMode)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentTransform.rect.width - 50f);
            else
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentTransform.rect.width / 2f);

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 200f);

            rectTransform.anchoredPosition = new Vector2(0f, 0f);

            var img = _messageCanvas.AddComponent<Image>();

            img.sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/UI/MessageBoxBackground");
            img.type = Image.Type.Sliced;

            var textObject = new GameObject("Message");
            var textTransform = textObject.AddComponent<RectTransform>();

            textTransform.SetParent(rectTransform);

            textTransform.localScale = new Vector3(1f,1f,1f);

            textTransform.anchoredPosition = new Vector2(0f,0f);

            textTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width - 30f);
            textTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectTransform.rect.height - 50f);

            var text = textObject.AddComponent<Text>();

            text.text = message;
            text.alignment = TextAnchor.UpperCenter;
            text.fontSize = _messageTextTemplate.fontSize;
            text.font = _messageTextTemplate.font;
            text.color = _messageTextTemplate.color;

            return rectTransform;
        }

        private static RectTransform CreateButton(string name, string text)
        {
            var buttonObject = new GameObject(name);
            var button = buttonObject.AddComponent<Button>();
            var buttonTransform = buttonObject.AddComponent<RectTransform>();

            buttonTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 144f);
            buttonTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 70f);

            buttonTransform.localScale = new Vector3(1f, 1f, 1f);

            var btnImg = buttonObject.AddComponent<Image>();

            button.targetGraphic = btnImg;

            btnImg.sprite = Resources.Load<Sprite>($"Skins/{_gc.SkinName}/UI/MessageBoxButton");
            btnImg.type = Image.Type.Sliced;

            var textObject = new GameObject("Text");
            var textTransform = textObject.AddComponent<RectTransform>();

            textTransform.SetParent(buttonTransform);

            textTransform.localScale = new Vector3(1f, 1f, 1f);

            textTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonTransform.rect.width - 5f);
            textTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonTransform.rect.height - 5f);

            var textComp = textObject.AddComponent<Text>();

            textComp.text = text;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontSize = _buttonTextTemplate.fontSize;
            textComp.font = _buttonTextTemplate.font;
            textComp.color = _buttonTextTemplate.color;

            return buttonTransform;
        }

        private static void Show()
        {
            GameObject.Find("Canvas").GetComponent<GraphicRaycaster>().enabled = false;

            _isShown = true;
        }

        private static void Close()
        {
            Object.Destroy(_messageCanvas);

            GameObject.Find("Canvas").GetComponent<GraphicRaycaster>().enabled = true;

            _isShown = false;
        }

    }
}
