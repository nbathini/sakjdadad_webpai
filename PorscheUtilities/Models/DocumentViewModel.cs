using Microsoft.AspNetCore.Http;

namespace PorscheUtilities.Models
{
    public class DocumentViewModel
    {
        public List<IFormFile> Files { get; set; }
        public string? SubPath { get; set; }
    }
}
