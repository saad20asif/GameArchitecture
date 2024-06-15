using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MainMenuState", menuName = "ProjectCore/States/MainMenuState")]
public class MainMenuState : State
{
    [SerializeField] private string prefabName; 
    private GameObject prefabInstance; 
    public override IEnumerator Init(IState _listener)
    {
        Listener = _listener;

        // Load and instantiate the prefab
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        if (prefab != null)
        {
            prefabInstance = Instantiate(prefab);
        }
        else
        {
            Debug.LogWarning($"Prefab with name {prefabName} could not be found in Resources!");
        }

        yield return base.Init(_listener);
    }

    public override IEnumerator Execute()
    {
        Debug.Log("Main Menu Execute");
        yield return base.Execute(); 
    }

    public override IEnumerator Exit()
    {
        // Destroy the prefab instance if it exists
        if (prefabInstance != null)
        {
            Destroy(prefabInstance);
        }

        yield return base.Exit(); // Call base Exit method
    }
}
