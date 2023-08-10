namespace IndStoreBot
{
    public class SettingsBundle
    {
        public long TargetChatId { get; set; }
        public IReadOnlyCollection<TicketFieldTemplate> Templates { get; set; }
    }
}
