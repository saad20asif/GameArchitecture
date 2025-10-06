using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blues.Core.Input.SwipeSystem
{
    public class SwipeDetector : MonoBehaviour
{
    public event Action<SwipeData> OnSwipeDetected;

    [Tooltip("Minimum swipe distance (screen pixels) to consider a swipe")]
    public float minSwipeDistance = 50f;

    [Tooltip("Maximum swipe duration (seconds)")]
    public float maxSwipeDuration = 0.5f;

    private Vector2 startPos;
    private float startTime;
    private bool trackingSwipe = false;
    private bool swipeFiredThisDrag = false;

    private Vector2 currentPos;

    private InputAction touchPositionAction;
    private InputAction touchPressAction;

    private void Awake()
    {
        touchPositionAction = new InputAction(type: InputActionType.Value, binding: "<Touchscreen>/touch0/position");
        touchPressAction = new InputAction(type: InputActionType.Button, binding: "<Touchscreen>/touch0/press");

        touchPressAction.started += OnTouchStarted;
        touchPressAction.canceled += OnTouchCanceled;

        touchPositionAction.performed += OnTouchMoved;

        touchPositionAction.Enable();
        touchPressAction.Enable();
    }

    private void OnDestroy()
    {
        touchPressAction.started -= OnTouchStarted;
        touchPressAction.canceled -= OnTouchCanceled;
        touchPositionAction.performed -= OnTouchMoved;

        touchPressAction.Disable();
        touchPositionAction.Disable();
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        startPos = touchPositionAction.ReadValue<Vector2>();
        currentPos = startPos;
        startTime = Time.time;
        trackingSwipe = true;
        swipeFiredThisDrag = false; // Reset on new drag
    }

    private void OnTouchMoved(InputAction.CallbackContext context)
    {
        if (!trackingSwipe || swipeFiredThisDrag)
            return;

        currentPos = context.ReadValue<Vector2>();
        float duration = Time.time - startTime;
        Vector2 delta = currentPos - startPos;

        if (duration <= maxSwipeDuration && delta.magnitude >= minSwipeDistance)
        {
            SwipeDirection direction = CalculateSwipeDirection(delta);
            float intensity = delta.magnitude / duration;

            swipeFiredThisDrag = true;

            OnSwipeDetected?.Invoke(new SwipeData(direction, intensity, duration));
        }

    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        trackingSwipe = false;
        swipeFiredThisDrag = false; // Ready for next swipe on next drag
    }

    private SwipeDirection CalculateSwipeDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        else
            return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
    }
}
}

