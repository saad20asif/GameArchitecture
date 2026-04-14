using System;
using CustomEditorScripts;
using DG.Tweening;
using Blues.Core.StateMachine;
using Sirenix.OdinInspector;
using System.Collections;
using Blues.Core.UI;
using Blues.Core.Variables;
using UnityEngine;

namespace Blues.Core.GameHud
{
    public class GameHud : MonoBehaviour,IShowable
    {
        [ColorFoldoutGroup("BaseHud", 1, 1, 4)]
        [SerializeField] private UiConfig HudBarsConfig;

        [ColorFoldoutGroup("BaseHud", 1, 1, 4)]
        [SerializeField] private RectTransform Header;

        [ColorFoldoutGroup("BaseHud", 1, 1, 4)]
        [SerializeField] private RectTransform Footer;

        [ColorFoldoutGroup("BaseHud", 1, 1, 4)]
        [SerializeField] private RectTransform Middle;

        private Vector2 _headerInitialPosition;
        private Vector2 _footerInitialPosition;

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;

        [SerializeField] private Int currentStateSortingOrder;

        /// <summary>
        /// When false, Show/Hide/Pause/Resume skip the default header/footer slide animations.
        /// Set by GameState before calling Show(). Subclasses can override
        /// OnCustomShow / OnCustomHide / OnCustomPause / OnCustomResume for their own animations.
        /// </summary>
        public bool UseDefaultAnimations { get; set; } = true;

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
            // Store the initial positions of Header and Footer
            _headerInitialPosition = Header.anchoredPosition;
            _footerInitialPosition = Footer.anchoredPosition;
        }

        public virtual void Show()
        {
            if(currentStateSortingOrder != null)
                _canvas.sortingOrder = currentStateSortingOrder.GetValue();

            _canvas.planeDistance = 5;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            if (UseDefaultAnimations)
            {
                HudAnimations.SlideInFromAbove(_headerInitialPosition, Header, HudBarsConfig.easeInDuration, HudBarsConfig.easeIn);
                HudAnimations.SlideInFromBelow(_headerInitialPosition, Footer, HudBarsConfig.easeInDuration, HudBarsConfig.easeIn);
            }
            else
            {
                OnCustomShow();
            }
        }

        public bool Paused { get; set; }

        public IEnumerator Hide()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            // Already paused (hidden/non-interactable) — skip all animations, just deactivate
            if (Paused)
            {
                DOTween.Kill(Header);
                DOTween.Kill(Footer);
            }
            else if (UseDefaultAnimations)
            {
                bool completed = false;

                Sequence hideSequence = DOTween.Sequence();

                hideSequence
                    .Append(HudAnimations.SlideOutAbove(Header, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut))
                    .Join(HudAnimations.SlideOutBelow(Footer, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut))
                    .OnComplete(() =>
                    {
                        completed = true;
                    });

                yield return new WaitUntil(() => completed);

                DOTween.Kill(Header);
                DOTween.Kill(Footer);
            }
            else
            {
                yield return OnCustomHide();
            }
        }

        public virtual void Resume()
        {
            Paused = false;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            if (UseDefaultAnimations)
                Show();
            else
                OnCustomResume();
        }

        public virtual void Pause()
        {
            Paused = true;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (UseDefaultAnimations)
            {
                HideGameHudBars();
            }
            else
            {
                OnCustomPause();
            }
        }

        /// <summary>Override for custom enter animation when UseDefaultAnimations is false.</summary>
        protected virtual void OnCustomShow() { }

        /// <summary>Override for custom exit animation when UseDefaultAnimations is false.
        /// Yield until your animation finishes — GameState waits for this.</summary>
        protected virtual IEnumerator OnCustomHide() { yield break; }

        /// <summary>Override for custom pause animation when UseDefaultAnimations is false.</summary>
        protected virtual void OnCustomPause() { }

        /// <summary>Override for custom resume animation when UseDefaultAnimations is false.</summary>
        protected virtual void OnCustomResume() { }

        private void HideGameHudBars(Action callback = null)
        {
            Sequence hideSequence = DOTween.Sequence();

            hideSequence.Append(HudAnimations.SlideOutAbove(Header, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut))
                        .Join(HudAnimations.SlideOutBelow(Footer, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut))
                        .OnComplete(() =>
                        {
                            callback?.Invoke();
                        });
        }

        private IEnumerator UnloadAssets()
        {
            yield return null;
            yield return Resources.UnloadUnusedAssets();
        }
    }
}
