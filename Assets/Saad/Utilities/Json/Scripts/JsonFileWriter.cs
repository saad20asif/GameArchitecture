using Newtonsoft.Json;
using System.IO;
namespace Blues.Core.Utilities.Json
{
    public class JsonFileWriter : IJsonWriter
    {
        public void WriteJson<T>(string filepath, T data)
        {
            string jsonContent = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filepath, jsonContent);
        }
    }
}
