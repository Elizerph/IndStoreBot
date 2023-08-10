namespace IndStoreBot
{
    public static class StreamWriterExtension
    {
        public static async Task WriteAllTextAsync(this StreamWriter writer, string text)
        {
            foreach (var item in text)
                await writer.WriteAsync(item);
        }
    }
}
