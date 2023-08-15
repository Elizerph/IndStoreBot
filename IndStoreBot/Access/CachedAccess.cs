namespace IndStoreBot.Access
{
    public class CachedAccess<T> : IReadWriteAccess<T>
    {
        private readonly IReadWriteAccess<T> _access;
        private T _cached;

        public CachedAccess(IReadWriteAccess<T> access)
        {
            _access = access;
        }

        public async Task<T> Read()
        {
            _cached ??= await _access.Read();
            return _cached;
        }

        public async Task Write(T instance)
        {
            _cached = instance;
            await _access.Write(instance);
        }
    }
}
