namespace IndStoreBot.Access
{
    public interface IReadWriteSetAccess<T> : IReadSetAccess<T>, IWriteSetAccess<T>
    {
    }
}
