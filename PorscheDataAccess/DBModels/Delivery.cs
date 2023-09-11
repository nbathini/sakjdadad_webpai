using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PorscheDataAccess.DBModels
{
    public class Delivery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Customer Customer { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime DeliveryTime { get; set; }
        public bool IsSurveySent { get; set; }        
        public CarModel Model { get; set; }        
        public Centre Centre { get; set; }
        public UserInfo PorschePro { get; set; }
        public UserInfo SalesConsultant { get; set; }
        public UserInfo ServiceAdvisor { get; set; }
        public bool SkipSurvey { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
