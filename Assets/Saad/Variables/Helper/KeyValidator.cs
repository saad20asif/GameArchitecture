using UnityEngine;
using System.Collections.Generic;

namespace ProjectCore.Helpers
{
    public static class KeyValidator
    {
        private static Dictionary<string, Object> registeredKeys = new Dictionary<string, Object>();

        public static bool IsKeyUnique(string key, Object context)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Key cannot be null or empty!", context);
                return false;
            }

            if (PlayerPrefs.HasKey(key))
            {
                Debug.LogError($"Key '{key}' is already saved in PlayerPrefs!", context);
                return false;
            }

            if (registeredKeys.TryGetValue(key, out var existingObject))
            {
                // Allow same object to re-register its key without error
                if (existingObject != context)
                {
                    Debug.LogError($"Key '{key}' has already been registered by '{existingObject.name}'!", context);
                    return false;
                }
            }
            else
            {
                registeredKeys[key] = context;
                Debug.Log($"Key '{key}' is unique and registered by '{context.name}'.", context);
            }

            return true;
        }

        public static void UnregisterKey(string key)
        {
            if (registeredKeys.Remove(key))
            {
                Debug.Log($"Key '{key}' has been unregistered.");
            }
        }

        public static void UnregisterAllFrom(Object context)
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in registeredKeys)
            {
                if (kvp.Value == context)
                    keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
            {
                registeredKeys.Remove(key);
                Debug.Log($"Key '{key}' unregistered from '{context.name}'.");
            }
        }
    }
}
