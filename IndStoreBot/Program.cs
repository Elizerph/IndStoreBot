using IndStoreBot.Access;
using IndStoreBot.Extensions;
using IndStoreBot.Handlers;

using System.Reflection;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace IndStoreBot
{
    internal class Program
    {
        private const string AdminVariableName = "telegrambotadmin";
        private const string TokenVariableName = "telegrambottoken";

        static async Task Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyVersion = assembly.GetName().Version;
            Log.WriteInfo($"Version {assemblyVersion}");
            var token = Environment.GetEnvironmentVariable(TokenVariableName);
            if (string.IsNullOrEmpty(token))
            {
                Log.WriteError("Token not found");
                return;
            }
            var admin = Environment.GetEnvironmentVariable(AdminVariableName);
            if (string.IsNullOrEmpty(admin))
            {
                Log.WriteError("Admin is not specified");
                return;
            }
            if (!long.TryParse(admin, out var adminKey))
            {
                Log.WriteError("Admin key is not recognized");
                return;
            }
            var bot = new TelegramBotClient(token);
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, _) =>
            {
                cts.Cancel();
            };
            await bot.SetMyCommandsAsync(new[]
            {
                new BotCommand
                {
                    Command = "start",
                    Description = "New request"
                }
            });
            Log.WriteInfo("Bot started. Press ^C to stop");

            var dataFolder = FolderAccess.Current.GetSubFolder("Data");
            var settingsStreamAccess = dataFolder.GetFileAccess("settings.json");
            var localizationStreamAccess = dataFolder.GetFileAccess("localization.json");
            var customFilesAccess = dataFolder.GetSubFolder("CustomFiles");

            var updateHandler = new UpdateHandlerComposite(new IUpdateHandler[] 
            {
                new AdminHandler(adminKey, new Dictionary<string, IReadWriteAccess<Stream>>(FileAccessIdComparer.Intance)
                {
                    { "settings", settingsStreamAccess },
                    { "localization", localizationStreamAccess },
                }, customFilesAccess),
                new UserHandler(settingsStreamAccess.AsText().AsObject<SettingsBundle>().WithCache(),
                    localizationStreamAccess.AsText().AsObject<Dictionary<string, string>>().WithCache(),
                    customFilesAccess),
                new ErrorHandler()
            });
            await bot.ReceiveAsync(updateHandler, null, cts.Token);
            await Task.Delay(-1, cts.Token);
        }
    }
}