public interface IPool<T> where T : class
{
    T Get();
    void Release(T element);
    void Clear();
    int CountActive { get; }
    int CountInactive { get; }
    int CountAll { get; }
}