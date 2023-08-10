using Newtonsoft.Json;

namespace IndStoreBot
{
    public class FileSingletonStorage<T> : ISingletonStorage<T>
    {
        private readonly string _filePath;
        private T _currentItem;

        public FileSingletonStorage(string filePath)
        {
            _filePath = filePath;
        }

        public async Task Save(T newItem)
        {
            _currentItem = newItem;
            var text = JsonConvert.SerializeObject(newItem);
            await File.WriteAllTextAsync(_filePath, text);
        }

        public async Task<T> Load()
        {
            if (_currentItem == null)
            {
                var text = await File.ReadAllTextAsync(_filePath);
                _currentItem = JsonConvert.DeserializeObject<T>(text);
            }
            return _currentItem;
        }
    }
}
