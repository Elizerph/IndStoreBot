using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace IndStoreBot.Handlers
{
    public class UpdateHandlerComposite : IUpdateHandler
    {
        private readonly IReadOnlyCollection<IUpdateHandler> _handlers;

        public UpdateHandlerComposite(IReadOnlyCollection<IUpdateHandler> handlers)
        {
            _handlers = handlers;
        }

        public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            foreach (var item in _handlers)
                await item.HandlePollingErrorAsync(botClient, exception, cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            foreach (var item in _handlers)
                await item.HandleUpdateAsync(botClient, update, cancellationToken);
        }
    }
}
