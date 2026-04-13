using System;
using Blues.Core.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LevelFailView — owns HOW the level-fail screen looks, nothing else.
///
/// Rules:
///   - Never holds a reference to LevelFailState or any State/Service
///   - Fires events upward; LevelFailState subscribes in Enter(), unsubscribes in Exit()
///   - Button listeners wired once in Awake() — GO is pooled, not destroyed
/// </summary>
public class LevelFailView : UIBase
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
