using Blues.Core.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Blues.Core.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Persistent/DBBool")]
    public class DBBool : Bool
    {
        [SerializeField] private string _key;

        private void OnEnable()
        {
            Load();
        }
        [Button]
        private void ValidateKey()
        {
            KeyValidator.UnregisterAllFrom(this); // Avoid stale entries from previous validations
            KeyValidator.IsKeyUnique(_key,this);
        }
        public override void SetValue(bool value)
        {
            base.SetValue(value);
            Save();
        }

        [Button(ButtonSizes.Small)]
        private void Save()
        {
            PlayerPrefs.SetInt(_key, Value ? 1 : 0);
            PlayerPrefs.Save(); // Ensure the data is saved
        }

        public virtual bool GetBool(string key)
        {
            return PlayerPrefs.GetInt(key) == 1;
        }

        private void Load()
        {
            if (string.IsNullOrEmpty(_key))
            {
                Debug.LogError($"{name} has an empty key. Skipping load.");
                return;
            }
            // Check if the key exists in PlayerPrefs
            if (PlayerPrefs.HasKey(_key))
            {
                // Load value from PlayerPrefs using the generated key
                Value = GetBool(_key);
            }
            else
            {
                // If the key doesn't exist, use the default value and save it to PlayerPrefs
                Value = DefaultValue;
                Save();
            }
        }
    }
}

