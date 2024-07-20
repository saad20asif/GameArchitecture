using ProjectCore.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "SpinWheelState", menuName = "ProjectCore/State Machine/States/SpinWheelState")]
public class SpinWheelState : UiViewState
{
    [SerializeField] private GameEvent GoToMainMenuEvent;

    public void GoToMainMenu()
    {
        GoToMainMenuEvent.Invoke();
    }
}
