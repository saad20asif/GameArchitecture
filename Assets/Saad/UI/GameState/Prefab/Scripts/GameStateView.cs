using UnityEngine;
using UnityEngine.UI;

public class GameStateView : MonoBehaviour
{
    [SerializeField] private Button LevelCompleteBtn;
    [SerializeField] private Button LevelFailBtn;
    [SerializeField] private GameState GameState;

    private void OnEnable()
    {
        LevelCompleteBtn.onClick.AddListener(OnLevelCompleteBtn);
        LevelFailBtn.onClick.AddListener(OnLevelFailBtn);
    }
    private void OnDisable()
    {
        LevelCompleteBtn.onClick.RemoveListener(OnLevelCompleteBtn);
        LevelFailBtn.onClick.RemoveListener(OnLevelFailBtn);
    }
    private void OnLevelCompleteBtn()
    {
        GameState.GoToLevelCompleteState();
    }
    private void OnLevelFailBtn()
    {
        GameState.GoToLevelFailState();
    }
}
