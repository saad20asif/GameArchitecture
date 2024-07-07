using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MainMenuState", menuName = "ProjectCore/State Machine/States/MainMenuState")]
public class MainMenuState : UiViewState
{
    [SerializeField] private GameEvent GoToGameEvent;
    [SerializeField] private GameEvent GoToSpinWheelEvent;
    
    public void GoToPlayState()
    {
        Debug.Log("Go to Level Complete Called!");
        GoToGameEvent.Invoke();
    }
    public void GoToSpinWheel()
    {
        Debug.Log("Go to Spin Wheel Called!");
        GoToSpinWheelEvent.Invoke();
    }

    public override IEnumerator Pause()
    {
        yield return base.Pause();
        Debug.Log("MainMenu Pause Called!");
    }

    public override IEnumerator Resume()
    {
        yield return base.Resume();
        Debug.Log("MainMenu Resume Called!");
    }
}
