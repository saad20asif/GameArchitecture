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

        [ShowInInspector]private UIBase _uiInstance;
        private GameObject _spawnedInstance;

        /// <summary>
        /// Returns the active view instance cast to T.
        /// Call in Enter() after base.Enter(), and in Exit() before base.Exit().
        /// Returns null if the view has not been spawned yet or has already been released.
        /// </summary>
        protected T GetView<T>() where T : UIBase => _uiInstance as T;

        public override IEnumerator Enter(IState previous)
        {
            yield return base.Enter(previous);

            GameObject viewObject = null;

            if (usePooling)
            {
                viewObject = uIStatesPooler.Get(stateId);
                if (viewObject == null)
                {
                    // PoolManagerSO.Get() already logged the error — just bail
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
            _uiInstance.SetSortingOrder(Listener.CurrentSortingOrder);
            _uiInstance.Show();
        }

        public override IEnumerator Exit()
        {
            if (_uiInstance != null)
            {
                var instance = _uiInstance;
                _uiInstance = null;          // null first — prevents double-release if Exit is called again mid-coroutine

                yield return instance.Hide();

                if (usePooling)
                {
                    uIStatesPooler.Release(stateId, instance.gameObject);
                }
                else if (_spawnedInstance != null)
                {
                    Destroy(_spawnedInstance);
                    _spawnedInstance = null;
                }
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
