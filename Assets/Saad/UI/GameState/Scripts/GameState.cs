using UnityEngine;
using ProjectCore.UI;
using ProjectCore.Events;
using System.Collections;
using CustomEditorScripts;
using ProjectCore.GameHud;
using ProjectCore.StateMachine;

public abstract class GameState : State
{
    [Header("References")]
    [SerializeField] private GameHud gameHudPrefab;  // Drag reference in Inspector
    [SerializeField] private string gameplayPrefabName; // Load from Resources

    protected GameHud gameHudInstance;
    private IShowable _iShowable;
    private GameObject gameplayInstance;

    [ColorFoldoutGroup("StateFlowEvents")]
    [SerializeField] private GameEvent GameStateEnter;
    [ColorFoldoutGroup("StateFlowEvents")]
    [SerializeField] private GameEvent GameStatePaused;
    [ColorFoldoutGroup("StateFlowEvents")]
    [SerializeField] private GameEvent GameStateResumed;
    [ColorFoldoutGroup("StateFlowEvents")]
    [SerializeField] private GameEvent GameStateExit;

    public override IEnumerator Enter(IState listener)
    {
        yield return base.Enter(listener);

        // 1. Instantiate gameplay world if any
        if (!string.IsNullOrEmpty(gameplayPrefabName))
        {
            var gameplayPrefab = Resources.Load<GameObject>(gameplayPrefabName);
            if (gameplayPrefab != null)
            {
                gameplayInstance = Instantiate(gameplayPrefab,StateRootManager.GameStateRoot);
            }
            else
            {
                Debug.LogWarning($"Gameplay prefab '{gameplayPrefabName}' not found in Resources.");
            }
        }

        // 2. Instantiate HUD
        if (gameHudPrefab != null && gameHudInstance == null)
        {
            gameHudInstance = Instantiate(gameHudPrefab, StateRootManager.GameStateRoot);
            _iShowable = gameHudInstance;
            gameHudInstance.Show();
        }
        else
        {
            Debug.LogWarning("GameHud prefab is missing or already instantiated.");
        }

        GameStateEnter.Invoke();
    }

    public override IEnumerator Exit()
    {
        // 1. Hide HUD
        if (gameHudInstance != null)
        {
            gameHudInstance.Hide(); // Don’t destroy, reuse via pooling
        }

        // 2. Destroy gameplay world
        if (gameplayInstance != null)
        {
            Destroy(gameplayInstance);
            gameplayInstance = null;
        }

        yield return base.Exit();
        GameStateExit.Invoke();
    }

    public override IEnumerator Pause()
    {
        yield return base.Pause();
        _iShowable?.Pause();
        GameStatePaused.Invoke();
    }

    public override IEnumerator Resume()
    {
        yield return base.Resume();
        _iShowable?.Resume();
        GameStateResumed.Invoke();
    }
}
