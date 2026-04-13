using Blues.Core.Events;
using System.Collections;
using Blues.Core.StateMachine;
using Sirenix.OdinInspector;
using THEBADDEST.Coroutines;
using UnityEngine;

/// <summary>
/// MainMenuState — owns WHAT happens and WHEN on the main menu.
///
/// Rules:
///   - Subscribes to view events in Enter(), unsubscribes by name in Exit()
///   - Navigation fires GameEvent SOs — never calls FSM directly
///   - No public methods exposed to the View
/// </summary>
[CreateAssetMenu(fileName = "MainMenuState", menuName = "ProjectCore/State Machine/States/MainMenuState")]
public class MainMenuState : UIViewState
{
    [SerializeField] private GameEvent GoToGameEvent;

    private MainMenuView _view;

    public override IEnumerator Enter(IState previous)
    {
        yield return base.Enter(previous);

        _view = GetView<MainMenuView>();
        if (_view == null) yield break;

        _view.OnPlayPressed += HandlePlayPressed;
    }

    public override IEnumerator Exit()
    {
        if (_view != null)
        {
            _view.OnPlayPressed -= HandlePlayPressed;
            _view = null;
        }

        yield return base.Exit();
    }

    public override IEnumerator Pause()
    {
        yield return base.Pause();
    }

    public override IEnumerator Resume()
    {
        yield return base.Resume();
    }

    private void HandlePlayPressed() => GoToGameEvent.Invoke();

    [Button]
    public void ReloadState()
    {
        CoroutineHandler.StartStaticCoroutine(Listener.ReloadCurrentState());
    }
}
