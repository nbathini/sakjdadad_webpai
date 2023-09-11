using System.Text.Json.Serialization;

namespace PorscheUtilities.Models
{
    public class UserDetailViewModel
    {
        [JsonIgnore]
        public long Id { get; set; }
        public string? RoleName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool IsActive { get; set; }
        public long CentreId { get; set; }
    }
}
