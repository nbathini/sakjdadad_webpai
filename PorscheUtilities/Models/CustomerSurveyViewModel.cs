namespace PorscheUtilities.Models
{
    public class CustomerSurveyViewModel
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime DeliveryTime { get; set; }
        public string? Model { get; set; }
        public string? QuestionResponse { get; set; }
        public bool IsResponseSubmitted { get; set; }
        public string? CentreName { get; set; }
        public string? token { get; set; }
    }
}
