
namespace PorscheUtilities.Models
{
    public class AuditLogViewModel
    {
        public long? Id { get; set; }
        public string? EventName { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string? ActivityTime { get; set; }
        public long? CentreId { get; set; }        
        public string? CentreName { get; set; }        
        public string? RoleName { get; set; }        
        public string? ModuleName { get; set; }        
        public bool IsActive { get; set; }        
        public int? CreatedBy { get; set; }        
        public string? PerformedBy { get; set; }        
        public DateTime? CreatedDate { get; set; }
    }
}
