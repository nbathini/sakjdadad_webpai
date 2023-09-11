namespace PorscheUtilities.Models
{
    public class PreDeliveryCheckListModel
    {
        public int Id { get; set; }
        public int DeliveryId { get; set; }
        public string? QuestionResponse { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
