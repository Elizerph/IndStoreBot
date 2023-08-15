using Telegram.Bot;
using Telegram.Bot.Types;

namespace IndStoreBot.Handlers
{
    public abstract class UserSegregationHandler : BaseHandler
    {
        private readonly Dictionary<(long, long), UserContext> _contexts = new();

        protected override async Task HandleButton(ITelegramBotClient botClient, long chatId, long userId, int messageId, string? messageText, string? caption, string? buttonData)
        {
            var context = GetContext(chatId, userId);
            await HandleButton(botClient, context, messageId, messageText, caption, buttonData);
        }

        protected override async Task HandleMessage(ITelegramBotClient botClient, long chatId, long userId, string? text, bool isCommand, Contact? contact, Document? document)
        {
            var context = GetContext(chatId, userId);
            await HandleMessage(botClient, context, isCommand, text, contact);
        }

        private UserContext GetContext(long chatId, long userId)
        {
            if (!_contexts.TryGetValue((chatId, userId), out var result))
            {
                result = new UserContext(chatId, userId);
                _contexts[(chatId, userId)] = result;
            }
            return result;
        }

        protected abstract Task HandleButton(ITelegramBotClient botClient, UserContext context, int messageId, string? messageText, string? caption, string? buttonData);

        protected abstract Task HandleMessage(ITelegramBotClient botClient, UserContext context, bool isCommand, string? text, Contact? contact);
    }
}
