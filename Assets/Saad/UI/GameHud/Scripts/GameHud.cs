using CustomEditorScripts;
using ProjectCore.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectCore.GameHud
{
    public class GameHud : MonoBehaviour
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

        private void Awake()
        {
            // Store the initial positions of Header and Footer
            _headerInitialPosition = Header.anchoredPosition;
            _footerInitialPosition = Footer.anchoredPosition;
        }

        public virtual void Show()
        {
            //Debug.Log("GameHud Show called!");

            HudAnimations.SlideInFromAbove(Header, HudBarsConfig.easeInDuration, HudBarsConfig.easeIn);
            HudAnimations.SlideInFromBelow(Footer, HudBarsConfig.easeInDuration, HudBarsConfig.easeIn);
        }

        public virtual void Hide()
        {
            //Debug.Log("GameHud Hide called!");

            HudAnimations.SlideOutAbove(Header, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut);
            HudAnimations.SlideOutBelow(Footer, HudBarsConfig.easeOutDuration, HudBarsConfig.easeOut);
        }
    }
}
