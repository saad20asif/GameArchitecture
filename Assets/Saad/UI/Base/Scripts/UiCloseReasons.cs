/// <summary>
/// Reasons for closing or transitioning UI/Game states, driving how paused states are handled.
/// </summary>
/// 
namespace Blues.Core.UI
{
    public enum UICloseReasons
    {
        Home,  // 0
        Game,  // 1
        SkipLevel, // 2
        Revive, // 3
        ResumeGame, // 4
        FullScreenPlacement, // 5
        ResumeAny, // 6
        DailyLogin, // 7
        /// <summary>Special case: show full screen placement popup.</summary>
        ShowFullScreenPlacement // 8
    }
    public enum ClosePolicy
    {
        Default,
        ClearAll,
        PopUntil,
        PopOne
    }
}

