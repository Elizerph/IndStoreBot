using Newtonsoft.Json;

namespace IndStoreBot.Access
{
    public class ObjectAccess<T> : IReadWriteAccess<T>
    {
        private readonly IReadWriteAccess<string> _textAccess;

        public ObjectAccess(IReadWriteAccess<string> textAccess)
        {
            _textAccess = textAccess;
        }

        public async Task<T> Read()
        {
            var text = await _textAccess.Read();
            return JsonConvert.DeserializeObject<T>(text);
        }

        public async Task Write(T instance)
        {
            var text = JsonConvert.SerializeObject(instance);
            await _textAccess.Write(text);
        }
    }
}
