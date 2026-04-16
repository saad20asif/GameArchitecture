using System;
using Blues.Core.UI;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsXUIView : UIBase
{
    [SerializeField] private Button SettingsBtn;
    [SerializeField] private Button SoundBtn;
    [SerializeField] private Button HapticsBtn;
    [SerializeField] private Button RestoreBtn;
    [SerializeField] private Button ExitBtn;

    [SerializeField] private Image SoundIcon;
    [SerializeField] private Image HapticsIcon;

    [SerializeField] private Color EnabledColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color DisabledColor = new Color(0.5f, 0.5f, 0.5f);

    public event Action OnSettingsPressed;
    public event Action OnSoundToggled;
    public event Action OnHapticsToggled;
    public event Action OnRestorePressed;
    public event Action OnExitPressed;

    public void SetData(GameSettingsXViewData data)
    {
        SoundIcon.color = data.SoundEnabled ? EnabledColor : DisabledColor;
        HapticsIcon.color = data.HapticsEnabled ? EnabledColor : DisabledColor;
    }

    protected override void Awake()
    {
        base.Awake();
        SettingsBtn.onClick.AddListener(OnSettingsBtnClicked);
        SoundBtn.onClick.AddListener(OnSoundBtnClicked);
        HapticsBtn.onClick.AddListener(OnHapticsBtnClicked);
        RestoreBtn.onClick.AddListener(OnRestoreBtnClicked);
        ExitBtn.onClick.AddListener(OnExitBtnClicked);
    }

    private void OnSettingsBtnClicked() => OnSettingsPressed?.Invoke();
    private void OnSoundBtnClicked()    => OnSoundToggled?.Invoke();
    private void OnHapticsBtnClicked()  => OnHapticsToggled?.Invoke();
    private void OnRestoreBtnClicked()  => OnRestorePressed?.Invoke();
    private void OnExitBtnClicked()     => OnExitPressed?.Invoke();
}
