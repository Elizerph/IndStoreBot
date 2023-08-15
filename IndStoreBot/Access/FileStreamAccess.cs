namespace IndStoreBot.Access
{
    public class FileStreamAccess : IReadWriteAccess<Stream>
    {
        private readonly string _filePath;

        public FileStreamAccess(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<Stream> Read()
        {
            return File.OpenRead(_filePath);
        }

        public async Task Write(Stream stream)
        {
            stream.Position = 0;
            using var fileStream = File.Open(_filePath, FileMode.OpenOrCreate);
            await stream.CopyToAsync(fileStream);
        }
    }
}
