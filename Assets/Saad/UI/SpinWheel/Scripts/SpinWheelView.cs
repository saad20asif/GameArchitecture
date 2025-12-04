using Blues.Core.UI;
using UnityEngine;
using UnityEngine.UI;

public class SpinWheelView : UiBase
{
    [SerializeField] private Button Back;
    [SerializeField] private SpinWheelState SpinWheelState;

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
        SpinWheelState.GoBack();
    }
}
