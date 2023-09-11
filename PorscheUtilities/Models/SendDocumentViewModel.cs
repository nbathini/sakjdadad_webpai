namespace PorscheUtilities.Models
{
    public class SendDocumentViewModel
    {
        public List<int>? FileList { get; set; }
        public List<int>? LinkList { get; set; }
        public long CustomerId { get; set; }
        public long DeliveryId { get; set; }
        public long CheckListId { get; set; }
        public int DeliveryStatus { get; set; }

    }
}
