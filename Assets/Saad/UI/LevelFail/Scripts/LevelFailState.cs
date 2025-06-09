using ProjectCore.Events;
using ProjectCore.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelFailState", menuName = "ProjectCore/State Machine/States/LevelFail State")]
public class LevelFailState : UIViewState
{
    [SerializeField] private GameEventWithInt GoToMainMenuEvent;
    
    public void GotoMainMenu()
    {
        GoToMainMenuEvent.Raise(1);
    }
}
