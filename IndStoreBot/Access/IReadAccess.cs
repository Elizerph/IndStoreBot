namespace IndStoreBot.Access
{
    public interface IReadAccess<T>
    {
        Task<T> Read();
    }
}
