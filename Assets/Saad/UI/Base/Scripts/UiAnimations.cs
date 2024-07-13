using DG.Tweening;
using UnityEngine;

public class UiAnimations : MonoBehaviour
{
    [SerializeField] private UiConfig uiConfig;

    public void FadeIn(CanvasGroup canvasGroup)
    {
        canvasGroup.DOFade(1, uiConfig.easeInDuration).SetEase(uiConfig.easeIn);
    }

    public void FadeOut(CanvasGroup canvasGroup, System.Action onComplete)
    {
        canvasGroup.DOFade(0, uiConfig.easeOutDuration).SetEase(uiConfig.easeOut).OnComplete(() => onComplete?.Invoke());
    }

    public Tweener ScaleIn(RectTransform uiPanel)
    {
        uiPanel.localScale = Vector3.zero;
        return uiPanel.DOScale(Vector3.one, uiConfig.easeInDuration).SetEase(uiConfig.easeIn);
    }

    public Tweener ScaleOut(RectTransform uiPanel)
    {
        return uiPanel.DOScale(Vector3.zero, uiConfig.easeOutDuration).SetEase(uiConfig.easeOut);
    }

    public Tweener SlideIn(RectTransform uiPanel)
    {
        // Slide in from the right
        Vector3 startPos = new Vector3(Screen.width, uiPanel.localPosition.y, uiPanel.localPosition.z);
        uiPanel.localPosition = startPos;
        return uiPanel.DOLocalMoveX(0, uiConfig.easeInDuration).SetEase(uiConfig.easeIn);
    }

    public Tweener SlideOut(RectTransform uiPanel)
    {
        // Slide out to the left
        Vector3 endPos = new Vector3(-Screen.width, uiPanel.localPosition.y, uiPanel.localPosition.z);
        return uiPanel.DOLocalMove(endPos, uiConfig.easeOutDuration).SetEase(uiConfig.easeOut);
    }
}
