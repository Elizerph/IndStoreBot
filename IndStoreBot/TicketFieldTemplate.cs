namespace IndStoreBot
{
    public class TicketFieldTemplate
    {
        public string TextId { get; set; }
        public string FieldId { get; set; }
        public IReadOnlyCollection<TicketFieldOption> Buttons { get; set; }
    }
}
