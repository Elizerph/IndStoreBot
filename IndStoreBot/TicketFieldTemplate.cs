namespace IndStoreBot
{
    public class TicketFieldTemplate
    {
        public string Id { get; set; }
        public string MessageText { get; set; }
        public IReadOnlyCollection<TicketFieldOption> Options { get; set; }
    }
}
