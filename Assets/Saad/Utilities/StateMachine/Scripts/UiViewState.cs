using ProjectCore.UI;
using UnityEngine;
using System.Collections;

namespace ProjectCore.StateMachine
{
    public abstract class UIViewState : State
    {
        [SerializeField] private StateViewPoolSO stateViewPool;

        private UiBase _uiInstance;

        public override IEnumerator Enter(IState previous)
        {
            yield return base.Enter(previous);

            var viewGO = stateViewPool.GetOrCreateView(this, FiniteStateMachine.ViewRoot);
            if (viewGO == null)
                yield break;

            _uiInstance = viewGO.GetComponent<UiBase>();
            viewGO.SetActive(true);
            _uiInstance.Show();
        }

        public override IEnumerator Exit()
        {
            if (_uiInstance != null)
            {
                _uiInstance.Hide(); // Assume Hide() just disables now, not destroys
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