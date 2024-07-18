using ProjectCore.Helpers;
using ProjectCore.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "vDBInt_", menuName = "ProjectCore/Variables/Persistent/DBInt")]
public class DBInt : Int
{
    private string _key;
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
            _key = KeyRegistry.GenerateKey(name);
            Debug.Log("Generated key: " + _key);
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
