using UnityEngine;
using System.Collections;
using ProjectCore.UI;
using ProjectCore.PoolSystem;

namespace ProjectCore.StateMachine
{
    public abstract class UIViewState : State
    {
        [Header("Unique ID for Pool or Resource Lookup")]
        [SerializeField] private string stateId;

        [Header("Pooling Configuration")]
        [Tooltip("Enable to use PoolManager. Disable to load prefab from Resources.")]
        [SerializeField] private bool usePooling = true;
        [SerializeField, Tooltip("Only assign if pooling is enabled")]
        private PoolManagerSO poolManagerSO;

        private UiBase _uiInstance;
        private GameObject _spawnedInstance;

        public override IEnumerator Enter(IState previous)
        {
            yield return base.Enter(previous);

            if (usePooling)
            {
                _uiInstance = poolManagerSO.GetComponent<UiBase>(stateId);
            }
            else
            {
                var prefab = Resources.Load<GameObject>(stateId);
                if (prefab == null)
                {
                    Debug.LogError($"[UIViewState] Prefab not found in Resources at path: {stateId}");
                    yield break;
                }

                _spawnedInstance = Object.Instantiate(prefab);
                _uiInstance = _spawnedInstance.GetComponent<UiBase>();

                if (_uiInstance == null)
                {
                    Debug.LogError($"[UIViewState] Instantiated object at '{stateId}' is missing UiBase component.");
                    yield break;
                }
            }

            _uiInstance.Show();
        }

        public override IEnumerator Exit()
        {
            if (_uiInstance != null)
            {
                _uiInstance.Hide(() =>
                {
                    if (usePooling)
                    {
                        poolManagerSO.Release(stateId, _uiInstance);
                    }
                    else if (_spawnedInstance != null)
                    {
                        Object.Destroy(_spawnedInstance);
                    }
                });
            }

            yield return base.Exit();
        }

        public override IEnumerator Pause()
        {
            _uiInstance?.Pause();
            yield return base.Pause();
        }

        public override IEnumerator Resume()
        {
            _uiInstance?.Resume();
            yield return base.Resume();
        }
    }
}
