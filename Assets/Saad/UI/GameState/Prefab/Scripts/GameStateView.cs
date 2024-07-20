using ProjectCore.Events;
using ProjectCore.TimeUtility;
using ProjectCore.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStateView : UiBase
{
    [SerializeField] private Button LevelCompleteBtn;
    [SerializeField] private Button LevelFailBtn;
    [SerializeField] private GameState GameState;
    [SerializeField] private GameEvent Tick;
    [SerializeField] private TextMeshProUGUI TimeText;
    int seconds = 0;

    private void OnEnable()
    {
        UpdateTime();
        LevelCompleteBtn.onClick.AddListener(OnLevelCompleteBtn);
        LevelFailBtn.onClick.AddListener(OnLevelFailBtn);
        Tick.Handler += UpdateTime;
    }
    private void OnDisable()
    {
        LevelCompleteBtn.onClick.RemoveListener(OnLevelCompleteBtn);
        LevelFailBtn.onClick.RemoveListener(OnLevelFailBtn);
        Tick.Handler -= UpdateTime;
    }
    private void OnLevelCompleteBtn()
    {
        GameState.GoToLevelCompleteState();
    }
    private void OnLevelFailBtn()
    {
        GameState.GoToLevelFailState();
    }
    private void UpdateTime()
    {
        if(!Paused)
        {
            seconds++;
            TimeText.text = TimeManager.FormatTime(seconds);
        }
    }
}
