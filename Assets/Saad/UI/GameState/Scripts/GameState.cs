using UnityEngine;
using ProjectCore.UI;
using ProjectCore.Events;
using System.Collections;
using ProjectCore.GameHud;
using ProjectCore.StateMachine;
using ProjectCore.PoolSystem;
using Sirenix.OdinInspector;

public abstract class GameState : State
{
    [SerializeField] private string gameplayPrefabId;
    [SerializeField] private string hudPrefabId;
    
    [SerializeField] private bool poolGameplay = true;
    [SerializeField] private bool poolGameHud = true;
    
    [ShowIf("@poolGameplay || poolGameHud")]
    [SerializeField, Required]
    [InfoBox("Ensure prefab is registered in PoolManagerSO.", InfoMessageType.None)]
    private PoolManagerSO statePooler;


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
            if (poolGameplay)
            {
                gameplayInstance = statePooler.Get(gameplayPrefabId);
            }
            else
            {
                var gameplayPrefab = Resources.Load<GameObject>(gameplayPrefabId);
                if (gameplayPrefab != null)
                {
                    gameplayInstance = Instantiate(gameplayPrefab,StateRootManager.States);
                }
                else
                {
                    Debug.LogWarning($"Gameplay prefab '{gameplayPrefabId}' not found in Resources.");
                }
            }

            //if (gameplayInstance != null)
                //gameplayInstance.transform.SetParent(StateRootManager.Gameplay, false);
        }

        // 2. Load HUD
        if (poolGameHud)
        {
            _spawnedHud = statePooler.Get(hudPrefabId);
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

            _spawnedHud = Instantiate(hudPrefab, StateRootManager.States);
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
                Debug.Log("GameHud instance hided.");
                if (poolGameHud)
                {
                    statePooler.Release(hudPrefabId, _spawnedHud);
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
            if (poolGameplay)
            {
                statePooler.Release(gameplayPrefabId, gameplayInstance);
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
