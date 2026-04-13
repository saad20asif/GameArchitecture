using System;
using Blues.Core.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SpinWheelView — owns HOW the spin-wheel screen looks, nothing else.
///
/// Rules:
///   - Never holds a reference to SpinWheelState or any State/Service
///   - Fires events upward; SpinWheelState subscribes in Enter(), unsubscribes in Exit()
///   - Button listeners wired once in Awake() — GO is pooled, not destroyed
/// </summary>
public class SpinWheelView : UIBase
{
    [SerializeField] private Button Back;

    public event Action OnBackPressed;

    protected override void Awake()
    {
        base.Awake();
        Back.onClick.AddListener(OnBackBtnClicked);
    }

    private void OnBackBtnClicked() => OnBackPressed?.Invoke();
}
