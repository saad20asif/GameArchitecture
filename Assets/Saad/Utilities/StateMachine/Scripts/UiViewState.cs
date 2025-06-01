using ProjectCore.UI;
using UnityEngine;
using System.Collections;
using ProjectCore.PoolSystem;

namespace ProjectCore.StateMachine
{
    public abstract class UIViewState : State
    {
        [SerializeField] private string stateId;
        [SerializeField] private PoolManagerSO poolManagerSO;

        private UiBase _uiInstance;

        public override IEnumerator Enter(IState previous)
        {
            yield return base.Enter(previous);

            var view = poolManagerSO.GetComponent<UiBase>(stateId);

            if (view == null)
                yield break;

            _uiInstance = view.GetComponent<UiBase>();
            view.gameObject.SetActive(true);
            _uiInstance.Show();
        }

        public override IEnumerator Exit()
        {
            if (_uiInstance != null)
            {
                _uiInstance.Hide(() =>
                {
                    Debug.Log($"{stateId} released");
                    poolManagerSO.Release(stateId, _uiInstance);
                }); // Assume Hide() just disables now, not destroys
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