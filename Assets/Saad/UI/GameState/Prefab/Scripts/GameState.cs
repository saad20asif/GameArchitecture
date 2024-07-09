using ProjectCore.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "ProjectCore/State Machine/States/Game State")]
public class GameState : UiViewState
{
    [SerializeField] private GameEvent GoToLevelCompleteEvent;
    [SerializeField] private GameEvent GoToLevelFailEvent;
    
    public void GoToLevelCompleteState()
    {
        Debug.Log("Go to Level Complete Called!");
        GoToLevelCompleteEvent.Invoke();
    }
    public void GoToLevelFailState()
    {
        Debug.Log("Go to Level Complete Called!");
        GoToLevelFailEvent.Invoke();
    }
}
