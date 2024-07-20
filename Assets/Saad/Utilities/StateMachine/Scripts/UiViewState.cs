using ProjectCore.StateMachine;
using ProjectCore.UI;
using System.Collections;
using UnityEngine;

public class UiViewState : State
{
    [SerializeField] protected string prefabName;
    protected GameObject prefabInstance;
    private IShowable _ishowable;
    //private UiBase _uiBase;

    public override IEnumerator Enter(IState _listener)
    {
        yield return base.Enter(_listener);

        // Load and instantiate the prefab
        GameObject prefab = Resources.Load<GameObject>(prefabName);

        if (prefab != null)
        {
            prefabInstance = Instantiate(prefab);
            _ishowable = prefabInstance.GetComponent<UiBase>();
            if (_ishowable != null)
            {
                _ishowable.Show();
            }
        }
        else
        {
            Debug.LogWarning($"Prefab with name {prefabName} could not be found in Resources!");
        }
    }
    public override IEnumerator Pause()
    {
        if (_ishowable != null)
        {
            _ishowable.Pause();
        }

        yield return base.Pause();
    }

    public override IEnumerator Resume()
    {
        if (_ishowable != null)
        {
            _ishowable.Resume();
        }

        yield return base.Resume();
    }
    public override IEnumerator Exit()
    {
        if (_ishowable != null)
        {
            _ishowable.Hide();
        }
        //else if (prefabInstance != null)
        //{
        //    Destroy(prefabInstance);
        //}

        yield return base.Exit(); // Call base Exit method
    }
}
