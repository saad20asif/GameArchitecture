namespace Blues.Core.Input.SwipeSystem
{
    public interface ISwipeDetector
    {
        event System.Action<SwipeData> OnSwipeDetected;
        void Enable();
        void Disable();
    }
}
