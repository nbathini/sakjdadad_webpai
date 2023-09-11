namespace PorscheUtilities.Models
{
    public class UserRoleModuleViewModel
    {
        public int Id { get; set; }
        public int UserInfoId { get; set; }
        public int RoleModuleId { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
