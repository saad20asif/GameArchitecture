using UnityEngine;
using ProjectCore.Events;

[CreateAssetMenu(fileName = "RateUsState", menuName = "ProjectCore/State Machine/States/RateUsState")]
public class RateUsState : UiViewState
{
    [SerializeField] private GameEvent GoToSpinWheelEvent;

    public void GoToSpinWheel()
    {
        GoToSpinWheelEvent.Invoke();
    }
}
