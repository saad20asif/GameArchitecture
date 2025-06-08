using UnityEngine;
using ProjectCore.UI;
using ProjectCore.Events;
using System.Collections;
using ProjectCore.GameHud;
using ProjectCore.StateMachine;
using ProjectCore.PoolSystem;

public abstract class GameState : State
{
    [Header("Gameplay Config")]
    [Tooltip("Prefab name under Resources folder for gameplay object")]
    [SerializeField] private string gameplayPrefabId;

    [Tooltip("Use pooling for gameplay instance")]
    [SerializeField] private bool usePoolingForGameplay = true;

    [Header("HUD Config")]
    [Tooltip("Unique ID used for pooling or Resources loading")]
    [SerializeField] private string hudPrefabId;

    [Tooltip("Use pooling for GameHud")]
    [SerializeField] private bool usePoolingForGameHud = true;

    [Tooltip("Only assign if pooling is enabled")]
    [SerializeField] private PoolManagerSO gameStatePooler;

    [Header("State Events")]
    [SerializeField] private GameEvent GameStateEnter;
    [SerializeField] private GameEvent GameStatePaused;
    [SerializeField] private GameEvent GameStateResumed;
    [SerializeField] private GameEvent GameStateExit;

    protected GameHud gameHudInstance;
    private IShowable _iShowable;
    private GameObject gameplayInstance;
    private GameObject _spawnedHud;

    public override IEnumerator Enter(IState previous)
    {
        yield return base.Enter(previous);

        // 1. Load Gameplay
        if (!string.IsNullOrEmpty(gameplayPrefabId))
        {
            if (usePoolingForGameplay)
            {
                gameplayInstance = gameStatePooler.Get(gameplayPrefabId);
            }
            else
            {
                var gameplayPrefab = Resources.Load<GameObject>(gameplayPrefabId);
                if (gameplayPrefab != null)
                {
                    gameplayInstance = Instantiate(gameplayPrefab);
                }
                else
                {
                    Debug.LogWarning($"Gameplay prefab '{gameplayPrefabId}' not found in Resources.");
                }
                gameplayInstance.transform.SetParent(StateRootManager.GameplayNonPooled);
            }

            //if (gameplayInstance != null)
                //gameplayInstance.transform.SetParent(StateRootManager.Gameplay, false);
        }

        // 2. Load HUD
        if (usePoolingForGameHud)
        {
            _spawnedHud = gameStatePooler.Get(hudPrefabId);
            gameHudInstance = _spawnedHud.GetComponent<GameHud>();
        }
        else
        {
            var hudPrefab = Resources.Load<GameObject>(hudPrefabId);
            if (hudPrefab == null)
            {
                Debug.LogError($"[GameState] HUD prefab '{hudPrefabId}' not found in Resources.");
                yield break;
            }

            _spawnedHud = Instantiate(hudPrefab, StateRootManager.GameplayNonPooled);
            gameHudInstance = _spawnedHud.GetComponent<GameHud>();
        }

        if (gameHudInstance != null)
        {
            _iShowable = gameHudInstance;
            gameHudInstance.Show();
        }
        else
        {
            Debug.LogError($"[GameState] HUD instance is missing or missing GameHud component.");
        }

        GameStateEnter?.Invoke();
    }

    public override IEnumerator Exit()
    {
        // 1. Hide HUD
        if (gameHudInstance != null)
        {
            gameHudInstance.Hide(() =>
            {
                if (usePoolingForGameHud)
                {
                    gameStatePooler.Release(hudPrefabId, _spawnedHud);
                }
                else if (_spawnedHud != null)
                {
                    Destroy(_spawnedHud);
                }
            });
        }

        // 2. Remove gameplay
        if (gameplayInstance != null)
        {
            if (usePoolingForGameplay)
            {
                gameStatePooler.Release(gameplayPrefabId, gameplayInstance);
            }
            else
            {
                Destroy(gameplayInstance);
            }

            gameplayInstance = null;
        }

        yield return base.Exit();
        GameStateExit?.Invoke();
    }

    public override IEnumerator Pause()
    {
        yield return base.Pause();
        _iShowable?.Pause();
        GameStatePaused?.Invoke();
    }

    public override IEnumerator Resume()
    {
        yield return base.Resume();
        _iShowable?.Resume();
        GameStateResumed?.Invoke();
    }
}
