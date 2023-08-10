namespace IndStoreBot
{
    public static class SingletonStorageExtension
    {
        public static async Task<TProp> Load<T, TProp>(this ISingletonStorage<T> storage, Func<T, TProp> projection)
        {
            var item = await storage.Load();
            return projection(item);
        }
    }
}
