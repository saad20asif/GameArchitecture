using System;
using UnityEngine;
using ProjectCore.Helpers;
using ProjectCore.Variables;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "vDBString_", menuName = "ProjectCore/Variables/Persistent/DBString")]
public class DBString : SharedString
{
    private string _key;
    private void OnEnable()
    {
        Load();
    }
    public override void SetValue(string value)
    {
        base.SetValue(value);
    }
    [Button(ButtonSizes.Small)]
    private void Save()
    {
        PlayerPrefs.SetString(_key, Value);
        PlayerPrefs.Save(); // Ensure the data is saved
    }
    private void Load()
    {
        if (string.IsNullOrEmpty(_key))
        {
            _key = KeyRegistry.GenerateKey(name);
        }

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
