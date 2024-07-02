using UnityEngine;

[CreateAssetMenu(fileName = "MainMenuState", menuName = "ProjectCore/State Machine/States/MainMenuState")]
public class MainMenuState : UiViewState
{
    [SerializeField] private GameEvent GoToGameEvent;
    
    public void GoToPlayState()
    {
        Debug.Log("Go to Level Complete Called!");
        GoToGameEvent.Invoke();
    }
}
