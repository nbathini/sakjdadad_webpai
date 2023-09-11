namespace PorscheUtilities.Models
{
    public class CarViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImagePath { get; set; }
        //public string? Base64Image { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
