using System.IO;
using Newtonsoft.Json;
public class JsonFileWriter : IJsonWriter
{
    public void Write<T>(string filepath, T data)
    {
        string _jsonContent = JsonConvert.SerializeObject(data);
        File.WriteAllText(filepath, _jsonContent);
    }
}
