using System;
using DG.Tweening;
using UnityEngine;
using ProjectCore.StateMachine;

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

                _canvas.planeDistance = 5;
                _canvas.sortingOrder = FiniteStateMachine.CurrentStateSortingOrder;
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            FadeIn(_canvasGroup);
            ScaleIn(UIPanel);
            
        }

        public virtual void Hide(Action callback)
        {
            if (_canvasGroup == null || UIPanel == null)
            {
                Debug.LogWarning("CanvasGroup or UIPanel is not assigned.");
                return;
            }

            // Start the scale-out animation
            ScaleOut(UIPanel).OnComplete(() =>
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false); // ✅ Reuse instead of Destroy
                callback.Invoke();
            });

            DOTween.Kill(this);
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
