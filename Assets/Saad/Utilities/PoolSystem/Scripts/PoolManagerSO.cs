using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Blues.Core.Variables;
using ProjectCore.PoolSystem;

namespace Blues.Core.PoolSystem
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

        private static PoolManagerRunner _runner;   // STATIC shared runner

        private bool initialized = false;

        private void OnEnable()
        {
            initialized = false;
        }

        private PoolManagerRunner Runner
        {
            get
            {
                if (_runner == null)
                {
                    GameObject go = new GameObject("PoolManagerRunner");
                    _runner = go.AddComponent<PoolManagerRunner>();
                    DontDestroyOnLoad(go);
                }
                return _runner;
            }
        }

        public void Initialize(Transform poolRoot = null)
        {
            if (initialized)
                return;

            _poolRoot = poolRoot ?? CreatePoolRoot();
            CreatePools();

            // ENSURE RUNNER EXISTS
            var r = Runner;
        }

        private Transform CreatePoolRoot()
        {
            var root = new GameObject("PoolRoot").transform;
            return root;
        }

        private void CreatePools()
        {
            _gameObjectPools.Clear();

            foreach (var config in _poolConfigs)
            {
                var parent = new GameObject($"{config.PoolID}_Pool").transform;
                parent.SetParent(_poolRoot);

                var pool = new UnityObjectPool<GameObject>(
                    createFunc: () => CreateGameObject(config.Prefab, parent),
                    onGet: OnGetGameObject,
                    onRelease: go => OnReleaseGameObject(go, parent),
                    defaultCapacity: config.DefaultCapacity,
                    maxSize: config.MaxSize
                );

                _gameObjectPools.Add(config.PoolID, pool);

                if (config.Prewarm)
                    pool.Prewarm();
            }
        }

        private GameObject CreateGameObject(GameObject prefab, Transform parent)
        {
            var instance = Instantiate(prefab, parent);
            instance.SetActive(false);
            return instance;
        }

        private void OnGetGameObject(GameObject gameObject)
        {
            gameObject.SetActive(true);
            if (gameObject.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnPoolGet();
        }

        private void OnReleaseGameObject(GameObject gameObject, Transform parent)
        {
            if (gameObject.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnPoolRelease();

            gameObject.transform.SetParent(parent);
            gameObject.SetActive(false);
        }

        // -------------------------------
        //   DELAYED RELEASE FIXED
        // -------------------------------
        public void ReleaseAfterDelay(string poolId, GameObject obj, float delay)
        {
            Runner.StartCoroutine(ReleaseDelayedRoutine(poolId, obj, delay));
        }

        private IEnumerator ReleaseDelayedRoutine(string poolId, GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            Release(poolId, obj);
        }

        // Standard methods
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
    }

    public interface IPoolable
    {
        void OnPoolGet();
        void OnPoolRelease();
    }
}
