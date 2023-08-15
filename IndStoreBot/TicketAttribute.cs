namespace IndStoreBot
{
    public class TicketAttribute
    {
        public string Id { get; set; }
        public string InvariantName { get; set; }
        public string InvariantMessage { get; set; }
        public string File { get; set; }
        public string NextId { get; set; }
        public IReadOnlyCollection<TicketAttributeOption> ButtonResponses { get; set; }
    }
}
