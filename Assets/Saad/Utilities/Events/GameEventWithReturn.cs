using System;
using UnityEngine;

namespace ProjectCore.Events
{
    public class GameEventWithReturn<T> : ScriptableObject
    {
        private event Func<T> Handler;

        public void Subscribe(Func<T> handler)
        {
            Handler += handler;
        }

        public void UnSubscribe(Func<T> handler)
        {
            Handler -= handler;
        }

        public virtual T Raise()
        {
            if (Handler != null)
            {
                return Handler.Invoke();
            }
            else
            {
                Debug.LogWarning($"GameEventWithReturn<{typeof(T).Name}> invoked but no handlers are attached.");
                return default;
            }
        }
    }
}
