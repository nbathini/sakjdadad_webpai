using Microsoft.AspNetCore.Http;

namespace PorscheUtilities.Models
{
    public class DocumentUploadViewModel
    {
        public List<IFormFile> Files { get; set; }
        public string? SubPath { get; set; }
        public string? Question { get; set; }
    }
}
