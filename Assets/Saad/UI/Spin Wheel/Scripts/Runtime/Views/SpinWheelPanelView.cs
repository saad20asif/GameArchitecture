using Blues.Core.Events;
using Blues.Core.Variables;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Featrues.SpinWheel
{
    public class SpinWheelPanelView : BaseView<SpinWheelViewRefs>
    {
        [SerializeField] private Int playerCoins;
        [SerializeField] private Int multiplier;
        [SerializeField] private GameEvent playerCoinsChanged;
        [SerializeField] private GameEvent multiplierChanged;
        [SerializeField] private GameEvent WheelDataLoaded;
        [SerializeField] private GameEvent OnSpinPressed;
        [SerializeField] private GameEvent OnSpinEnded;
        [SerializeField] private SpinWheelConfigurations spinWheelConfigurations;

        private void OnEnable()
        {
            Subscribe();
        }
        private void OnDisable()
        {
            UnSubscribe();
        }
        [Button]
        public override void Show()
        { 
            this.gameObject.SetActive(true);
        }
        [Button]
        public override void Hide()
        {
            this.gameObject.SetActive(false);
        }

        private void Subscribe()
        {
            ViewRefs.spinButton.onClick.AddListener(StartSpinning);
            OnSpinEnded.Subscribe(EnableSpinButton);
            playerCoinsChanged.Subscribe(CoinsChanged);
            multiplierChanged.Subscribe(MultiplierChanged);
            WheelDataLoaded.Subscribe(UpdateViewUI);
        }
        private void UnSubscribe()
        {
            ViewRefs.spinButton.onClick.RemoveListener(StartSpinning);
            OnSpinEnded.UnSubscribe(EnableSpinButton);
            playerCoinsChanged.UnSubscribe(CoinsChanged);
            multiplierChanged.UnSubscribe(MultiplierChanged);
            WheelDataLoaded.UnSubscribe(UpdateViewUI);
        }
        private void CoinsChanged()
        {
            ViewRefs.currentCoins.text = playerCoins.GetValue().ToString();
        }
        private void MultiplierChanged()
        {
            ViewRefs.currentMultiplier.text = "x" + multiplier.GetValue().ToString();
        }
        private void UpdateViewUI()
        {
            CoinsChanged();
            MultiplierChanged();
        }
        private void StartSpinning()
        {
            OnSpinPressed.Invoke();
            ViewRefs.spinButton.interactable = false;
        }
        private void EnableSpinButton()
        {
            ViewRefs.spinButton.interactable = true;
        } 

    }
}
