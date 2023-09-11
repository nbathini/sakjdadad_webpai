using System.ComponentModel.DataAnnotations;

namespace PorscheUtilities.Models
{
    public class RoleModuleViewModel
    {
        public int Id { get; set; }
        [Required]
        public string? RoleName { get; set; }
        public string? RoleDescription { get; set; }

        [Required]
        public ModulesViewModel Modules { get; set; }
        public bool IsCreate { get; set; }
        public bool IsEdit { get; set; }
        public bool IsView { get; set; }
        public bool IsDelete { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
