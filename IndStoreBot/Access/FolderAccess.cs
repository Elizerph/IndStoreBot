namespace IndStoreBot.Access
{
    public class FolderAccess : IReadWriteSetAccess<Stream>
    {
        public static FolderAccess Current { get; } = new FolderAccess(string.Empty);

        private readonly string _folder;

        public FolderAccess(string folder)
        {
            _folder = folder;
        }

        public IReadWriteAccess<Stream> GetFileAccess(string file)
        {
            return new FileStreamAccess(Path.Combine(_folder, file));
        }

        public FolderAccess GetSubFolder(string folder)
        {
            return new FolderAccess(Path.Combine(_folder, folder));
        }

        public Task<Stream> Read(string id)
        {
            return GetFileAccess(id).Read();
        }

        public Task Write(string id, Stream instance)
        {
            return GetFileAccess(id).Write(instance);
        }
    }
}
