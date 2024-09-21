using ProjectCore.StateMachine;
using ProjectCore.UI;
using System.Collections;
using UnityEngine;

public class UiViewState : State
{
    [SerializeField] protected string prefabName;
    protected GameObject prefabInstance;
    private IShowable _iShowable;

    public override IEnumerator Enter(IState _listener)
    {
        yield return base.Enter(_listener);

        // Load and instantiate the prefab
        prefabInstance = Instantiate(Resources.Load<GameObject>(prefabName));

        if (prefabInstance != null)
        {
            _iShowable = prefabInstance.GetComponent<UiBase>();
            if (_iShowable != null)
            {
                _iShowable.Show();
            }
            else
            {
                Debug.LogWarning($"Prefab {prefabName} does not have a UiBase component.");
            }
        }
        else
        {
            Debug.LogWarning($"Prefab with name {prefabName} could not be found in Resources!");
        }
    }

    public override IEnumerator Pause()
    {
        if (_iShowable != null)
        {
            _iShowable.Pause();
        }

        yield return base.Pause();
    }

    public override IEnumerator Resume()
    {
        if (_iShowable != null)
        {
            _iShowable.Resume();
        }

        yield return base.Resume();
    }

    public override IEnumerator Exit()
    {
        if (_iShowable != null)
        {
            _iShowable.Hide();
        }

        yield return base.Exit(); // Call base Exit method
    }

    private void OnDestroy()
    {
        // Ensure that the prefab instance is properly cleaned up
        if (prefabInstance != null)
        {
            Destroy(prefabInstance);
        }
    }
}
