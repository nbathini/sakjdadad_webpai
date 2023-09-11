using PorscheUtilities.Utility;
using System.ComponentModel.DataAnnotations;

namespace PorscheUtilities.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = Messages.InvalidEmailAddress)]
        public string? Email { get; set; }
    }
}
