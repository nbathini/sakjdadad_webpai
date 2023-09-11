using System.ComponentModel.DataAnnotations;

namespace PorscheUtilities.Models
{
    public class UserLoginViewModel
    {
        [Required]
        public string? UserEmail { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
