using System;
using UnityEngine;
using ProjectCore.Helpers;
using ProjectCore.Variables;
using Sirenix.OdinInspector;

namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "vDBString_", menuName = "ProjectCore/Variables/Persistent/DBString")]
    public class DBString : SharedString
    {
        [SerializeField] private string _key;
    
        [Button]
        private void ValidateKey()
        {
            KeyValidator.UnregisterAllFrom(this); // Avoid stale entries from previous validations
            KeyValidator.IsKeyUnique(_key,this);
        }
        private void OnEnable()
        {
            Load();
        }
        public override void SetValue(string value)
        {
            base.SetValue(value);
            Save();
        }
        [Button(ButtonSizes.Small)]
        private void Save()
        {
            PlayerPrefs.SetString(_key, Value);
            PlayerPrefs.Save(); // Ensure the data is saved
        }
        private void Load()
        {
            // Check if the key exists in PlayerPrefs
            if (PlayerPrefs.HasKey(_key))
            {
                // Load value from PlayerPrefs using the generated key
                Value = PlayerPrefs.GetString(_key);
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

