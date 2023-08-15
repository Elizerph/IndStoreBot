namespace IndStoreBot.Access
{
    public interface IReadWriteAccess<T> : IReadAccess<T>, IWriteAccess<T>
    {
    }
}
