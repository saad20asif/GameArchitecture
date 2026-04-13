using System;
using Blues.Core.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LevelCompleteView — owns HOW the level-complete screen looks, nothing else.
///
/// Rules:
///   - Never holds a reference to LevelCompleteState or any State/Service
///   - Fires events upward; LevelCompleteState subscribes in Enter(), unsubscribes in Exit()
///   - Button listeners wired once in Awake() — GO is pooled, not destroyed
/// </summary>
public class LevelCompleteView : UIBase
{
    [SerializeField] private Button NextBtn;

    public event Action OnNextPressed;

    protected override void Awake()
    {
        base.Awake();
        NextBtn.onClick.AddListener(OnNextBtnClicked);
    }

    private void OnNextBtnClicked() => OnNextPressed?.Invoke();
}
