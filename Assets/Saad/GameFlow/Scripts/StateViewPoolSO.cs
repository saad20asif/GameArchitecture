using UnityEngine;
using System.Collections.Generic;

namespace ProjectCore.StateMachine
{
    [CreateAssetMenu(fileName = "StateViewPool", menuName = "ProjectCore/State Machine/State View Pool")]
    public class StateViewPoolSO : ScriptableObject
    {
        [System.Serializable]
        public class StateViewEntry
        {
            public UIViewState State;
            public GameObject ViewPrefab;
        }

        [SerializeField] private List<StateViewEntry> entries;

        private readonly Dictionary<UIViewState, GameObject> _viewInstances = new();

        public GameObject GetOrCreateView(UIViewState state, Transform parent)
        {
            if (_viewInstances.TryGetValue(state, out var existing) && existing != null)
            {
                return existing;
            }

            var entry = entries.Find(e => e.State == state);
            if (entry == null || entry.ViewPrefab == null)
            {
                Debug.LogWarning($"No view prefab assigned for {state.name} in the pool.");
                return null;
            }

            var instance = Object.Instantiate(entry.ViewPrefab, parent);
            instance.SetActive(false); // Optional: show later via state
            _viewInstances[state] = instance;
            return instance;
        }

        public void HideAllViews()
        {
            foreach (var kvp in _viewInstances)
            {
                if (kvp.Value != null)
                    kvp.Value.SetActive(false);
            }
        }
    }
}