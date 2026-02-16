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
            //Debug.Log("GameHud Show called!");
            if(currentStateSortingOrder != null) 
                _canvas.sortingOrder = currentStateSortingOrder.GetValue();
            
            _canvas.planeDistance = 5;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            HudAnimations.SlideInFromAbove(_headerInitialPosition, Header, HudBarsConfig.easeInDuration, HudBarsConfig.easeIn);
            HudAnimations.SlideInFromBelow(_headerInitialPosition, Footer, HudBarsConfig.easeInDuration, HudBarsConfig.easeIn);
        }

        public IEnumerator Hide()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

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
        private void HideGameHudBars(Action callback = null)
        {
            Sequence hideSequence = DOTween.Sequence();

            hideSequence.Append(HudAnimations.SlideOutAbove(Header, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut))
                        .Join(HudAnimations.SlideOutBelow(Footer, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut))
                        .OnComplete(() =>
                        {
                            callback.Invoke();
                            // Call the coroutine to unload assets after animations complete
                            //StartCoroutine(UnloadAssets());
                        });
        }

        private IEnumerator UnloadAssets()
        {
            // Wait a frame to ensure the Destroy() call has been processed
            yield return null;
            // Call Resources.UnloadUnusedAssets to free up memory
            yield return Resources.UnloadUnusedAssets();
        }
        public virtual void Resume()
        {
            //print("Resssss ");
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            Show();
        }
        public virtual void Pause()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            HideGameHudBars(() =>
            {
                //print("Gamebars hided!");
            });
        }
    }
}
