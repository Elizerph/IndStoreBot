namespace IndStoreBot
{
    public interface ISingletonStorage<T>
    {
        Task Save(T instance);
        Task<T> Load();
    }
}
