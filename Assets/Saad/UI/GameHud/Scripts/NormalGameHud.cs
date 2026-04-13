using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Blues.Core.Events;
using Blues.Core.TimeUtility;
using Blues.Core.GameHud;

/// <summary>
/// NormalGameHud — owns HOW the gameplay HUD looks, nothing else.
///
/// Rules:
///   - Never holds a reference to NormalGameState or any State/Service
///   - Fires events upward; NormalGameState subscribes in Enter(), unsubscribes in Exit()
///   - Button listeners wired once in Awake() — GO is pooled, not destroyed
///   - Tick GameEvent subscription lives in OnEnable/OnDisable (active only while visible)
/// </summary>
public class NormalGameHud : GameHud
{
    [SerializeField] private Button            LevelCompleteBtn;
    [SerializeField] private Button            LevelFailBtn;
    [SerializeField] private GameEvent         Tick;
    [SerializeField] private TextMeshProUGUI   TimeText;

    public event Action OnLevelCompletePressed;
    public event Action OnLevelFailPressed;

    public bool Paused { get; private set; }

    private int _seconds;

    protected override void Awake()
    {
        base.Awake();
        LevelCompleteBtn.onClick.AddListener(OnLevelCompleteBtnClicked);
        LevelFailBtn.onClick.AddListener(OnLevelFailBtnClicked);
    }

    private void OnEnable()
    {
        _seconds = 0;
        Tick.Subscribe(UpdateTime);
    }

    private void OnDisable()
    {
        Tick.UnSubscribe(UpdateTime);
    }

    private void OnLevelCompleteBtnClicked() => OnLevelCompletePressed?.Invoke();
    private void OnLevelFailBtnClicked()     => OnLevelFailPressed?.Invoke();

    private void UpdateTime()
    {
        if (!Paused)
        {
            _seconds++;
            TimeText.text = TimeManager.FormatTime(_seconds);
        }
    }

    public override void Resume()
    {
        base.Resume();
        Paused = false;
    }

    public override void Pause()
    {
        base.Pause();
        Paused = true;
    }
}
