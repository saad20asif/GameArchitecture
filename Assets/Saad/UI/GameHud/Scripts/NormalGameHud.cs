using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectCore.Events;
using ProjectCore.TimeUtility;
using ProjectCore.GameHud;

public class NormalGameHud : GameHud
{
    [SerializeField] private Button LevelCompleteBtn;
    [SerializeField] private Button LevelFailBtn;
    [SerializeField] private NormalGameState NormalGameState;
    [SerializeField] private GameEvent Tick;
    [SerializeField] private TextMeshProUGUI TimeText;
    int seconds = 0;

    public bool Paused { get; private set; }

    private void OnEnable()
    {
        UpdateTime();
        LevelCompleteBtn.onClick.AddListener(OnLevelCompleteBtn);
        LevelFailBtn.onClick.AddListener(OnLevelFailBtn);
        Tick.Subscribe(UpdateTime);
    }
    private void OnDisable()
    {
        LevelCompleteBtn.onClick.RemoveListener(OnLevelCompleteBtn);
        LevelFailBtn.onClick.RemoveListener(OnLevelFailBtn);
        Tick.UnSubscribe(UpdateTime);
    }
    private void OnLevelCompleteBtn()
    {
        NormalGameState.GoToLevelCompleteState();
    }
    private void OnLevelFailBtn()
    {
        NormalGameState.GoToLevelFailState();
    }
    private void UpdateTime()
    {
        if (!Paused)
        {
            seconds++;
            TimeText.text = TimeManager.FormatTime(seconds);
        }
    }
}
