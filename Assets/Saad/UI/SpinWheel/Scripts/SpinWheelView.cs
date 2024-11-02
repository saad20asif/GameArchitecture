using ProjectCore.UI;
using UnityEngine;
using UnityEngine.UI;

public class SpinWheelView : UiBase
{
    [SerializeField] private Button Back;
    [SerializeField] private Button RateUsBtn;
    [SerializeField] private SpinWheelState SpinWheelState;

    private void OnEnable()
    {
        Back.onClick.AddListener(OnBackBtnClicked);
        RateUsBtn.onClick.AddListener(OnRateUsBtnClicked);   
    }
    private void OnDisable()
    {
        Back.onClick.RemoveListener(OnBackBtnClicked);
        RateUsBtn.onClick.RemoveListener(OnRateUsBtnClicked);
    }
    private void OnBackBtnClicked()
    {
        //print("OnBackBtnClicked");
        SpinWheelState.GoToMainMenu();
    }
    private void OnRateUsBtnClicked()
    {
        //print("OnBackBtnClicked");
        SpinWheelState.GoToRateUs();
    }

}
