namespace LinkMatch.Core.Pooling
{
    public interface IPool<T>
    {
        T Get();
        void Return(T item);
        int CountInactive { get; }
        void Prewarm(int count);
        void Clear();
    }
}