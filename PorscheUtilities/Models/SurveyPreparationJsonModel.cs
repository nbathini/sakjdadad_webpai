namespace PorscheUtilities.Models
{
    public class SurveyPreparationJsonModel
    {
        public string Info { get; set; }
        public List<SurveyPrepField> Fields { get; set; }
    }
    public class SurveyPrepField
    {
        public string? Type { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public string? Name { get; set; }
        public bool Is_Required { get; set; }
        public bool IsInfoSheet { get; set; }
        public List<string>? Options { get; set; }
    }
}

