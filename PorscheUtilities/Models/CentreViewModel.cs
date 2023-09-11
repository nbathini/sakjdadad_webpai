using PorscheUtilities.Utility;
using System.ComponentModel.DataAnnotations;

namespace PorscheUtilities.Models
{
    public class CentreViewModel
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = Messages.InvalidEmailAddress)]
        public string Email { get; set; }

        [RegularExpression(@"^(\+\d{1,2}\s?)?1?\-?\.?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = Messages.InvalidContactNumber)]
        public string ContactNumber { get; set; }
        public bool IsActive { get; set; }
        public int Staff { get; set; }
        
    }
}
