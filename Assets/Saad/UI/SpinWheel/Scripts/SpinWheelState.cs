using ProjectCore.Events;
using ProjectCore.StateMachine;
using ProjectCore.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "SpinWheelState", menuName = "ProjectCore/State Machine/States/SpinWheelState")]
public class SpinWheelState : UIViewState
{
    [SerializeField] private GameEventWithInt GoToMainMenuEvent;
    [SerializeField] private GameEvent GoToRateUsEvent;

    public void GoToMainMenu()
    {
        int index = (int)UICloseReasons.FullScreenPlacement;
        GoToMainMenuEvent.Raise(index);
    }
    public void GoToRateUs()
    {
        GoToRateUsEvent.Invoke();
    }
}
