using Blues.Core.Events;
using Blues.Core.StateMachine;
using System.Collections;
using UnityEngine;

/// <summary>
/// LevelCompleteState — owns WHAT happens and WHEN on the level-complete screen.
///
/// Rules:
///   - Subscribes to view events in Enter(), unsubscribes by name in Exit()
///   - Navigation fires GameEvent SOs — never calls FSM directly
///   - No public methods exposed to the View
/// </summary>
[CreateAssetMenu(fileName = "LevelCompleteState", menuName = "ProjectCore/State Machine/States/LevelComplete State")]
public class LevelCompleteState : UIViewState
{
    [SerializeField] private GameEventWithInt GoToMainMenuEvent;

    private LevelCompleteView _view;

    public override IEnumerator Enter(IState previous)
    {
        yield return base.Enter(previous);

        _view = GetView<LevelCompleteView>();
        if (_view == null) yield break;

        _view.OnNextPressed += HandleNextPressed;
    }

    public override IEnumerator Exit()
    {
        if (_view != null)
        {
            _view.OnNextPressed -= HandleNextPressed;
            _view = null;
        }

        yield return base.Exit();
    }

    private void HandleNextPressed() => GoToMainMenuEvent.Raise(1);
}
