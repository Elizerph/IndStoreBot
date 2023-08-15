using IndStoreBot.Access;
using IndStoreBot.Extensions;

using Newtonsoft.Json;

using System.Text.Json.Nodes;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace IndStoreBot.Handlers
{
    public class AdminHandler : BaseHandler
    {
        private readonly long _adminId;
        private readonly IReadWriteAccess<SettingsBundle> _settingsAccess;
        private readonly IReadWriteAccess<Dictionary<string, string>> _localizationAccess;
        private readonly IReadWriteSetAccess<Stream> _customFilesAccess;

        public AdminHandler(long adminId, IReadWriteAccess<SettingsBundle> settingsAccess, IReadWriteAccess<Dictionary<string, string>> localizationAccess, IReadWriteSetAccess<Stream> customFilesAccess)
        {
            _adminId = adminId;
            _settingsAccess = settingsAccess;
            _localizationAccess = localizationAccess;
            _customFilesAccess = customFilesAccess;
        }

        protected override Task HandleButton(ITelegramBotClient botClient, long chatId, long userId, int messageId, string? messageText, string? caption, string? buttonData)
        {
            return Task.CompletedTask;
        }

        protected override async Task HandleMessage(ITelegramBotClient botClient, long chatId, long userId, string? text, bool isCommand, Contact? contact, Document? document)
        {
            if (userId != _adminId)
                return;

            if (document != null)
            {
                var fileName = document.FileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    Log.WriteError("File name is null or empty");
                    return;
                }
                using var memoryStream = new MemoryStream();
                await botClient.GetInfoAndDownloadFileAsync(document.FileId, memoryStream);
                memoryStream.Position = 0;
                if (string.Equals(fileName, "settings.json"))
                {
                    using var reader = new StreamReader(memoryStream);
                    var fileText = await reader.ReadToEndAsync();
                    var settings = JsonConvert.DeserializeObject<SettingsBundle>(fileText);
                    await _settingsAccess.Write(settings);
                    await botClient.SendTextMessageAsync(chatId, $"New settings applied");
                }
                else if (string.Equals(fileName, "localization.json"))
                {
                    using var reader = new StreamReader(memoryStream);
                    var fileText = await reader.ReadToEndAsync();
                    var localization = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileText);
                    await _localizationAccess.Write(localization);
                    await botClient.SendTextMessageAsync(chatId, $"New localization applied");
                }
                else
                {
                    await _customFilesAccess.Write(fileName, memoryStream);
                    await botClient.SendTextMessageAsync(chatId, $"Custom file {fileName} uploaded");
                }
            }
            else if (!string.IsNullOrEmpty(text) && isCommand)
            {
                if (string.Equals(text, "/help"))
                {
                    var helpTextLines = new[]
                    {
                        "Admin commands:",
                        "\t/settings: provides settings file",
                        "\t/localization: provides localization file",
                        "Admin file uploads:",
                        "\tsettings.json: setup settings",
                        "\tlocalization: setup localization file",
                        "\t\tlocalization emoji featue: use <emo{id}>, where {id} is emoji unicode code",
                        "\tCustom files: upload via document. Reference through file name in order to attach"
                    };
                    await botClient.SendTextMessageAsync(chatId, string.Join(Environment.NewLine, helpTextLines));
                }
                else if (string.Equals(text, "/settings"))
                {
                    var settings = await _settingsAccess.Read();
                    var fileText = JsonConvert.SerializeObject(settings);
                    using var memoryStream = new MemoryStream();
                    using var writer = new StreamWriter(memoryStream);
                    await writer.WriteAllTextAsync(fileText);
                    memoryStream.Position = 0;
                    await botClient.SendDocumentAsync(chatId, InputFile.FromStream(memoryStream, $"settings.json"));
                }
                else if(string.Equals(text, "/localization"))
                {
                    var localization = await _localizationAccess.Read();
                    var fileText = JsonConvert.SerializeObject(localization);
                    using var memoryStream = new MemoryStream();
                    using var writer = new StreamWriter(memoryStream);
                    await writer.WriteAllTextAsync(fileText);
                    memoryStream.Position = 0;
                    await botClient.SendDocumentAsync(chatId, InputFile.FromStream(memoryStream, $"localization.json"));
                }
                else
                {
                    Log.WriteInfo($"Admin command {text} ignored");
                }
            }
        }
    }
}
