using Blues.Core.Events;
using Blues.Core.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCompleteState", menuName = "ProjectCore/State Machine/States/LevelComplete State")]
public class LevelCompleteState : UIViewState
{
    [SerializeField] private GameEventWithInt GoToMainMenuEvent;
   
    public void GoToMainMenu()
    {
        GoToMainMenuEvent.Raise(1);
    }
}
