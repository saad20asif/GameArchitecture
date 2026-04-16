using Blues.Core.Events;
using Blues.Core.StateMachine;
using Blues.Core.Variables;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettingsXState", menuName = "ProjectCore/State Machine/States/GameSettingsXState")]
public class GameSettingsXState : UIViewState
{
    [SerializeField] private GameEvent GoToHomeEvent;

    [Header("Variables")]
    [SerializeField] private DBBool SoundEnabled;
    [SerializeField] private DBBool HapticsEnabled;

    private GameSettingsXUIView _view;

    public override IEnumerator Enter(IState previous)
    {
        yield return base.Enter(previous);

        _view = GetView<GameSettingsXUIView>();
        if (_view == null) yield break;

        SubscribeEvents();
        RefreshView();
    }

    public override IEnumerator Exit()
    {
        if (_view != null)
        {
            UnsubscribeEvents();
            _view = null;
        }

        yield return base.Exit();
    }

    private void SubscribeEvents()
    {
        _view.OnSettingsPressed += HandleSettingsPressed;
        _view.OnSoundToggled   += HandleSoundToggled;
        _view.OnHapticsToggled += HandleHapticsToggled;
        _view.OnRestorePressed += HandleRestorePressed;
        _view.OnExitPressed    += HandleExitPressed;
    }

    private void UnsubscribeEvents()
    {
        _view.OnSettingsPressed -= HandleSettingsPressed;
        _view.OnSoundToggled   -= HandleSoundToggled;
        _view.OnHapticsToggled -= HandleHapticsToggled;
        _view.OnRestorePressed -= HandleRestorePressed;
        _view.OnExitPressed    -= HandleExitPressed;
    }

    private void HandleSettingsPressed()
    {
        GoToHomeEvent.Invoke();
    }

    private void HandleSoundToggled()
    {
        SoundEnabled.SetValue(!SoundEnabled.GetValue());
        RefreshView();
    }

    private void HandleHapticsToggled()
    {
        HapticsEnabled.SetValue(!HapticsEnabled.GetValue());
        RefreshView();
    }

    private void HandleRestorePressed()
    {
        Debug.Log("[GameSettingsX] Restore purchases requested.");
    }

    private void HandleExitPressed()
    {
        GoToHomeEvent.Invoke();
    }

    private void RefreshView()
    {
        _view.SetData(new GameSettingsXViewData
        {
            SoundEnabled = SoundEnabled.GetValue(),
            HapticsEnabled = HapticsEnabled.GetValue()
        });
    }
}
