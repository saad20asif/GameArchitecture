using System;
using Blues.Core.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainMenuView — owns HOW the main menu looks, nothing else.
///
/// Rules:
///   - Never holds a reference to MainMenuState or any State/Service
///   - Fires events upward; MainMenuState subscribes in Enter(), unsubscribes in Exit()
///   - Button listeners wired once in Awake() — GO is pooled, not destroyed
/// </summary>
public class MainMenuView : UIBase
{
    [SerializeField] private Button PlayBtn;

    public event Action OnPlayPressed;

    protected override void Awake()
    {
        base.Awake();
        PlayBtn.onClick.AddListener(OnPlayBtnClicked);
    }

    private void OnPlayBtnClicked() => OnPlayPressed?.Invoke();
}
