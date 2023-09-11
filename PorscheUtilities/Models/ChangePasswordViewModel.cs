using PorscheUtilities.Utility;
using System.ComponentModel.DataAnnotations;

namespace PorscheUtilities.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string? OldPassword { get; set; }
        [Required]
        [StringLength(15, ErrorMessage = Messages.PasswordMinLengthMessage, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
