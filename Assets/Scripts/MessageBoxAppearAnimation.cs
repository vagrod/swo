using UnityEngine;

namespace Assets.Scripts
{
    public class MessageBoxAppearAnimation : MonoBehaviour
    {

        private const float AnimationDuration = 0.1f;
        private const float InitialScale = 0.4f;
        private const float ScaleDelta = 1f - InitialScale;

        private float _animationCounter;

        void Start()
        {
            gameObject.transform.localScale = new Vector3(InitialScale, InitialScale, InitialScale);
        }

        void Update()
        {
            if (_animationCounter >= AnimationDuration)
            {
                this.enabled = false;

                gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

                _animationCounter = 0f;

                return;
            }

            _animationCounter += Time.deltaTime;

            var p = _animationCounter / AnimationDuration;
            var current = ScaleDelta * p;

            gameObject.transform.localScale = new Vector3(InitialScale + current, InitialScale + current, InitialScale + current);
        }

    }
}
