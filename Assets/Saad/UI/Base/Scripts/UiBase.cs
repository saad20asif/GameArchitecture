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

        /// <summary>
        /// When false, Show/Hide skip the default animation system entirely.
        /// Set by UIViewState before calling Show(). Subclasses can override
        /// OnCustomShow / OnCustomHide to play their own animations.
        /// </summary>
        public bool UseDefaultAnimations { get; set; } = true;
        
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

            if (UseDefaultAnimations)
            {
                _animationSystem.PlayAnimation(AnimationPhase.Enter, () =>
                {
                    MakeStateInteractable(true);
                });
            }
            else
            {
                // No default animation — make interactable immediately,
                // then let subclass run custom enter animation.
                MakeStateInteractable(true);
                OnCustomShow();
            }
        }

        public virtual IEnumerator Hide()
        {
            if (_isHiding)
                yield break;

            _isHiding = true;
            MakeStateInteractable(false);

            // Already paused (hidden/non-interactable) — skip all animations, just deactivate
            if (Paused)
            {
                // no-op: fall through to SetActive(false)
            }
            else if (UseDefaultAnimations)
            {
                bool completed = false;
                _animationSystem.PlayAnimation(AnimationPhase.Exit, () =>
                {
                    completed = true;
                });
                yield return new WaitUntil(() => completed);
            }
            else
            {
                yield return OnCustomHide();
            }

            gameObject.SetActive(false);
            _isHiding = false;
        }

        public virtual void Pause()
        {
            if (!Paused)
            {
                MakeStateInteractable(false);
                Paused = true;

                if (UseDefaultAnimations)
                    _animationSystem.ForceCompleteCurrentAnimation();

                OnCustomPause();
            }
        }

        public virtual void Resume()
        {
            if (Paused)
            {
                Paused = false;

                if (UseDefaultAnimations)
                    _animationSystem.ForceCompleteCurrentAnimation();

                MakeStateInteractable(true);
                OnCustomResume();
            }
        }

        /// <summary>Override to play a custom enter animation when UseDefaultAnimations is false.</summary>
        protected virtual void OnCustomShow() { }

        /// <summary>Override to play a custom exit animation when UseDefaultAnimations is false.
        /// Yield until your animation finishes — FSM waits for this.</summary>
        protected virtual IEnumerator OnCustomHide() { yield break; }

        /// <summary>Override for custom pause behaviour (called regardless of UseDefaultAnimations).</summary>
        protected virtual void OnCustomPause() { }

        /// <summary>Override for custom resume behaviour (called regardless of UseDefaultAnimations).</summary>
        protected virtual void OnCustomResume() { }

        protected void MakeStateInteractable(bool flag)
        {
            //_canvasGroup.interactable = flag;
            _canvasGroup.blocksRaycasts = flag;
        }

        public bool Paused { get; set; }
    }
}