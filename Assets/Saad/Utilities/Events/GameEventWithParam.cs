using System;
using UnityEngine;

namespace ProjectCore.Events
{
    public delegate void GameEventWithHandler<T>(T t);
    public delegate void GameEventWithHandler<T1, T2>(T1 t1, T2 t2);
    public delegate void GameEventWithHandler<T1, T2, T3>(T1 t1, T2 t2, T3 t3);

    public class GameEventWithParam<T> : ScriptableObject
    {
        private event GameEventWithHandler<T> Handler;

        public void Subscribe(GameEventWithHandler<T> handler)
        {
            Handler += handler;
        }

        public void Unsubscribe(GameEventWithHandler<T> handler)
        {
            Handler -= handler;
        }

        protected virtual void Raise(T t)
        {
            if (Handler != null)
            {
                Handler.Invoke(t);
            }
            else
            {
                Debug.LogWarning($"GameEventWithParam<{typeof(T).Name}> invoked but no handlers are attached.");
            }
        }
    }

    public class GameEventWithParam<T1, T2> : ScriptableObject
    {
        private event GameEventWithHandler<T1, T2> Handler;

        public void Subscribe(GameEventWithHandler<T1, T2> handler)
        {
            Handler += handler;
        }

        public void Unsubscribe(GameEventWithHandler<T1, T2> handler)
        {
            Handler -= handler;
        }

        protected virtual void Raise(T1 t1, T2 t2)
        {
            if (Handler != null)
            {
                Handler.Invoke(t1, t2);
            }
            else
            {
                Debug.LogWarning($"GameEventWithParam<{typeof(T1).Name}, {typeof(T2).Name}> invoked but no handlers are attached.");
            }
        }
    }

    public class GameEventWithParam<T1, T2, T3> : ScriptableObject
    {
        private event GameEventWithHandler<T1, T2, T3> Handler;

        public void Subscribe(GameEventWithHandler<T1, T2, T3> handler)
        {
            Handler += handler;
        }

        public void Unsubscribe(GameEventWithHandler<T1, T2, T3> handler)
        {
            Handler -= handler;
        }

        protected virtual void Raise(T1 t1, T2 t2, T3 t3)
        {
            if (Handler != null)
            {
                Handler.Invoke(t1, t2, t3);
            }
            else
            {
                Debug.LogWarning($"GameEventWithParam<{typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}> invoked but no handlers are attached.");
            }
        }
    }
}
