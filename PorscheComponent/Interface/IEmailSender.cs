using PorscheUtilities.Models;
using System.Net.Mail;

namespace PorscheComponent.Interface
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendEmailAsync(string toEmail, string subject, string message, List<Attachment> attachments, List<ImageEmbed> imageList);
        Task SendEmailAsync(string toEmail, string ccEmail, string subject, string message, List<Attachment> attachments);
        Task SendEmailAsync(string toEmail, string subject, string message, List<ImageEmbed> imageList);
    }
}
