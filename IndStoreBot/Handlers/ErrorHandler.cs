using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace IndStoreBot.Handlers
{
    public class ErrorHandler : IUpdateHandler
    {
        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Log.WriteError("Error", exception);
            return Task.CompletedTask;
        }

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
