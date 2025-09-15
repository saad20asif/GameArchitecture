using System;
using UnityEngine;
using ProjectCore.Variables;

namespace ProjectCore.UI
{
    public abstract class UiBase : MonoBehaviour, IShowable
    {
        [SerializeField] protected StateAnimationConfig animationConfig;
        [SerializeField] protected RectTransform UIPanel;
        [SerializeField] private Int currentStateSortingOrder;
        
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;

        private UiAnimationSystem _animationSystem;
        
        // Track animation state
        private Action _pendingHideCallback;
        private bool _isHiding;
        
        protected virtual void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            _canvas.sortingOrder = currentStateSortingOrder.GetValue();
            _canvas.planeDistance = 5;
            
            if (_canvas.worldCamera == null)
                _canvas.worldCamera = Camera.main;
            
            _animationSystem = new UiAnimationSystem(this, _canvasGroup, UIPanel, animationConfig);
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            _animationSystem.PlayAnimation(AnimationPhase.Enter, () =>
            {
                MakeStateInteractable(true);
            });
        }

        public virtual void Hide(Action callback)
        {
            if (_isHiding)
            {
                // If already hiding, queue the new callback
                var originalCallback = _pendingHideCallback;
                _pendingHideCallback = () =>
                {
                    originalCallback?.Invoke();
                    callback?.Invoke();
                };
                return;
            }
            
            _isHiding = true;
            _pendingHideCallback = callback;
            MakeStateInteractable(false);
            _animationSystem.PlayAnimation(AnimationPhase.Exit, () =>
            {
                OnHideComplete();
                _pendingHideCallback?.Invoke();
                _isHiding = false;
                _pendingHideCallback = null;
            });
        }

        private void OnHideComplete()
        {
            gameObject.SetActive(false);
        }

        public virtual void Pause()
        {
            if (!Paused)
            {
                MakeStateInteractable(false);
                Paused = true;
                
                // Complete any ongoing animations before pausing
                _animationSystem.ForceCompleteCurrentAnimation();
            }
        }

        public virtual void Resume()
        {
            if (Paused)
            {
                Paused = false;
                
                // Complete pause animation before resuming
                _animationSystem.ForceCompleteCurrentAnimation();
                MakeStateInteractable(true);
            }
        }

        protected void MakeStateInteractable(bool flag)
        {
            _canvasGroup.interactable = flag;
            _canvasGroup.blocksRaycasts = flag;
        }

        public bool Paused { get; set; }
    }
}