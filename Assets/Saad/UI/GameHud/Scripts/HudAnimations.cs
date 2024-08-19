using DG.Tweening;
using UnityEngine;

namespace ProjectCore.GameHud
{
    public static class HudAnimations
    {
        public static void SlideInFromAbove(Vector2 initialPosition, RectTransform target, float duration, Ease ease)
        {
            target.anchoredPosition = new Vector2(initialPosition.x, initialPosition.y + target.rect.height);
            Debug.Log(initialPosition.y + target.rect.height);
            target.DOAnchorPos(initialPosition, duration).SetEase(ease);
        }

        public static void SlideOutAbove(RectTransform target, float duration, Ease ease)
        {
            target.DOAnchorPosY(target.anchoredPosition.y + target.rect.height, duration).SetEase(ease);
        }

        public static void SlideInFromBelow(Vector2 initialPosition, RectTransform target, float duration, Ease ease)
        {
            target.anchoredPosition = new Vector2(initialPosition.x, initialPosition.y - target.rect.height);
            target.DOAnchorPos(initialPosition, duration).SetEase(ease);
        }

        public static void SlideOutBelow(RectTransform target, float duration, Ease ease)
        {
            target.DOAnchorPosY(target.anchoredPosition.y - target.rect.height, duration).SetEase(ease);
        }

        // You can add more animations like FadeIn, FadeOut, etc.
    }
}
