using UnityEngine;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.OdinInspector;

public class SharedDictionary<K,V> : SerializedScriptableObject
{
    [OdinSerialize]
    protected Dictionary<K, V> keyValuePairs;

    private void OnEnable()
    {
        keyValuePairs = new Dictionary<K, V>();
    }
    public int Count => keyValuePairs.Count;
    public void Add(K key, V value)
    {
        if (!keyValuePairs.ContainsKey(key))
        {
            keyValuePairs.Add(key, value);
        }
        else
        {
            Debug.LogWarning($"Key {key} already exists in the dictionary.");
        }
    }
    public bool Remove(K key)
    {
        return keyValuePairs.Remove(key);
    }
    public bool ContainsKey(K key)
    {
        return keyValuePairs.ContainsKey(key);
    }
    public bool TryGetValue(K key, out V value)
    {
        return keyValuePairs.TryGetValue(key, out value);
    }
    public void Clear()
    {
        keyValuePairs.Clear();
    }
    public ICollection<K> GetAllKeys()
    {
        return keyValuePairs.Keys;
    }
    public ICollection<V> GetAllValues()
    {
        return keyValuePairs.Values;
    }
    public void UpdateValue(K key, V value)
    {
        if (keyValuePairs.ContainsKey(key))
        {
            keyValuePairs[key] = value;
        }
        else
        {
            Debug.LogWarning($"Key {key} does not exist in the dictionary.");
        }
    }


}
