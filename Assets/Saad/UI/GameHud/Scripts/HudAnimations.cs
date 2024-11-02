using DG.Tweening;
using UnityEngine;

namespace ProjectCore.GameHud
{
    public static class HudAnimations
    {
        public static Tween SlideInFromAbove(Vector2 initialPosition, RectTransform target, float duration, Ease ease)
        {
            target.anchoredPosition = new Vector2(initialPosition.x, initialPosition.y + target.rect.height);
            return target.DOAnchorPos(initialPosition, duration).SetEase(ease);
        }

        public static Tween SlideOutAbove(RectTransform target, float duration, Ease ease)
        {
            return target.DOAnchorPosY(target.anchoredPosition.y + target.rect.height, duration).SetEase(ease);
        }

        public static Tween SlideInFromBelow(Vector2 initialPosition, RectTransform target, float duration, Ease ease)
        {
            target.anchoredPosition = new Vector2(initialPosition.x, initialPosition.y - target.rect.height);
            return target.DOAnchorPos(initialPosition, duration).SetEase(ease);
        }

        public static Tween SlideOutBelow(RectTransform target, float duration, Ease ease)
        {
            return target.DOAnchorPosY(target.anchoredPosition.y - target.rect.height, duration).SetEase(ease);
        }

        // Add more animations if needed
    }

}
