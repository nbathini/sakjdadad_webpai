namespace PorscheUtilities.Models
{
    public class SurveyPreparationViewModel
    {
        public int Id { get; set; }
        public int DeliveryId { get; set; }
        public int CustomerId { get; set; }
        public string? QuestionResponse { get; set; }
    }
}
