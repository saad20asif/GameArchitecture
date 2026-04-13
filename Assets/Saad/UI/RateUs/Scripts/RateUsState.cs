using UnityEngine;
using Blues.Core.Events;
using Blues.Core.StateMachine;
using System.Collections;

/// <summary>
/// RateUsState — owns WHAT happens and WHEN on the rate-us screen.
///
/// Rules:
///   - Subscribes to view events in Enter(), unsubscribes by name in Exit()
///   - Navigation fires GameEvent SOs — never calls FSM directly
///   - No public methods exposed to the View
/// </summary>
[CreateAssetMenu(fileName = "RateUsState", menuName = "ProjectCore/State Machine/States/RateUsState")]
public class RateUsState : UIViewState
{
    [SerializeField] private GameEvent backBtnPressedEvent;

    private RateUsView _view;

    public override IEnumerator Enter(IState previous)
    {
        yield return base.Enter(previous);

        _view = GetView<RateUsView>();
        if (_view == null) yield break;

        _view.OnBackPressed += HandleBackPressed;
    }

    public override IEnumerator Exit()
    {
        if (_view != null)
        {
            _view.OnBackPressed -= HandleBackPressed;
            _view = null;
        }

        yield return base.Exit();
    }

    private void HandleBackPressed() => backBtnPressedEvent.Invoke();
}
