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

        [SerializeField] private bool usePooling = true;

        [SerializeField, Tooltip("Only assign if pooling is enabled")]
        private PoolManagerSO poolManagerSO;

        private UiBase _uiInstance;
        private GameObject _spawnedInstance;

        public override IEnumerator Enter(IState previous)
        {
            yield return base.Enter(previous);

            GameObject viewObject = null;

            if (usePooling)
            {
                viewObject = poolManagerSO.Get(stateId);
                if (viewObject == null)
                {
                    Debug.LogError($"[UIViewState] No pooled GameObject found for stateId: {stateId}");
                    yield break;
                }
            }
            else
            {
                var prefab = Resources.Load<GameObject>(stateId);
                if (prefab == null)
                {
                    Debug.LogError($"[UIViewState] Prefab not found in Resources at path: {stateId}");
                    yield break;
                }

                viewObject = Instantiate(prefab);
                viewObject.transform.SetParent(StateRootManager.UINonPooled);
                _spawnedInstance = viewObject;
            }

            _uiInstance = viewObject.GetComponent<UiBase>();
            if (_uiInstance == null)
            {
                Debug.LogError($"[UIViewState] GameObject at '{stateId}' does not contain UiBase component.");
                yield break;
            }

            viewObject.SetActive(true);
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
                        poolManagerSO.Release(stateId, _uiInstance.gameObject);
                    }
                    else if (_spawnedInstance != null)
                    {
                        Destroy(_spawnedInstance);
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
