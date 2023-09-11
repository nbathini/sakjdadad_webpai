namespace PorscheUtilities.Models
{
    public class UserInfoViewModel
    {
        public int Id { get; set; }        
        
        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        
        public string? Phone { get; set; }
        
        public string? RoleName { get; set; }        
        
        public string? CentreName { get; set; }        
        
        public string? JobRole { get; set; }        
        
        public bool IsActive { get; set; }       
    }

    public class StaffViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        public string? Designation { get; set; }
        public bool IsActive { get; set; }
    }
}
