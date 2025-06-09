using System;
using UnityEngine;
using Newtonsoft.Json;

namespace ProjectCore.Utilities.Json
{
    public class JsonPrettyPrinter : MonoBehaviour
    {
        public string PrettyPrintJson(string jsonContent)
        {
            try
            {
                var parsedJson = JsonConvert.DeserializeObject(jsonContent);
                return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to pretty print JSON: {ex.Message}");
                return jsonContent;
            }
        }
    }
}
