using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteView : MonoBehaviour
{
    [SerializeField] private Button NextBtn;
    [SerializeField] private LevelCompleteState LevelCompleteState;

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
        LevelCompleteState.GoToMainMenu();
    }
}
