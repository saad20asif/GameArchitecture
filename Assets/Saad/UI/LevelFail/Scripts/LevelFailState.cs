using UnityEngine;

[CreateAssetMenu(fileName = "LevelFailState", menuName = "ProjectCore/State Machine/States/LevelFail State")]
public class LevelFailState : UiViewState
{
    [SerializeField] private GameEvent GoToMainMenuEvent;
    
    public void GotoMainMenu()
    {
        GoToMainMenuEvent.Invoke();
    }
}
