namespace PorscheUtilities.Models
{
    public class DeliveryCheckListViewModel
    {
        public int Id { get; set; }
        public int DeliveryId { get; set; }
        public string? QuestionResponse { get; set; }
        public int DeliveryStatus { get; set; }
        public bool SkipSurvey { get; set; }
    }
}
