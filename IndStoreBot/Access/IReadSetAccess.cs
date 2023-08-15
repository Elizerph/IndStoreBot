namespace IndStoreBot.Access
{
    public interface IReadSetAccess<T>
    {
        Task<T> Read(string id);
    }
}
