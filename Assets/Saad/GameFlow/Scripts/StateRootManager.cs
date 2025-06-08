using UnityEngine;

public static class StateRootManager
{
    public static Transform UI { get; private set; }
    public static Transform UIPooled { get; private set; } 
    public static Transform UINonPooled { get; private set; }
    public static Transform Gameplay { get; private set; }
    public static Transform GameplayPooled { get; private set; }
    public static Transform GameplayNonPooled { get; private set; }
    

    public static void Initialize(Transform ui,Transform uiPooled, Transform uiNonPooled, Transform gameplay,Transform gameplayPooled, Transform gameplayNonPooled)
    {
        UI = ui;
        UIPooled = uiPooled;
        UINonPooled = uiNonPooled;
        Gameplay = gameplay;
        GameplayPooled = gameplayPooled;
        gameplayNonPooled = gameplayPooled;
        IsInitialized = true;
    }

    public static bool IsInitialized { get; private set; }

}

