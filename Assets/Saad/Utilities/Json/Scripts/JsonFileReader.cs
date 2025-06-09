using System.IO;
using Newtonsoft.Json;
namespace ProjectCore.Utilities.Json
{
    public class JsonFileReader : IJsonReader
    {
        public T ReadJson<T>(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("File Not Found!", filepath);
            }
            string _jsonContent = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<T>(_jsonContent);
        }
    }
}
