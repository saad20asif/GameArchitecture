using UnityEngine;

public static class StateRootManager
{
    public static Transform States { get; private set; }

    public static void Initialize(Transform state)
    {
        States = state;
        IsInitialized = true;
    }

    public static bool IsInitialized { get; private set; }

}

