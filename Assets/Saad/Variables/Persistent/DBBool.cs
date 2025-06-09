using ProjectCore.Helpers;
using ProjectCore.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "vDBBool_", menuName = "ProjectCore/Variables/Persistent/DBBool")]
public class DBBool : Bool
{
    private string _key;

    private void OnEnable()
    {
        Load();
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
            _key = KeyRegistry.GenerateKey(name);
            Debug.Log("Generated key: " + _key);
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
