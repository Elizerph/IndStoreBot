using Newtonsoft.Json;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace IndStoreBot
{
    public class BotLauncher
    {
        private readonly string _token;
        private readonly long _admin;
        private readonly Dictionary<(long, long), UserContext> _contexts = new();
        private TelegramBotClient _client;
        private readonly SettingsBundleProvider _settingsProvider;

        public BotLauncher(string token, long admin, SettingsBundleProvider settingsProvider)
        {
            _token = token ?? throw new ArgumentNullException(nameof(token));
            _admin = admin;
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            _client = new TelegramBotClient(_token);
            await _client.SetMyCommandsAsync(new [] 
            { 
                new BotCommand 
                { 
                    Command = "start",
                    Description = "Заказать футболку"
                }
            }, null, null, cancellationToken);
            await _client.ReceiveAsync(HandleUpdate, HandleException, null, cancellationToken);
        }

        private async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    var message = update.Message;
                    if (message != null)
                    {
                        var user = message.From;
                        if (user == null)
                        {
                            Log.WriteError("Update message has no user");
                        }
                        else
                        {
                            var messageChat = message.Chat;
                            switch (message.Chat.Type)
                            {
                                case ChatType.Private:
                                    if (user.Id == _admin && message.Document != null)
                                    {
                                        using var memoryStream = new MemoryStream();
                                        await _client.GetInfoAndDownloadFileAsync(message.Document.FileId, memoryStream);
                                        memoryStream.Position = 0;
                                        using var reader = new StreamReader(memoryStream);
                                        var text = await reader.ReadToEndAsync();
                                        try
                                        {
                                            var newSettings = JsonConvert.DeserializeObject<SettingsBundle>(text);
                                            await _settingsProvider.SaveAndApply(newSettings);
                                            await _client.SendTextMessageAsync(messageChat.Id, "Новые настройки применены");
                                        }
                                        catch (JsonException)
                                        {
                                            await _client.SendTextMessageAsync(messageChat.Id, "Настройки не распознаны");
                                        }
                                    }
                                    else
                                    {
                                        if (!_contexts.TryGetValue((messageChat.Id, user.Id), out var context))
                                        {
                                            context = new(_client, messageChat.Id, _settingsProvider);
                                            _contexts[(messageChat.Id, user.Id)] = context;
                                        }
                                        var contact = message.Contact;
                                        if (contact != null)
                                        {
                                            await _client.SendTextMessageAsync(messageChat.Id, "Контакт подтвержден", replyMarkup: new ReplyKeyboardRemove());
                                            await context.Contact(contact);
                                        }
                                        else
                                        {
                                            var entities = message.Entities;
                                            if (entities?.SingleOrDefault()?.Type == MessageEntityType.BotCommand)
                                            {
                                                var messageText = message.Text;
                                                if (string.IsNullOrEmpty(messageText))
                                                {
                                                    Log.WriteError("Command with empty text ingored");
                                                }
                                                else
                                                {
                                                    switch (messageText.ToLower())
                                                    {
                                                        case "/start":
                                                            await context.Start();
                                                            break;
                                                        default:
                                                            Log.WriteInfo($"Command ignored: {messageText}");
                                                            break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                await context.Text(message.Text);
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    Log.WriteInfo($"Message chat type unhadled: {messageChat.Type}");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Log.WriteError("Update type is Message, but message us null");
                    }
                    break;
                case UpdateType.CallbackQuery:
                    var query = update.CallbackQuery;
                    if (query != null)
                    {
                        var queryMessage = query.Message;
                        if (queryMessage != null)
                        {
                            var queryMessageChat = queryMessage.Chat;
                            var queryUser = query.From;
                            if (!_contexts.TryGetValue((queryMessageChat.Id, queryUser.Id), out var context))
                            {
                                context = new(_client, queryMessage.Chat.Id, _settingsProvider);
                                _contexts[(queryMessageChat.Id, queryUser.Id)] = context;
                            }
                            var selectedLabel = await context.Button(query.Data, queryMessage.MessageId);
                            await client.EditMessageTextAsync(queryMessageChat.Id.ToString(), queryMessage.MessageId, string.Join(Environment.NewLine, new[] { queryMessage.Text, selectedLabel }));
                        }
                        else
                        {
                            Log.WriteError("Callback query message is null");
                        }
                    }
                    else
                    {
                        Log.WriteError("Update type is CallbackQuery, but query us null");
                    }
                    break;
                default:
                    Log.WriteInfo($"Update type unhadled: {update.Type}");
                    break;
            }
        }

        private async Task HandleException(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            Log.WriteError("Error", exception);
        }
    }
}
