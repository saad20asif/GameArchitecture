using UnityEngine;

namespace Core.UI.TextAnimations
{
    [CreateAssetMenu(
        fileName = "TMPTextAnimationConfig",
        menuName = "UI/Text Animation/TMP Text Animation Config"
    )]
    public class TMPTextAnimationConfig : ScriptableObject
    {
        [Header("Timing")]
        public float characterDelay = 0.05f;
        public float totalDuration = 0.35f;

        [Header("Scale")]
        public float overshootScale = 1.5f;

        [Header("Ease (ONLY easing)")]
        public AnimationCurve easeCurve =
            AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Curvature")]
        [Tooltip("Positive = arch up, Negative = arch down, 0 = flat")]
        public float curveHeight = 0f;
    }
}
