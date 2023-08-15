namespace IndStoreBot.Access
{
    public interface IWriteAccess<T>
    {
        Task Write(T instance);
    }
}
