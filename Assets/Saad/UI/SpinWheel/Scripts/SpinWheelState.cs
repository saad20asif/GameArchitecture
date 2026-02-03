using System.Collections;
using Blues.Core.Events;
using Blues.Core.StateMachine;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "SpinWheelState", menuName = "ProjectCore/State Machine/States/SpinWheelState")]
public class SpinWheelState : UIViewState
{
    [SerializeField] private GameEvent backBtnPressedEvent;
    [SerializeField] private Featrues.SpinWheel.SpinWheelConfigurations spinWheelConfigurations;
    [SerializeField] private GameEvent OnSpinWheelDataLoaded;
    
    public void GoBack()
    {
        backBtnPressedEvent.Invoke();
    }

    public override IEnumerator Enter(IState previous)
    {
        LoadWheelData();
        return base.Enter(previous);
    }
    private void LoadWheelData()
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
