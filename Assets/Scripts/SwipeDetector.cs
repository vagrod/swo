using System;
using UnityEngine;

public class SwipeDetector : MonoBehaviour
{

    public float minSwipeDistY;
    public float minSwipeDistX;
    public Action OnSwipeLeftToRight{ get; set; }
    public Action OnSwipeRightToLeft { get; set; }
    public Action OnSwipeUpToDown{ get; set; }
    public Action OnSwipeDownToUp { get; set; }

    private Vector2 startPos;

    void Update()
    {
        if (Input.touchCount > 0)
        {

            var touch = Input.touches[0];

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startPos = touch.position;

                    break;
                case TouchPhase.Ended:
                    float swipeDistVertical = (new Vector3(0, touch.position.y, 0) - new Vector3(0, startPos.y, 0)).magnitude;

                    if (swipeDistVertical > minSwipeDistY)
                    {
                        float swipeValue = Mathf.Sign(touch.position.y - startPos.y);

                        if (swipeValue > 0)//up swipe
                        {
                            OnSwipeDownToUp?.Invoke();
                        }

                        else if (swipeValue < 0)//down swipe
                        {
                            OnSwipeUpToDown?.Invoke();
                        }
                    }

                    float swipeDistHorizontal = (new Vector3(touch.position.x, 0, 0) - new Vector3(startPos.x, 0, 0)).magnitude;

                    if (swipeDistHorizontal > minSwipeDistX)
                    {
                        float swipeValue = Mathf.Sign(touch.position.x - startPos.x);

                        if (swipeValue > 0)//right swipe
                        {
                            OnSwipeLeftToRight?.Invoke();
                        }
                        else if (swipeValue < 0)//left swipe
                        {
                            OnSwipeRightToLeft?.Invoke();
                        }

                    }
                    break;
            }
        }
    }
}
