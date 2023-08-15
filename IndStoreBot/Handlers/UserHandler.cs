using IndStoreBot.Access;
using IndStoreBot.Extensions;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace IndStoreBot.Handlers
{
    public class UserHandler : UserSegregationHandler
    {
        private readonly IReadAccess<SettingsBundle> _settingsProvider;
        private readonly IReadAccess<Dictionary<string, string>> _localizationProvider;
        private readonly IReadSetAccess<Stream> _filesProvider;

        public UserHandler(IReadAccess<SettingsBundle> settingsProvider, IReadAccess<Dictionary<string, string>> localizationProvider, IReadSetAccess<Stream> filesProvider)
        {
            _settingsProvider = settingsProvider;
            _localizationProvider = localizationProvider;
            _filesProvider = filesProvider;
        }

        protected override async Task HandleButton(ITelegramBotClient botClient, UserContext context, int messageId, string? messageText, string? caption, string? buttonData)
        {
            if (messageId != context.CurrentMessageId)
                return;
            switch (context.State)
            {
                case UserContextState.FillTicketFields:
                    if (!context.Attributes.TryGetValue(context.CurrentAttributeId, out var currentAttribute))
                    {
                        Log.WriteError($"Attribute with {context.CurrentAttributeId} does not exists");
                        await ResetAttributes(context);
                        await ProcessAttributes(context, botClient);
                        return;
                    }
                    var selectedOption = currentAttribute.ButtonResponses.First(e => (e.InvariantValue ?? e.InvariantLabel) == buttonData);
                    var selectedLabel = await Localize(selectedOption.InvariantLabel);
                    var newText = string.Join(Environment.NewLine, new[] 
                    { 
                        messageText ?? caption, 
                        "------", 
                        selectedLabel 
                    });
                    if (messageText != null)
                        await botClient.EditMessageTextAsync(context.ChatId, messageId, newText);
                    else if (caption != null)
                        await botClient.EditMessageCaptionAsync(context.ChatId, messageId, newText);
                    context.TicketValues[await Localize(currentAttribute.InvariantName ?? currentAttribute.Id)] = await Localize(selectedOption.InvariantValue ?? selectedOption.InvariantLabel);
                    context.CurrentAttributeId = selectedOption.NextId ?? currentAttribute.NextId;
                    await ProcessAttributes(context, botClient);
                    break;
                default:
                    break;
            }
        }

        protected override async Task HandleMessage(ITelegramBotClient botClient, UserContext context, bool isCommand, string? text, Contact? contact)
        {
            if (isCommand && text == "/start")
            {
                if (context.Contact == null)
                {
                    context.State = UserContextState.AwaitContact;
                    await botClient.SendTextMessageAsync(context.ChatId, await Localize("greetings_text"));
                    await botClient.SendTextMessageAsync(context.ChatId, await Localize("user_contact_request_text"), replyMarkup:
                        new ReplyKeyboardMarkup(
                            KeyboardButton.WithRequestContact(await Localize("user_contact_request_button"))
                            ));
                }
                else
                {
                    await ResetAttributes(context);
                    await ProcessAttributes(context, botClient);
                }
            }
            else
            {
                switch (context.State)
                {
                    case UserContextState.AwaitContact:
                        if (contact != null)
                        {
                            context.Contact = contact;
                            await botClient.SendTextMessageAsync(context.ChatId, await Localize("user_contact_confirmed"), replyMarkup: new ReplyKeyboardRemove());
                            await ResetAttributes(context);
                            await ProcessAttributes(context, botClient);
                        }
                        break;
                    case UserContextState.FillTicketFields:
                        if (!context.Attributes.TryGetValue(context.CurrentAttributeId, out var currentAttribute))
                        {
                            Log.WriteError($"Attribute with {context.CurrentAttributeId} does not exists");
                            await ResetAttributes(context);
                            await ProcessAttributes(context, botClient);
                            return;
                        }
                        if (currentAttribute.ButtonResponses != null)
                            return;
                        context.TicketValues[await Localize(currentAttribute.InvariantName ?? currentAttribute.Id)] = text;
                        context.CurrentAttributeId = currentAttribute.NextId;
                        await ProcessAttributes(context, botClient);
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task ResetAttributes(UserContext context)
        {
            var settings = await _settingsProvider.Read();
            context.Attributes = settings.Attributes.ToDictionary(e => e.Id);
            context.CurrentAttributeId = settings.FirstAttributeId;
            context.State = UserContextState.FillTicketFields;
            context.TicketValues = new();
        }

        private async Task ProcessAttributes(UserContext context, ITelegramBotClient botClient)
        {
            if (string.IsNullOrEmpty(context.CurrentAttributeId))
            {
                context.State = UserContextState.AwaitTicketRequest;
                await botClient.SendTextMessageAsync(context.ChatId, await Localize("user_request_accepted_text"));
                var requestLines = new List<string>
                {
                    $"Заявка от {string.Join(' ', new[] 
                    { 
                        context.Contact.FirstName,
                        context.Contact.LastName,
                        context.Contact.PhoneNumber 
                    })}"
                };
                requestLines.AddRange(context.TicketValues.Select(p => $"{p.Key}: {p.Value}"));
                var settings = await _settingsProvider.Read();
                if (settings.TargetChatId != 0L)
                {
                    await botClient.SendTextMessageAsync(settings.TargetChatId, string.Join(Environment.NewLine, requestLines));
                    await botClient.SendContactAsync(settings.TargetChatId, context.Contact.PhoneNumber, context.Contact.FirstName, null, context.Contact.LastName, context.Contact.Vcard);
                }
            }
            else
            {
                if (!context.Attributes.TryGetValue(context.CurrentAttributeId, out var currentAttribute))
                {
                    Log.WriteError($"Attribute with {context.CurrentAttributeId} does not exists");
                    await ResetAttributes(context);
                    await ProcessAttributes(context, botClient);
                    return;
                }
                IReplyMarkup? replyMarkup;
                if (currentAttribute.ButtonResponses != null)
                {
                    replyMarkup = new InlineKeyboardMarkup(
                        await Task.WhenAll(currentAttribute.ButtonResponses.Select(async e =>
                        {
                            var label = await Localize(e.InvariantLabel);
                            return new[]
                            {
                                new InlineKeyboardButton(label)
                                {
                                    CallbackData = e.InvariantValue ?? e.InvariantLabel
                                }
                            };
                        }))
                    );
                }
                else
                {
                    replyMarkup = null;
                }
                Message sent;
                if (string.IsNullOrEmpty(currentAttribute.File))
                    sent = await botClient.SendTextMessageAsync(context.ChatId, await Localize(currentAttribute.InvariantMessage), replyMarkup: replyMarkup);
                else
                {
                    using var stream = await _filesProvider.Read(currentAttribute.File);
                    sent = await botClient.SendPhotoAsync(context.ChatId, InputFile.FromStream(stream), caption: await Localize(currentAttribute.InvariantMessage), replyMarkup: replyMarkup);
                }
                context.CurrentMessageId = sent.MessageId;
            }
        }

        private async Task<string> Localize(string id)
        {
            var localization = await _localizationProvider.Read();
            if (!localization.TryGetValue(id, out var result))
                result = id;
            return result.InsertEmo();
        }
    }
}
