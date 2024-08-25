using CustomEditorScripts;
using DG.Tweening;
using ProjectCore.UI;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace ProjectCore.GameHud
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

            HudAnimations.SlideInFromAbove(_headerInitialPosition, Header, HudBarsConfig.easeInDuration, HudBarsConfig.easeIn);
            HudAnimations.SlideInFromBelow(_headerInitialPosition, Footer, HudBarsConfig.easeInDuration, HudBarsConfig.easeIn);
        }

        public virtual void Hide()
        {
            // Debug.Log("GameHud Hide called!");

            // Slide out animations for Header and Footer
            Sequence hideSequence = DOTween.Sequence();

            hideSequence.Append(HudAnimations.SlideOutAbove(Header, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut))
                        .Join(HudAnimations.SlideOutBelow(Footer, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut))
                        .OnComplete(() =>
                        {
                            // Call the coroutine to unload assets after animations complete
                            StartCoroutine(UnloadAssets());
                        });
            Destroy(gameObject);
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
        public virtual void Resume()
        {
            print("Resssss ");
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            Show();
        }
        public virtual void Pause()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            Hide();
        }
    }
}
