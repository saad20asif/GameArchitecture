using Blues.Core.UI;
using UnityEngine;
using UnityEngine.UI;

public class LevelFailView : UIBase
{
    [SerializeField] private Button NextBtn;
    [SerializeField] private LevelFailState LevelFailState;

    private void OnEnable()
    {
        NextBtn.onClick.AddListener(OnNextBtnClicked);
    }
    private void OnDisable()
    {
        NextBtn.onClick.RemoveListener(OnNextBtnClicked);
    }
    private void OnNextBtnClicked()
    {
        LevelFailState.GotoMainMenu();
    }
}
