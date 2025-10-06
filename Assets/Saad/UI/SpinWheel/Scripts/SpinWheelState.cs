using Blues.Core.Events;
using Blues.Core.StateMachine;
using Blues.Core.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "SpinWheelState", menuName = "ProjectCore/State Machine/States/SpinWheelState")]
public class SpinWheelState : UIViewState
{
    [SerializeField] private GameEvent backBtnPressedEvent;
    [SerializeField] private GameEvent GoToRateUsEvent;

    public void GoBack()
    {
        backBtnPressedEvent.Invoke();
    }
    public void GoToRateUs()
    {
        GoToRateUsEvent.Invoke();
    }
}
