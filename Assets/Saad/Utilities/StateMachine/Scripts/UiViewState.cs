using ProjectCore.StateMachine;
using System.Collections;
using UnityEngine;

public class UiViewState : State
{
    [SerializeField] protected string prefabName;
    protected GameObject prefabInstance;
    private CanvasGroup _canvasGroup;
    public override IEnumerator Enter(IState _listener)
    {
        yield return base.Enter(_listener);

        // Load and instantiate the prefab
        GameObject prefab = Resources.Load<GameObject>(prefabName);

        if (prefab != null)
        {
            prefabInstance = Instantiate(prefab);
            if(prefabInstance.GetComponent<CanvasGroup>()!=null)
                _canvasGroup = prefabInstance.GetComponent<CanvasGroup>();
        }
        else
        {
            Debug.LogWarning($"Prefab with name {prefabName} could not be found in Resources!");
        }
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

    public override IEnumerator Pause()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false; // Optional: also disable raycasting
        }
        yield return base.Pause();
    }

    public override IEnumerator Resume()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true; // Optional: also disable raycasting
        }
        yield return base.Resume();
    }
}
