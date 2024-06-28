using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCompleteState", menuName = "ProjectCore/State Machine/States/LevelComplete State")]
public class LevelCompleteState : State
{
    [SerializeField] private string prefabName;
    private GameObject prefabInstance;
    [SerializeField] private GameEvent GoToMainMenuEvent;
    public override IEnumerator Enter(IState _listener)
    {
        yield return base.Enter(_listener);
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
        yield return base.Exit();
        if (prefabInstance != null)
        {
            Destroy(prefabInstance);
        }
    }
    public void GoToMainMenu()
    {
        GoToMainMenuEvent.Invoke();
    }
}
