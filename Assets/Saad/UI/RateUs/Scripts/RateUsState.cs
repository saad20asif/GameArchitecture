using UnityEngine;
using ProjectCore.Events;
using ProjectCore.StateMachine;

[CreateAssetMenu(fileName = "RateUsState", menuName = "ProjectCore/State Machine/States/RateUsState")]
public class RateUsState : UIViewState
{
    [SerializeField] private GameEvent GoToSpinWheelEvent;

    public void GoToSpinWheel()
    {
        GoToSpinWheelEvent.Invoke();
    }
}
