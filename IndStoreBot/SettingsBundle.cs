namespace IndStoreBot
{
    public class SettingsBundle
    {
        public long TargetChatId { get; set; }
        public IReadOnlyCollection<TicketAttribute> Attributes { get; set; }
    }
}
