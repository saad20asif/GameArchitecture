using UnityEngine;
using Blues.Core.Input.SwipeSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SwipeDetector swipeDetector;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float maxMoveSpeed = 15f;

    private void OnEnable()
    {
        swipeDetector.OnSwipeDetected += HandleSwipe;
    }

    private void OnDisable()
    {
        swipeDetector.OnSwipeDetected -= HandleSwipe;
    }

    private void HandleSwipe(SwipeData swipeData)
    {
        float speed = Mathf.Clamp(swipeData.Intensity / 1000f, baseMoveSpeed, maxMoveSpeed);
        Debug.Log($"Swipe {swipeData.Direction} with intensity {swipeData.Intensity}, move speed set to {speed}, duration {swipeData.Duration}");

        // Trigger movement logic here based on direction and speed
        // e.g. StartMove(swipeData.Direction, speed);
    }
}
