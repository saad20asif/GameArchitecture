using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectCore.Events
{
    [CreateAssetMenu(fileName = "e_", menuName = "ProjectCore/Events/Game Event - Basic")]
    public class GameEvent : ScriptableObject
    {
        public delegate void EventHandler();
        private event EventHandler _handler;

        [GUIColor(1, 1, 0.5f)]
        [Button(ButtonSizes.Medium)]
        public void Invoke()
        {
            if (_handler != null)
            {
                _handler.Invoke();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"GameEvent '{name}' invoked but no handlers are attached.");
#endif
            }
        }

        public void Subscribe(EventHandler handler)
        {
            _handler += handler;
        }

        public void UnSubscribe(EventHandler handler)
        {
            _handler -= handler;
        }
        public bool HasListeners()
        {
            return _handler != null;
        }

        private void OnDisable()
        {
            _handler = null; // Clear all handlers when the ScriptableObject is disabled or destroyed to avoid memory leaks
        }
    }
}
