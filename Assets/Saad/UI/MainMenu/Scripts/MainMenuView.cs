using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private MainMenuState MainMenuState;
    [SerializeField] private Button PlayBtn;
    //[SerializeField] private LevelCompleteS

    private void OnEnable()
    {
        PlayBtn.onClick.AddListener(OnNextBtnClicked);
    }
    private void OnDisable()
    {
        PlayBtn.onClick.RemoveListener(OnNextBtnClicked);
    }
    private void OnNextBtnClicked()
    {
        MainMenuState.GoToPlayState();
    }
}
