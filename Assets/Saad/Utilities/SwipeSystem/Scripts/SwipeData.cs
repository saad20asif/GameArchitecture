namespace Blues.Core.Input.SwipeSystem
{
    public struct SwipeData
    {
        public SwipeDirection Direction;
        public float Intensity; // e.g. swipe distance / duration or velocity
        public float Duration;    // time in seconds the swipe lasted

        public SwipeData(SwipeDirection dir, float intensity, float duration)
        {
            Direction = dir;
            Intensity = intensity;
            Duration = duration;
        }
    }
}
