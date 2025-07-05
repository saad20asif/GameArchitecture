using ProjectCore.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Persistent/DBInt")]
    public class DBInt : Int
    {
        [SerializeField]private string _key;

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
        public override void Decrement(int _decrement)
        {
            base.Decrement(_decrement);
            Save();
        }
        public override void Increment(int _increment)
        {
            base.Increment(_increment);
            Save();
        }
        public override void SetValue(int value)
        {
            base.SetValue(value);
            Save();
        }
        [Button(ButtonSizes.Small)]
        private void Save()
        {
            PlayerPrefs.SetInt(_key, Value);
            PlayerPrefs.Save(); // Ensure the data is saved
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
                Value = PlayerPrefs.GetInt(_key);
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

