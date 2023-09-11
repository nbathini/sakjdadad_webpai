using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PorscheDataAccess.DBModels
{
    public class DeliveryHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DeliveryId { get; set; }
        public long CustomerId { get; set; }
        public long SalesConsultantId { get; set; }
        public long ServiceAdvisorId { get; set; }
        public long PorscheProId { get; set; }
        public int ModelId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime DeliveryTime { get; set; }
        public bool IsSurveySent { get; set; }
        public long CentreId { get; set; }
        public bool SkipSurvey { get; set; }
        public int UserRole { get; set; }
        public string Status { get; set; }
        public int DeliveryTypeId { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
