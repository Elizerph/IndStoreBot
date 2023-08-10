using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;

namespace IndStoreBot
{
    public class UserContext
    {
        private readonly TelegramBotClient _client;
        private readonly ChatId _userchat;
        private readonly ISingletonStorage<SettingsBundle> _settingsProvider;
        private readonly ISingletonStorage<Dictionary<string, string>> _localization;
        private readonly Dictionary<string, string> _ticketValues = new();
        private UserContextState _state;
        private int _currentFieldMessageId;
        private Contact _userContact;
        private LinkedList<TicketFieldTemplate> _ticketFields;
        private LinkedListNode<TicketFieldTemplate> _currentFieldNode;

        public UserContext(TelegramBotClient client, ChatId userChat, ISingletonStorage<SettingsBundle> settingsStorage, ISingletonStorage<Dictionary<string, string>> localization)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _userchat = userChat ?? throw new ArgumentNullException(nameof(userChat));
            _state = UserContextState.AwaitContact;
            _settingsProvider = settingsStorage ?? throw new ArgumentNullException(nameof(settingsStorage));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        public async Task Start()
        {
            switch (_state)
            {
                case UserContextState.AwaitContact:
                    await _client.SendTextMessageAsync(_userchat, await Localize("greetings_text"));
                    await _client.SendTextMessageAsync(_userchat, await Localize("user_contact_request_text"), replyMarkup:
                        new ReplyKeyboardMarkup(
                            KeyboardButton.WithRequestContact(await Localize("user_contact_request_button"))
                            ));
                    break;
                case UserContextState.AwaitTicketRequest:
                case UserContextState.FillTicketFields:
                    await StartProcess();
                    break;
                default:
                    break;
            }
        }

        public async Task Contact(Contact contact)
        {
            switch (_state)
            {
                case UserContextState.AwaitContact:
                    _userContact = contact;
                    await StartProcess();
                    break;
                default:
                    break;
            }
        }

        public async Task Text(string? text)
        {
            switch (_state)
            {
                case UserContextState.FillTicketFields:
                    _ticketValues[await Localize(_currentFieldNode.Value.FieldId)] = text;
                    _currentFieldNode = _currentFieldNode.Next;
                    await ProcessNode(_currentFieldNode);
                    break;
                default:
                    break;
            }
        }

        public async Task<string?> Button(string? data, int messageId)
        {
            if (messageId != _currentFieldMessageId)
                return null;
            switch (_state)
            {
                case UserContextState.FillTicketFields:
                    _ticketValues[await Localize(_currentFieldNode.Value.FieldId)] = await Localize(data);
                    var selectedOption = _currentFieldNode.Value.Buttons?.FirstOrDefault(e => e.TextId == data);
                    var selectedLabel = await Localize(selectedOption?.TextId);
                    var subTemplate = selectedOption?.Template;
                    if (subTemplate != null)
                    {
                        var subTemplateNode = new LinkedListNode<TicketFieldTemplate>(subTemplate);
                        _currentFieldNode = _ticketFields.AddAfter(_currentFieldNode, subTemplate);
                    }
                    else
                    {
                        _currentFieldNode = _currentFieldNode.Next;
                    }
                    await ProcessNode(_currentFieldNode);
                    return selectedLabel;
                default:
                    return null;
            }
        }

        private async Task StartProcess()
        {
            _state = UserContextState.FillTicketFields;
            var settings = await _settingsProvider.Load();
            _ticketFields = new(settings.Templates);
            _ticketValues.Clear();
            _currentFieldNode = _ticketFields.First;
            await ProcessNode(_currentFieldNode);
        }

        private async Task ProcessNode(LinkedListNode<TicketFieldTemplate> node)
        {
            if (node == null)
            {
                _state = UserContextState.AwaitTicketRequest;
                await _client.SendTextMessageAsync(_userchat, await Localize("user_request_accepted_text"));
                var requestLines = new List<string>
                {
                    $"Заявка от {string.Join(' ', new[] { _userContact.FirstName, _userContact.LastName, _userContact.PhoneNumber })}"
                };
                requestLines.AddRange(_ticketValues.Select(p => $"{p.Key}: {p.Value}"));
                var settings = await _settingsProvider.Load();
                if (settings.TargetChatId != 0L)
                {
                    await _client.SendTextMessageAsync(settings.TargetChatId, string.Join(Environment.NewLine, requestLines));
                    await _client.SendContactAsync(settings.TargetChatId, _userContact.PhoneNumber, _userContact.FirstName, null, _userContact.LastName, _userContact.Vcard);
                }
            }
            else
            {
                var template = node.Value;
                IReplyMarkup? replyMarkup;
                if (template.Buttons != null)
                {
                    replyMarkup = new InlineKeyboardMarkup(
                        await Task.WhenAll(template.Buttons.Select(async e => 
                        {
                            var localized = await Localize(e.TextId);
                            return new[]
                            {
                                new InlineKeyboardButton(localized)
                                {
                                    CallbackData = e.TextId
                                }
                            };
                        }))
                    );
                }
                else
                {
                    replyMarkup = null;
                }
                var sent = await _client.SendTextMessageAsync(_userchat, await Localize(template.TextId), replyMarkup: replyMarkup);
                _currentFieldMessageId = sent.MessageId;
            }
        }

        private async Task<string> Localize(string id)
        {
            return await _localization.Load(e => e.GetOrDefault(id));
        }
    }
}
