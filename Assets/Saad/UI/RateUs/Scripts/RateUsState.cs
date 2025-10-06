using UnityEngine;
using Blues.Core.Events;
using Blues.Core.StateMachine;

[CreateAssetMenu(fileName = "RateUsState", menuName = "ProjectCore/State Machine/States/RateUsState")]
public class RateUsState : UIViewState
{
    [SerializeField] private GameEvent backBtnPressedEvent;

    public void GoBack()
    {
        backBtnPressedEvent.Invoke();
    }
}
