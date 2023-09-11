using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PorscheDataAccess.DBModels
{
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? EventName { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string? ActivityTime { get; set; }
        public long? CentreId { get; set; }
        public string? RoleName { get; set; }
        public string? ModuleName { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
