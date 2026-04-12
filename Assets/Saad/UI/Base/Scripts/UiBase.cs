using System;
using System.Collections;
using UnityEngine;

namespace Blues.Core.UI
{
    public abstract class UIBase : MonoBehaviour, IShowable
    {
        [SerializeField] protected StateAnimationConfig animationConfig;
        [SerializeField] protected RectTransform UIPanel;

        private int _sortingOrder;
        
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;

        private UiAnimationSystem _animationSystem;
        
        // Track animation state
        private Action _pendingHideCallback;
        private bool _isHiding;
        
        protected virtual void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            // Validate setup
            if (_canvas.worldCamera == null) _canvas.worldCamera = Camera.main;
            if (UIPanel == null) UIPanel = GetComponent<RectTransform>();

            _animationSystem = new UiAnimationSystem(this, _canvasGroup, UIPanel, animationConfig);
        }

        // Called by UIViewState before Show() so the canvas lands on the correct layer
        public void SetSortingOrder(int order)
        {
            _sortingOrder = order;
        }

        public virtual void Show()
        {
            _canvas.sortingOrder = _sortingOrder;
            
            _canvas.planeDistance = 5;
            gameObject.SetActive(true);
            _animationSystem.PlayAnimation(AnimationPhase.Enter, () =>
            {
                MakeStateInteractable(true);
            });
        }

        public virtual IEnumerator Hide()
        {
            if (_isHiding)
                yield break;

            _isHiding = true;
            MakeStateInteractable(false);

            bool completed = false;

            _animationSystem.PlayAnimation(AnimationPhase.Exit, () =>
            {
                completed = true;
            });

            // FSM WAITS here
            yield return new WaitUntil(() => completed);

            gameObject.SetActive(false);
            _isHiding = false;
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
            //_canvasGroup.interactable = flag;
            _canvasGroup.blocksRaycasts = flag;
        }

        public bool Paused { get; set; }
    }
}