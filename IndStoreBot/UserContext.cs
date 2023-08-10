using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace IndStoreBot
{
    public class UserContext
    {
        private readonly TelegramBotClient _client;
        private readonly ChatId _userchat;
        private readonly SettingsBundleProvider _settingsProvider;
        private readonly Dictionary<string, string> _ticketValues = new();
        private UserContextState _state;
        private int _currentFieldMessageId;
        private Contact _userContact;
        private LinkedList<TicketFieldTemplate> _ticketFields;
        private LinkedListNode<TicketFieldTemplate> _currentFieldNode;

        public UserContext(TelegramBotClient client, ChatId userChat, SettingsBundleProvider settingsProvider)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _userchat = userChat ?? throw new ArgumentNullException(nameof(userChat));
            _state = UserContextState.AwaitContact;
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        }

        public async Task Start()
        {
            switch (_state)
            {
                case UserContextState.AwaitContact:
                    await _client.SendTextMessageAsync(_userchat, "Экран приветствия");
                    await _client.SendTextMessageAsync(_userchat, "Запрос на контакт юзера", replyMarkup:
                        new ReplyKeyboardMarkup(
                            KeyboardButton.WithRequestContact("Оставляет свой контакт")
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
                    _ticketValues[_currentFieldNode.Value.Id] = text;
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
                    _ticketValues[_currentFieldNode.Value.Id] = data;
                    var selectedOption = _currentFieldNode.Value.Options?.FirstOrDefault(e => e.Id == data);
                    var selectedLabel = selectedOption?.Label;
                    var subTemplate = selectedOption?.FieldTemplate;
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
            var templates = await _settingsProvider.GetTemplates();
            _ticketFields = new(templates);
            _ticketValues.Clear();
            _currentFieldNode = _ticketFields.First;
            await ProcessNode(_currentFieldNode);
        }

        private async Task ProcessNode(LinkedListNode<TicketFieldTemplate> node)
        {
            if (node == null)
            {
                _state = UserContextState.AwaitTicketRequest;
                await _client.SendTextMessageAsync(_userchat, "Ваша заявка принята.  В скором времени мы свяжемся с вами, чтобы уточнить детали заказа");
                var requestLines = new List<string>
                { 
                    $"Заявка от {string.Join(' ', new[] { _userContact.FirstName, _userContact.LastName, _userContact.PhoneNumber })}"
                };
                requestLines.AddRange(_ticketValues.Select(p => $"{p.Key}: {p.Value}"));
                var chatId = await _settingsProvider.GetChatId();
                await _client.SendTextMessageAsync(chatId, string.Join(Environment.NewLine, requestLines));
                await _client.SendContactAsync(chatId, _userContact.PhoneNumber, _userContact.FirstName, null, _userContact.LastName, _userContact.Vcard);
            }
            else
            {
                var template = node.Value;
                IReplyMarkup? replyMarkup;
                if (template.Options != null)
                {
                    replyMarkup = new InlineKeyboardMarkup(
                        template.Options.Select(e => new[] { new InlineKeyboardButton(e.Label) { CallbackData = e.Id } })
                    );
                }
                else
                {
                    replyMarkup = null;
                }
                var sent = await _client.SendTextMessageAsync(_userchat, template.MessageText, replyMarkup: replyMarkup);
                _currentFieldMessageId = sent.MessageId;
            }
        }
    }
}
