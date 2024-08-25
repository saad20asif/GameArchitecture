using CustomEditorScripts;
using ProjectCore.UI;
using Sirenix.OdinInspector;
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
            //Debug.Log("GameHud Hide called!");

            HudAnimations.SlideOutAbove(Header, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut);
            HudAnimations.SlideOutBelow(Footer, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut);
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
