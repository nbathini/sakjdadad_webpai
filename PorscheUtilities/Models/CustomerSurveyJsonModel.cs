namespace PorscheUtilities.Models
{
    public class CustomerSurveyJsonModel
    {
        public string? Type { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public string? Name { get; set; }
        public bool IsVisible { get; set; }
        public bool IsInfoSheet { get; set; }
        public string? MinLabel { get; set; }
        public string? MaxLabel { get; set; }
        public string? Min { get; set; }
        public string? Max { get; set; }
        public string? Step { get; set; }
        public List<string>? Options { get; set; }
    }
}
