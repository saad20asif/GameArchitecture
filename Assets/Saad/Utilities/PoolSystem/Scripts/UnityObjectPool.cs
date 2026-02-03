using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace ProjectCore.PoolSystem
{
    public class UnityObjectPool<T> : IPool<T>, IDisposable where T : class
    {
        private ObjectPool<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _defaultCapacity;
        private readonly int _maxSize;
        private readonly bool _isUnityObject;

        public int CountActive => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;
        public int CountAll => _pool.CountAll;

        public UnityObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int defaultCapacity = 10,
            int maxSize = 10000
        ) {
            _createFunc = createFunc;
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _defaultCapacity = defaultCapacity;
            _maxSize = maxSize;
            _isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));

            _pool = new ObjectPool<T>(
                createFunc: CreateHandler,
                actionOnGet: GetHandler,
                actionOnRelease: ReleaseHandler,
                actionOnDestroy: DestroyHandler,
                collectionCheck: true,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxSize
            );
        }

        private T CreateHandler() => _createFunc();

        private void GetHandler(T element)
        {
            // Handle GameObject/Component activation
            if (_isUnityObject)
            {
                switch (element)
                {
                    case GameObject gameObject:
                        gameObject.SetActive(true);
                        break;
                    case Component component:
                        component.gameObject.SetActive(true);
                        break;
                }
            }
            
            _onGet?.Invoke(element);
        }

        private void ReleaseHandler(T element)
        {
            // Handle GameObject/Component deactivation
            if (_isUnityObject)
            {
                switch (element)
                {
                    case GameObject gameObject:
                        gameObject.SetActive(false);
                        break;
                    case Component component:
                        component.gameObject.SetActive(false);
                        break;
                }
            }
            
            _onRelease?.Invoke(element);
        }

        private void DestroyHandler(T element)
        {
            _onDestroy?.Invoke(element);
            
            if (_isUnityObject)
            {
                switch (element)
                {
                    case GameObject gameObject:
                        Object.Destroy(gameObject);
                        break;
                    case Component component:
                        Object.Destroy(component.gameObject);
                        break;
                }
            }
        }
        public bool TryGet(out T element)
        {
            if (CountAll >= _maxSize && CountInactive == 0)
            {
                element = null;
                return false;
            }

            element = _pool.Get();
            return true;
        }

        public T Get() => _pool.Get();
        public void Release(T element) => _pool.Release(element);
        public void Clear() => _pool.Clear();

        public void Prewarm()
        {
            T[] instances = new T[_defaultCapacity];
            for (int i = 0; i < _defaultCapacity; i++) instances[i] = Get();
            for (int i = 0; i < _defaultCapacity; i++) Release(instances[i]);
        }

        public void Dispose() => Clear();
    }
}
