using UnityEngine;

public static class StateRootManager
{
    public static Transform UIViewRoot { get; private set; }
    public static Transform GameStateRoot { get; private set; }

    public static void Initialize(Transform uiRoot, Transform gameRoot)
    {
        UIViewRoot = uiRoot;
        GameStateRoot = gameRoot;
    }
    public static bool IsInitialized => UIViewRoot != null && GameStateRoot != null;
}

