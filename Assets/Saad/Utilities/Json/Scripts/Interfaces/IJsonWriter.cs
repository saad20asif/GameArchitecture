namespace ProjectCore.Utilities.Json
{
    interface IJsonWriter
    {
        public void WriteJson<T>(string filepath, T data);
    }
}
