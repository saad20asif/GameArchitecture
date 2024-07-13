using DG.Tweening;
using UnityEngine;

namespace ProjectCore.UI
{
    public abstract class UiBase : UiAnimations, IShowable
    {
        private CanvasGroup _canvasGroup;
        [SerializeField] protected RectTransform UIPanel;
        [SerializeField] protected float fadeDuration = 0.5f;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        public virtual void Show()
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            FadeIn(_canvasGroup);
            ScaleIn(UIPanel);
        }

        public virtual void Hide()
        {
            ScaleOut(UIPanel).OnComplete(() =>
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                Destroy(gameObject);
            });
        }

        public virtual void Pause()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public virtual void Resume()
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}
