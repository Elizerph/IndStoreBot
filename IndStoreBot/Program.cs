using Newtonsoft.Json;

using System.Reflection;

namespace IndStoreBot
{
    internal class Program
    {
        private const string AdminVariableName = "telegrambotadmin";
        private const string TokenVariableName = "telegrambottoken";
        private const string SettingsFilePath = "settings.json";

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
            var settingsBundleProvider = new SettingsBundleProvider(SettingsFilePath);
            var bot = new BotLauncher(token, adminKey, settingsBundleProvider);
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, _) =>
            {
                cts.Cancel();
            };

            Log.WriteInfo("Bot started. Press ^C to stop");
            await bot.Start(cts.Token);
            await Task.Delay(-1, cts.Token);
        }
    }
}