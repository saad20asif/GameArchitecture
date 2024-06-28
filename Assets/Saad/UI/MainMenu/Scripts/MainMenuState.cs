using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MainMenuState", menuName = "ProjectCore/State Machine/States/MainMenuState")]
public class MainMenuState : State
{
    [SerializeField] private GameEvent GoToGameEvent;
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
    public void GoToPlayState()
    {
        Debug.Log("Go to Level Complete Called!");
        GoToGameEvent.Invoke();
    }
}
