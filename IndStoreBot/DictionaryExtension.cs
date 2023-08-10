namespace IndStoreBot
{
    public static class DictionaryExtension
    {
        public static T GetOrDefault<T>(this Dictionary<T, T> instance, T key)
        {
            if (instance.TryGetValue(key, out var value))
                return value;
            else
                return key;
        }
    }
}
