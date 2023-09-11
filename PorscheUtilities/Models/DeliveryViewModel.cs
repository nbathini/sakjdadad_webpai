using PorscheUtilities.Utility;
using System.ComponentModel.DataAnnotations;

namespace PorscheUtilities.Models
{
    public class DeliveryViewModel
    {
        public int Id { get; set; }
        [Required]

        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = Messages.InvalidEmailAddress)]
        public string? CustomerEmail { get; set; }
        public int CustomerId { get; set; }
        [Required]
        public string? CustomerName { get; set; }
        [Required]
        public UserInfoViewModel SalesConsultant { get; set; }
        [Required]
        public UserInfoViewModel PorschePro { get; set; }
        [Required]
        public UserInfoViewModel ServiceAdvisor { get; set; }
        [Required]
        [RegularExpression(@"^(\+\d{1,2}\s?)?1?\-?\.?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = Messages.InvalidContactNumber)]
        public string? ContactNumber { get; set; }
        [Required]
        public CarViewModel Model { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DeliveryDate { get; set; }

        [Required]
        public DateTime DeliveryTime { get; set; }
        public bool IsSurveySent { get; set; }
        public bool SkipSurvey { get; set; }
        [Required]
        public DeliveryTypeViewModel DeliveryType { get; set; }
        public string? CentreName { get; set; }
        public int DeliveryStatus { get; set; }
    }
}
