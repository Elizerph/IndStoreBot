using IndStoreBot.Access;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace IndStoreBot.Handlers
{
    public class AdminHandler : BaseHandler
    {
        private readonly long _adminId;
        private readonly Dictionary<string, IReadWriteAccess<Stream>> _confugurationAccesses;
        private readonly IReadWriteSetAccess<Stream> _customFilesAccess;

        public AdminHandler(long adminId, Dictionary<string, IReadWriteAccess<Stream>> confugurationAccesses, IReadWriteSetAccess<Stream> customFilesAccess)
        {
            _adminId = adminId;
            _confugurationAccesses = confugurationAccesses;
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
                if (_confugurationAccesses.TryGetValue(fileName, out var access))
                {
                    await access.Write(memoryStream);
                    await botClient.SendTextMessageAsync(chatId, $"New configuration {fileName} applied");
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
                else if (_confugurationAccesses.TryGetValue(text, out var access))
                {
                    using var stream = await access.Read();
                    await botClient.SendDocumentAsync(chatId, InputFile.FromStream(stream, $"{text}.json"));
                }
                else
                {
                    Log.WriteInfo($"Admin command {text} ignored");
                }
            }
        }
    }
}
