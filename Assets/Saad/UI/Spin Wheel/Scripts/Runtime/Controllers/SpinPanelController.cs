using Blues.Core.Events;
using DG.Tweening;
using UnityEngine;

namespace Featrues.SpinWheel
{    
    public class SpinPanelController : MonoBehaviour
    {
        [SerializeField] private SpinWheelConfigurations spinWheelConfigurations;
        [SerializeField] private GameEvent OnSpinWheelDataLoaded;

        private void OnEnable()
        {
            if(spinWheelConfigurations != null)
            {
                if(spinWheelConfigurations.LoadDataFromJson())
                {
                    DOVirtual.DelayedCall(0.2f , ()=>
                    {
                        OnSpinWheelDataLoaded.Invoke();
                    });
                }
            }
        }
    }
}