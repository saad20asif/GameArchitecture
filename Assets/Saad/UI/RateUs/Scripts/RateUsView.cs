using UnityEngine;
using ProjectCore.UI;
using UnityEngine.UI;

public class RateUsView : UiBase
{
    [SerializeField] private Button Back;
    [SerializeField] private RateUsState RateUsState;

    private void OnEnable()
    {
        Back.onClick.AddListener(OnBackBtnClicked);
    }
    private void OnDisable()
    {
        Back.onClick.RemoveListener(OnBackBtnClicked);
    }
    private void OnBackBtnClicked()
    {
        print("OnBackBtnClicked");
        RateUsState.GoToSpinWheel();
    }
}
