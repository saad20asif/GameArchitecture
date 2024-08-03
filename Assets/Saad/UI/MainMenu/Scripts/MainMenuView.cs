using ProjectCore.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : UiBase
{
    [SerializeField] private MainMenuState MainMenuState;
    [SerializeField] private Button PlayBtn;
    [SerializeField] private Button GoToSpinWheel;

    private void OnEnable()
    {
        PlayBtn.onClick.AddListener(OnNextBtnClicked);
        GoToSpinWheel.onClick.AddListener(OnSpinWheelClicked);
    }
    private void OnDisable()
    {
        PlayBtn.onClick.RemoveListener(OnNextBtnClicked);
        GoToSpinWheel.onClick.RemoveListener(OnSpinWheelClicked);
    }
    private void OnNextBtnClicked()
    {
        MainMenuState.GoToPlayState();
    }
    private void OnSpinWheelClicked()
    {
        MainMenuState.GoToSpinWheel();
    }
    public override IEnumerator Show()
    {
        base.Show();
        yield return null;
    }

    public override IEnumerator Hide()
    {
        base.Hide();
        yield return null;
    }
}
