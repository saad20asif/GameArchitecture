using ProjectCore.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCompleteState", menuName = "ProjectCore/State Machine/States/LevelComplete State")]
public class LevelCompleteState : UiViewState
{
    [SerializeField] private GameEvent GoToMainMenuEvent;
   
    public void GoToMainMenu()
    {
        GoToMainMenuEvent.Invoke();
    }
}
