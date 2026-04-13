using System.Collections;
using Blues.Core.Events;
using Blues.Core.StateMachine;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// SpinWheelState — owns WHAT happens and WHEN on the spin-wheel screen.
///
/// Rules:
///   - Subscribes to view events in Enter(), unsubscribes by name in Exit()
///   - Navigation fires GameEvent SOs — never calls FSM directly
///   - No public methods exposed to the View
///
/// NOTE: DOVirtual.DelayedCall is used to give the wheel data event a frame
///   to propagate after loading. TODO: replace with a proper coroutine or
///   event-driven trigger to avoid an orphaned tween if Exit fires within 0.2s.
/// </summary>
[CreateAssetMenu(fileName = "SpinWheelState", menuName = "ProjectCore/State Machine/States/SpinWheelState")]
public class SpinWheelState : UIViewState
{
    [SerializeField] private GameEvent backBtnPressedEvent;
    [SerializeField] private Featrues.SpinWheel.SpinWheelConfigurations spinWheelConfigurations;
    [SerializeField] private GameEvent OnSpinWheelDataLoaded;

    private SpinWheelView _view;
    private Tweener _dataLoadTween;

    public override IEnumerator Enter(IState previous)
    {
        // base.Enter spawns + shows the view — must come first
        yield return base.Enter(previous);

        _view = GetView<SpinWheelView>();
        if (_view == null) yield break;

        _view.OnBackPressed += HandleBackPressed;

        LoadWheelData();
    }

    public override IEnumerator Exit()
    {
        // Cancel the delayed data-loaded event if we exit before it fires
        _dataLoadTween?.Kill();
        _dataLoadTween = null;

        if (_view != null)
        {
            _view.OnBackPressed -= HandleBackPressed;
            _view = null;
        }

        yield return base.Exit();
    }

    private void LoadWheelData()
    {
        if (spinWheelConfigurations == null) return;
        if (!spinWheelConfigurations.LoadDataFromJson()) return;

        /*_dataLoadTween = DOVirtual.DelayedCall(0.2f, () =>
        {
            _dataLoadTween = null;
            OnSpinWheelDataLoaded.Invoke();
        });*/
    }

    private void HandleBackPressed() => backBtnPressedEvent.Invoke();
}
