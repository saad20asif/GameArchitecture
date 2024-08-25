using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectCore.UI
{
    public abstract class UiBase : UiAnimations, IShowable
    {
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        [SerializeField] protected RectTransform UIPanel;
        [SerializeField] protected float fadeDuration = 0.5f;
        [SerializeField] protected bool Paused = false;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvas = GetComponent<Canvas>();
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvas.worldCamera == null)
                {
                    _canvas.worldCamera = Camera.main;
                }
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
            if (_canvasGroup == null || UIPanel == null)
            {
                Debug.LogWarning("CanvasGroup or UIPanel is not assigned.");
                Destroy(gameObject);
                return;
            }

            // Start the scale-out animation
            ScaleOut(UIPanel).OnComplete(() =>
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                Destroy(gameObject);
                StartCoroutine(UnloadAssets());
            });
            // Optionally kill any ongoing DOTween animations associated with this UI element
            DOTween.Kill(this);
        }

        private IEnumerator UnloadAssets()
        {
            // Wait a frame to ensure the Destroy() call has been processed
            yield return null;
            // Call Resources.UnloadUnusedAssets to free up memory
            yield return Resources.UnloadUnusedAssets();
        }


        public virtual void Pause()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            Paused = true;
        }

        public virtual void Resume()
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            Paused = false;
        }
    }
}
