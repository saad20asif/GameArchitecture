using ProjectCore.StateMachine;
using System.Collections;
using UnityEngine;

public class UiViewState : State
{
    [SerializeField] protected string prefabName;
    protected GameObject prefabInstance;
    private UiBase _uiBase;

    public override IEnumerator Enter(IState _listener)
    {
        yield return base.Enter(_listener);

        // Load and instantiate the prefab
        GameObject prefab = Resources.Load<GameObject>(prefabName);

        if (prefab != null)
        {
            prefabInstance = Instantiate(prefab);
            _uiBase = prefabInstance.GetComponent<UiBase>();
            if (_uiBase != null)
            {
                _uiBase.Show();
            }
        }
        else
        {
            Debug.LogWarning($"Prefab with name {prefabName} could not be found in Resources!");
        }
    }
    public override IEnumerator Pause()
    {
        if (_uiBase != null)
        {
            _uiBase.Pause();
        }

        yield return base.Pause();
    }

    public override IEnumerator Resume()
    {
        if (_uiBase != null)
        {
            _uiBase.Resume();
        }

        yield return base.Resume();
    }
    public override IEnumerator Exit()
    {
        if (_uiBase != null)
        {
            _uiBase.Hide();
        }
        //else if (prefabInstance != null)
        //{
        //    Destroy(prefabInstance);
        //}

        yield return base.Exit(); // Call base Exit method
    }
}
