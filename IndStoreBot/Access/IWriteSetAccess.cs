namespace IndStoreBot.Access
{
    public interface IWriteSetAccess<T>
    {
        Task Write(string id, T instance);
    }
}
