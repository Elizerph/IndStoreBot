using IndStoreBot.Access;

namespace IndStoreBot.Extensions
{
    public static class ReadWriteExtension
    {
        public static IReadWriteAccess<string> AsText(this IReadWriteAccess<Stream> instance)
        {
            return new TextAccess(instance);
        }

        public static IReadWriteAccess<T> AsObject<T>(this IReadWriteAccess<string> instance)
        {
            return new ObjectAccess<T>(instance);
        }

        public static IReadWriteAccess<T> WithCache<T>(this IReadWriteAccess<T> instance)
        {
            return new CachedAccess<T>(instance);
        }
    }
}
