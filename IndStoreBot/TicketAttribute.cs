namespace IndStoreBot
{
    public class TicketAttribute
    {
        public string NameId { get; set; }
        public string MessageId { get; set; }
        public IReadOnlyCollection<TicketAttributeOption> Options { get; set; }
    }
}
