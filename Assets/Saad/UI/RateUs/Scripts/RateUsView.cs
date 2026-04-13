using System;
using Blues.Core.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RateUsView — owns HOW the rate-us screen looks, nothing else.
///
/// Rules:
///   - Never holds a reference to RateUsState or any State/Service
///   - Fires events upward; RateUsState subscribes in Enter(), unsubscribes in Exit()
///   - Button listeners wired once in Awake() — GO is pooled, not destroyed
/// </summary>
public class RateUsView : UIBase
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
