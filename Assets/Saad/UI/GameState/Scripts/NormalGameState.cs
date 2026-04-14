using UnityEngine;
using Blues.Core.Events;
using System.Collections;
using Blues.Core.StateMachine;

/// <summary>
/// NormalGameState — owns WHAT happens and WHEN during normal gameplay.
///
/// Rules:
///   - Subscribes to HUD events in Enter(), unsubscribes by name in Exit()
///   - Navigation fires GameEvent SOs — never called from HUD directly
///   - No public methods exposed to the HUD or any other view
/// </summary>
[CreateAssetMenu(fileName = "GameState", menuName = "ProjectCore/State Machine/States/Normal Game State")]
public class NormalGameState : GameState
{
    [SerializeField] private GameEvent GoToLevelCompleteEvent;
    [SerializeField] private GameEvent GoToLevelFailEvent;
    [SerializeField] private GameEvent GoToSettingsEvent;

    private NormalGameHud _hud;

    public override IEnumerator Enter(IState previous)
    {
        // base.Enter (GameState) spawns gameplay + HUD and sets gameHudInstance
        yield return base.Enter(previous);

        _hud = gameHudInstance as NormalGameHud;
        if (_hud == null)
        {
            Debug.LogError("[NormalGameState] gameHudInstance is not a NormalGameHud.");
            yield break;
        }

        _hud.OnLevelCompletePressed += HandleLevelComplete;
        _hud.OnLevelFailPressed     += HandleLevelFail;
        _hud.OnSettingsPressed      += HandleSettings;
    }

    public override IEnumerator Exit()
    {
        if (_hud != null)
        {
            _hud.OnLevelCompletePressed -= HandleLevelComplete;
            _hud.OnLevelFailPressed     -= HandleLevelFail;
            _hud.OnSettingsPressed      -= HandleSettings;
            _hud = null;
        }

        yield return base.Exit();
    }

    private void HandleLevelComplete() => GoToLevelCompleteEvent.Invoke();
    private void HandleLevelFail()     => GoToLevelFailEvent.Invoke();
    private void HandleSettings()      => GoToSettingsEvent.Invoke();
}
