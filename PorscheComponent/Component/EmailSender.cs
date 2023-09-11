using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PorscheComponent.Interface;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace PorscheComponent.Component
{
    public class EmailSender : IEmailSender
    {
        #region Private Variables

        private readonly IConfiguration _configuration;
        private readonly ISecretsManagerComponent _secretsManagerComponent;
        private readonly ILogger<EmailSender> _logger;

        #endregion

        #region Constructor
        public EmailSender(IConfiguration configuration, ISecretsManagerComponent secretsManagerComponent, ILogger<EmailSender> logger)
        {
            _secretsManagerComponent = secretsManagerComponent;
            _configuration = configuration;
            _logger = logger;
        }

        #endregion

        #region Send Email Methods

        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            return SendEmail(toEmail, null, subject, message, null, null);
        }

        public Task SendEmailAsync(string toEmail, string subject, string message, List<Attachment> attachments, List<ImageEmbed> imageList)
        {
            return SendEmail(toEmail, null, subject, message, attachments, imageList);
        }

        public Task SendEmailAsync(string toEmail, string ccEmail, string subject, string message, List<Attachment> attachments)
        {
            return SendEmail(toEmail, ccEmail, subject, message, attachments, null);
        }

        public Task SendEmailAsync(string toEmail, string subject, string message, List<ImageEmbed> imageList)
        {
            return SendEmail(toEmail, null, subject, message, null, imageList);
        }

        private Task SendEmail(string toEmail, string ccEmail, string subject, string message, List<Attachment> attachments, List<ImageEmbed> imageList)
        {
            try
            {
                string emailSecretName = _configuration.GetSection("EmailSecretName").Value;
                string region = _configuration.GetSection("Region").Value;

                AWSSecrets emailSecrets = new AWSSecrets();

                emailSecrets = _secretsManagerComponent.GetSecret(emailSecretName, region);

                if (emailSecrets != null)
                {
                    MailMessage mail;
                    System.Net.NetworkCredential auth;
                    SmtpClient client;
                    if (Convert.ToBoolean(_configuration.GetSection(Constants.AppSecretEnabled).Value))
                    {
                        mail = new MailMessage(_configuration.GetSection(Constants.SmtpEmailFrom).Value, toEmail, subject, message);
                        auth = new System.Net.NetworkCredential(emailSecrets.username, emailSecrets.password);
                        client = new SmtpClient(emailSecrets.host, Convert.ToInt32(emailSecrets.port));
                        client.EnableSsl = Convert.ToBoolean(_configuration[Constants.EnableSsl]);
                    }
                    else
                    {
                        mail = new MailMessage(_configuration.GetSection(Constants.SmtpEmailFrom).Value, toEmail, subject, message);
                        auth = new System.Net.NetworkCredential(emailSecrets.username, emailSecrets.password);
                        client = new SmtpClient(emailSecrets.host, Convert.ToInt32(emailSecrets.port));
                        client.EnableSsl = Convert.ToBoolean(_configuration.GetSection(Constants.SmtpEnableSsl).Value);
                    }

                    if (attachments != null && attachments.Count > 0)
                    {
                        foreach (var attachment in attachments)
                        {
                            mail.Attachments.Add(attachment);
                        }
                    }

                    if (imageList != null && imageList.Count > 0)
                    {
                        AlternateView htmlView = AlternateView.CreateAlternateViewFromString(message, null, MediaTypeNames.Text.Html);

                        foreach (var image in imageList)
                        {
                            LinkedResource theEmailImage = new LinkedResource(image.Path, MediaTypeNames.Image.Jpeg);
                            theEmailImage.ContentId = image.Id;
                            htmlView.LinkedResources.Add(theEmailImage);
                        }
                        mail.AlternateViews.Add(htmlView);
                    }

                    client.UseDefaultCredentials = false;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    mail.BodyEncoding = Encoding.UTF8;
                    mail.SubjectEncoding = Encoding.Default;
                    mail.IsBodyHtml = true;

                    if (!string.IsNullOrEmpty(ccEmail))
                    {
                        mail.CC.Add(ccEmail);
                    }

                    client.Credentials = auth;
                    client.Send(mail);
                }

                
            }
            catch (Exception ex)
            {
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(EmailSender), nameof(SendEmail), ex));
            }
            return Task.CompletedTask;
        }

        #endregion

    }
}
