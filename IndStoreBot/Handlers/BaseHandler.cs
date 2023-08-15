using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace IndStoreBot.Handlers
{
    public abstract class BaseHandler : IUpdateHandler
    {
        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    var message = update.Message;
                    if (message == null)
                    {
                        Log.WriteError("Update type is Message, but message is null");
                        return;
                    }
                    var messageUser = message.From;
                    if (messageUser == null)
                    {
                        Log.WriteError("Message author is null");
                        return;
                    }
                    var messageChat = message.Chat;
                    if (messageChat == null)
                    {
                        Log.WriteError("Message chat is null");
                        return;
                    }
                    switch (messageChat.Type)
                    {
                        case ChatType.Private:
                            var isCommand = message.Entities?.SingleOrDefault(e => e.Type == MessageEntityType.BotCommand) != null;
                            await HandleMessage(botClient, messageChat.Id, messageUser.Id, message.Text, isCommand, message.Contact, message.Document);
                            break;
                        default:
                            Log.WriteInfo($"Chat type unhadled: {messageChat.Type}");
                            break;
                    }
                    break;
                case UpdateType.CallbackQuery:
                    var query = update.CallbackQuery;
                    if (query == null)
                    {
                        Log.WriteError("Update type is CallbackQuery, but query is null");
                        return;
                    }
                    var queryMessage = query.Message;
                    if (queryMessage == null)
                    {
                        Log.WriteError("Query message is null");
                        return;
                    }
                    var queryUser = query.From;
                    if (queryUser == null)
                    {
                        Log.WriteError("Query  user is null");
                        return;
                    }
                    var queryMessageChat = queryMessage.Chat;
                    if (queryMessageChat == null)
                    {
                        Log.WriteError("Query message chat is null");
                        return;
                    }
                    var queryData = query.Data;
                    if (queryData == null)
                    {
                        Log.WriteError("Query data is null");
                        return;
                    }
                    await HandleButton(botClient, queryMessageChat.Id, queryUser.Id, queryMessage.MessageId, queryMessage.Text, queryMessage.Caption, queryData);
                    break;
                default:
                    Log.WriteInfo($"Update type unhadled: {update.Type}");
                    break;
            }
        }

        protected abstract Task HandleMessage(ITelegramBotClient botClient, long chatId, long userId, string? text, bool isCommand, Contact? contact, Document? document);
        protected abstract Task HandleButton(ITelegramBotClient botClient, long chatId, long userId, int messageId, string? messageText, string? caption, string? buttonData);
    }
}
