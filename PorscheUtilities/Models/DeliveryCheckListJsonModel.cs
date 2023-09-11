namespace PorscheUtilities.Models
{
    public class DeliveryCheckListJsonModel
    {
        public string? AllocatedTime { get; set; }
        public string? Name { get; set; }
        public string? Info { get; set; }
        public string? Label { get; set; }
        public List<Field>? Fields { get; set; }
        public List<Question>? Questions { get; set; }
    }
    public class Document
    {
        public long? Id { get; set; }
        public string? FileName { get; set; }
        public string? FileFullPath { get; set; }
        public string? FileType { get; set; }
        public string? FileSubPath { get; set; }
        public long? FileSize { get; set; }
        public bool? IsSent { get; set; }
        public DateTime? SentOn { get; set; }
    }

    public class CarImage
    {
        public long Id { get; set; }
        public string? ImageName { get; set; }
        public string? ImageFullPath { get; set; }
        public string? ImageType { get; set; }
        public string? ImageSubPath { get; set; }
        public long ImageSize { get; set; }
    }

    public class Question
    {
        public string? Name { get; set; }
        public string? Label { get; set; }
        public string? Answer { get; set; }
        public bool IsRequired { get; set; }
        public string? Type { get; set; }
        public bool MoveToFollowUp { get; set; }
        public bool? IsAddLink { get; set; }
        public bool? IsAddDoc { get; set; }
        public List<LinkViewModel>? Links { get; set; }
        public List<Document>? Documents { get; set; }
        
    }

    public class Field
    {
        public string? Name { get; set; }
        public string? Label { get; set; }
        public List<Question>? Questions { get; set; }
        public string? Type { get; set; }
        public string? Answer { get; set; }
    }
}
