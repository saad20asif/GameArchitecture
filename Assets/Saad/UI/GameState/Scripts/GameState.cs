using ProjectCore.GameHud;
using ProjectCore.StateMachine;
using ProjectCore.UI;
using System;
using System.Collections;
using UnityEngine;

public abstract class GameState : State
{
    [SerializeField] private string gameHudPrefabName;  // Name of the prefab in Resources
    protected GameHud gameHudInstance;
    private IShowable _iShowable;
    
    public override IEnumerator Enter(IState listener)
    {
        yield return base.Enter(listener);

        // Load and instantiate the HUD prefab from Resources
        if (!string.IsNullOrEmpty(gameHudPrefabName))
        {
            GameObject _hudObject = Instantiate(Resources.Load<GameObject>(gameHudPrefabName));
            if (_hudObject != null)
            {
                gameHudInstance = _hudObject.GetComponent<GameHud>();
                _iShowable = _hudObject.GetComponent<GameHud>();
                if (gameHudInstance != null)
                {
                    gameHudInstance.Show();
                }
                else
                {
                    Debug.LogWarning("GameHud prefab does not contain a GameHud component.");
                }
            }
            else
            {
                Debug.LogWarning($"HUD prefab '{gameHudPrefabName}' could not be found in Resources.");
            }
        }
        else
        {
            Debug.LogWarning("GameHud prefab name is not assigned.");
        }
    }

    public override IEnumerator Exit()
    {
        // Hide and destroy the HUD instance
        if (gameHudInstance != null)
        {
            gameHudInstance.Hide();
        }

        yield return base.Exit();
    }

    public override IEnumerator Pause()
    {
        yield return base.Pause();
        _iShowable.Pause();
    }

    public override IEnumerator Resume()
    {
        yield return base.Resume();
        _iShowable.Resume();
    }


    // Additional methods for handling game state logic can go here
}
