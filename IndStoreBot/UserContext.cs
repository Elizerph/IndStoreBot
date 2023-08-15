using Telegram.Bot.Types;

namespace IndStoreBot
{
    public class UserContext
    {
        public long ChatId { get; }
        public long UserId { get; }
        public UserContextState State { get; set; } = UserContextState.AwaitContact;
        public Dictionary<string, string> TicketValues { get; set; }
        public Contact? Contact { get; set; }
        public string? CurrentAttributeId { get; set; }
        public Dictionary<string, TicketAttribute> Attributes { get; set; }
        public int CurrentMessageId { get; set; }

        public UserContext(long chatId, long userId)
        {
            ChatId = chatId;
            UserId = userId;
        }
    }
}
