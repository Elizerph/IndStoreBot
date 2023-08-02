using ElizerBot;

using System.Reflection;

namespace IndStoreBot
{
    internal class Program
    {
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
            var updateHandler = new UpdateHandler();
            var bot = updateHandler.BuildAdapter(SupportedMessenger.Telegram, token);
            await bot.Init();
            //await bot.SetCommands(new Dictionary<string, string>
            //{

            //});
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, _) =>
            {
                cts.Cancel();
            };

            Log.WriteInfo("Bot started. Press ^C to stop");
            await Task.Delay(-1, cts.Token);
        }
    }
}