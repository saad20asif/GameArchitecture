namespace ProjectCore.PoolSystem
{
    public interface IPool<T>
    {
        T Get();
        void Release(T item);
        void Prewarm();
        void Clear();
        int CountActive { get; }
        int CountInactive { get; }
        int CountAll { get; }
    }
}