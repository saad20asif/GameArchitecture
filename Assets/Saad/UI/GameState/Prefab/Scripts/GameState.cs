using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "ProjectCore/State Machine/States/Game State")]
public class GameState : State
{
    [SerializeField] private GameEvent GoToLevelCompleteEvent;
    [SerializeField] private GameEvent GoToLevelFailEvent;
    [SerializeField] private string prefabName;
    private GameObject prefabInstance;
    public override IEnumerator Enter(IState _listener)
    {
        yield return base.Enter(_listener);

        // Load and instantiate the prefab
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        Debug.Log("MainMnu Prefab Loaded!");
        if (prefab != null)
        {
            prefabInstance = Instantiate(prefab);
        }
        else
        {
            Debug.LogWarning($"Prefab with name {prefabName} could not be found in Resources!");
        }
        
    }
    public override IEnumerator Exit()
    {
        Debug.Log("Main Menu Exit Called ! ");
        // Destroy the prefab instance if it exists
        if (prefabInstance != null)
        {
            Destroy(prefabInstance);
        }

        yield return base.Exit(); // Call base Exit method
    }
    public void GoToLevelCompleteState()
    {
        Debug.Log("Go to Level Complete Called!");
        GoToLevelCompleteEvent.Invoke();
    }
    public void GoToLevelFailState()
    {
        Debug.Log("Go to Level Complete Called!");
        GoToLevelFailEvent.Invoke();
    }
}
