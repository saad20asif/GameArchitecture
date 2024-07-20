using System.IO;
using Newtonsoft.Json;
public class JsonFileReader : IJsonReader
{
    public T Read<T>(string filepath)
    {
        if(!File.Exists(filepath))
        {
            throw new FileNotFoundException("File Not Found! ",filepath);
        }
        string _jsonContent = File.ReadAllText(filepath);
        return JsonConvert.DeserializeObject<T>(_jsonContent);
    }
}
