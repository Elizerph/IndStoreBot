using Newtonsoft.Json;

namespace IndStoreBot
{
    public class SettingsBundleProvider
    {
        private readonly string _settingsFilePath;
        private SettingsBundle _current;

        public SettingsBundleProvider(string settingsFilePath) 
        {
            _settingsFilePath = settingsFilePath ?? throw new ArgumentNullException(nameof(settingsFilePath));
        }

        public async Task SaveAndApply(SettingsBundle bundle)
        {
            _current = bundle;
            var text = JsonConvert.SerializeObject(bundle);
            await File.WriteAllTextAsync(_settingsFilePath, text);
        }

        private async Task<SettingsBundle> ReadSettings()
        {
            var text = await File.ReadAllTextAsync(_settingsFilePath);
            return JsonConvert.DeserializeObject<SettingsBundle>(text);
        }

        public async Task<long> GetChatId()
        {
            if (_current == null)
                _current = await ReadSettings();
            return _current.TargetChatId;
        }

        public async Task<IReadOnlyCollection<TicketFieldTemplate>> GetTemplates()
        {
            if (_current == null)
                _current = await ReadSettings();
            return _current.TicketFields;
        }
    }
}
