using UnityEngine;
using Blues.Core.Events;

[CreateAssetMenu(fileName = "GameState", menuName = "ProjectCore/State Machine/States/Normal Game State")]
public class NormalGameState : GameState
{
    [SerializeField] private GameEvent GoToLevelCompleteEvent;
    [SerializeField] private GameEvent GoToLeveLFailEvent;
    public void GoToLevelCompleteState()
    {
        GoToLevelCompleteEvent.Invoke();
    }

    public void GoToLevelFailState()
    {
        GoToLeveLFailEvent.Invoke();
    }
}
