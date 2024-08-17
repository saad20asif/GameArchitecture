namespace ProjectCore.Utilities.Json
{
    interface IJsonReader
    {
        public T ReadJson<T>(string filepath);
    }
}
