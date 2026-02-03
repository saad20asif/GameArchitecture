using UnityEngine;
using System.Collections;
using Blues.Core.PoolSystem;
using Blues.Core.UI;
using Sirenix.OdinInspector;

namespace Blues.Core.StateMachine
{
    public class UIViewState : State
    {
        [SerializeField] protected string stateId;

        [SerializeField] protected bool usePooling = true;

        [ShowIf("@usePooling")]
        [SerializeField, Required]
        [InfoBox("Ensure prefab is registered in PoolManagerSO.", InfoMessageType.None)]
        private PoolManagerSO uIStatesPooler;

        private UIBase _uiInstance;
        private GameObject _spawnedInstance;

        public override IEnumerator Enter(IState previous)
        {
            yield return base.Enter(previous);

            GameObject viewObject = null;

            if (usePooling)
            {
                viewObject = uIStatesPooler.Get(stateId);
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
                viewObject.transform.SetParent(StateRootManager.States);
                _spawnedInstance = viewObject;
            }

            _uiInstance = viewObject.GetComponent<UIBase>();
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
                        uIStatesPooler.Release(stateId, _uiInstance.gameObject);
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
