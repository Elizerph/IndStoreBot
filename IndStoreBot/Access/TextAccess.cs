using IndStoreBot.Extensions;

namespace IndStoreBot.Access
{
    public class TextAccess : IReadWriteAccess<string>
    {
        private readonly IReadWriteAccess<Stream> _streamAccess;

        public TextAccess(IReadWriteAccess<Stream> streamAccess)
        {
            _streamAccess = streamAccess;
        }

        public async Task<string> Read()
        {
            using var stream = await _streamAccess.Read();
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public async Task Write(string text)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            await writer.WriteAllTextAsync(text);
            await writer.FlushAsync();
            await _streamAccess.Write(stream);
        }
    }
}
