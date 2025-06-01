using UnityEngine;
using System.Collections.Generic;

namespace ProjectCore.PoolSystem
{
    [CreateAssetMenu(menuName = "Pools/Pool Manager")]
    public class PoolManagerSO : ScriptableObject
    {
        [System.Serializable]
        public class PoolConfig
        {
            public string PoolID;
            public GameObject Prefab;
            public int DefaultCapacity = 10;
            public int MaxSize = 100;
            public bool Prewarm = false;
        }

        [SerializeField] private PoolConfig[] _poolConfigs;
        private Dictionary<string, UnityObjectPool<GameObject>> _gameObjectPools = new();
        private Dictionary<string, object> _componentPools = new();
        private Transform _poolRoot;

        public void Initialize(Transform poolRoot = null)
        {
            _poolRoot = poolRoot ?? CreatePoolRoot();
            CreatePools();
        }

        private Transform CreatePoolRoot()
        {
            var root = new GameObject("PoolRoot").transform;
            // Fixed ambiguous reference
            //DontDestroyOnLoad(root.gameObject);
            return root;
        }

        private void CreatePools()
        {
            foreach (var config in _poolConfigs)
            {
                var parent = new GameObject($"{config.PoolID}_Pool").transform;
                parent.SetParent(_poolRoot);
                
                var pool = new UnityObjectPool<GameObject>(
                    createFunc: () => CreateGameObject(config.Prefab, parent),
                    onGet: OnGetGameObject,
                    onRelease: OnReleaseGameObject,
                    defaultCapacity: config.DefaultCapacity,
                    maxSize: config.MaxSize
                );
                
                _gameObjectPools.Add(config.PoolID, pool);
                
                if (config.Prewarm) pool.Prewarm();
            }
        }

        private GameObject CreateGameObject(GameObject prefab, Transform parent)
        {
            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.SetActive(false);
            return instance;
        }

        private void OnGetGameObject(GameObject gameObject)
        {
            // Reset state if needed
            if (gameObject.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnPoolGet();
        }

        private void OnReleaseGameObject(GameObject gameObject)
        {
            // Clean up if needed
            if (gameObject.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnPoolRelease();
        }

        public GameObject Get(string poolId)
        {
            if (_gameObjectPools.TryGetValue(poolId, out var pool))
                return pool.Get();
            
            throw new KeyNotFoundException($"Pool {poolId} not found");
        }

        public void Release(string poolId, GameObject gameObject)
        {
            if (_gameObjectPools.TryGetValue(poolId, out var pool))
                pool.Release(gameObject);
            else
                Destroy(gameObject);
        }

        public T GetComponent<T>(string poolId) where T : Component
        {
            // Get or create component pool
            if (!_componentPools.TryGetValue(poolId, out var poolObj))
            {
                var gameObjectPool = _gameObjectPools[poolId];
                var componentPool = new UnityObjectPool<T>(
                    createFunc: () => gameObjectPool.Get().GetComponent<T>(),
                    onGet: OnGetComponent,
                    onRelease: OnReleaseComponent,
                    defaultCapacity: gameObjectPool.CountAll // Match GameObject pool size
                );
                
                _componentPools.Add(poolId, componentPool);
                return componentPool.Get();
            }
            
            return ((UnityObjectPool<T>)poolObj).Get();
        }

        public void Release<T>(string poolId, T component) where T : Component
        {
            if (_componentPools.TryGetValue(poolId, out var poolObj))
                ((UnityObjectPool<T>)poolObj).Release(component);
            else
            {
                Debug.Log($"Object {poolId} Destroyed");
                Destroy(component.gameObject);
            }
                
        }

        private void OnGetComponent<T>(T component) where T : Component
        {
            component.gameObject.SetActive(true);
            if (component is IPoolable poolable)
                poolable.OnPoolGet();
        }

        private void OnReleaseComponent<T>(T component) where T : Component
        {
            component.gameObject.SetActive(false);
            if (component is IPoolable poolable)
                poolable.OnPoolRelease();
        }
    }

    public interface IPoolable
    {
        void OnPoolGet();
        void OnPoolRelease();
    }
}
