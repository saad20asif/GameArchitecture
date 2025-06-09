using Newtonsoft.Json;
using UnityEngine;

namespace ProjectCore.Utilities.Json
{
    public class JsonValidator : MonoBehaviour
    {
        public bool IsValidJson(string jsonContent)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject(jsonContent);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
