using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using PorscheComponent.Interface;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace PorscheAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.Dealer)]
    public class SurveyController : ControllerBase
    {
        #region Private Variables

        private readonly ISurveyComponent _surveyComponent;
        private readonly IDeliveryComponent _deliveryComponent;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SurveyController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ICustomerComponent _customerComponent;
        private readonly IDocumentComponent _documentComponent;
        private readonly ILinkComponent _linkComponent;
        private readonly IAuditLogComponent _auditLogComponent;

        #endregion

        #region Constructor
        public SurveyController(ILogger<SurveyController> logger, ISurveyComponent surveyComponent, IDeliveryComponent deliveryComponent, IConfiguration configuration, IEmailSender emailSender, ICustomerComponent customerComponent, IDocumentComponent documentComponent, ILinkComponent linkComponent, IAuditLogComponent auditLogComponent)
        {
            _surveyComponent = surveyComponent;
            _deliveryComponent = deliveryComponent;
            _configuration = configuration;
            _logger = logger;
            _emailSender = emailSender;
            _customerComponent = customerComponent;
            _documentComponent = documentComponent;
            _linkComponent = linkComponent;
            _auditLogComponent = auditLogComponent;
        }

        #endregion

        #region APIs

        #region To add survey preparation details and send customer survey

        [HttpPost]        
        [Route(nameof(CustomerSurvey))]
        public async Task<ApiResponse> CustomerSurvey(SurveyPreparationViewModel surveyPreparationViewModel)
        {
            ApiResponse response = null;
            try
            {
                if (ModelState.IsValid)
                {
                    var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                    long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                    var userRole = CommonFunctions.GetUserRole(bearerToken);
                    var centreId = CommonFunctions.GetCentreId(bearerToken);

                    if (await _surveyComponent.AddEditDeliveryPreparation(surveyPreparationViewModel, loggedInuserId, centreId, userRole) > 0)
                    {
                        var delivery = await _deliveryComponent.GetDeliveryById(centreId, surveyPreparationViewModel.DeliveryId);
                        
                        if (delivery?.SkipSurvey != true)
                        {
                            if (await _surveyComponent.UpdateCustomerSurveyStatus(delivery.Id, loggedInuserId, centreId, userRole) > 0)
                            {
                                await CustomerSurveyMail(surveyPreparationViewModel, loggedInuserId, centreId, userRole);
                                response = new ApiResponse(Messages.SurveySent, true);
                            }
                            else
                            {
                                response = new ApiResponse(Messages.ResponseSubmitted, true);
                            }
                        }
                        else
                        {
                            response = new ApiResponse(Messages.ResponseSubmitted, true);
                        }
                    }
                    else
                    {
                        response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                        response.IsError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(CustomerSurvey), ex));

            }
            return response;
        }

        #endregion

        #region To get survey preparation json

        [HttpGet]        
        [Route(nameof(DeliveryPreparation))]
        public async Task<ApiResponse> DeliveryPreparation(int deliveryId)
        {
            ApiResponse response = null;
            try
            {
                var surveyPreparation = await _surveyComponent.GetDeliveryPreparation(deliveryId);
                if (surveyPreparation != null && surveyPreparation.QuestionResponse != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, surveyPreparation);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, null);
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(DeliveryPreparation), ex));

            }
            return response;
        }

        #endregion

        #region To get pre delivery checklist json

        [HttpGet]        
        [Route(nameof(PreDeliveryChecklist))]
        public async Task<ApiResponse> PreDeliveryChecklist(int deliveryId)
        {
            ApiResponse response = null;
            try
            {
                var preDeliveryCheckList = await _surveyComponent.GetPreDeliveryCheckList(deliveryId);
                if (preDeliveryCheckList != null && preDeliveryCheckList.QuestionResponse != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, preDeliveryCheckList);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, null);
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(PreDeliveryChecklist), ex));

            }
            return response;
        }

        #endregion

        #region To submit pre delivery checklist

        [HttpPost]        
        [Route(nameof(PreDeliveryChecklist))]
        public async Task<ApiResponse> PreDeliveryChecklist(PreDeliveryCheckListModel preDeliveryCheckListModel)
        {
            ApiResponse response = null;
            try
            {
                if (ModelState.IsValid)
                {
                    var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                    long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                    var userRole = CommonFunctions.GetUserRole(bearerToken);
                    var centreId = CommonFunctions.GetCentreId(bearerToken);

                    if (await _surveyComponent.AddEditPreDeliveryCheckList(preDeliveryCheckListModel, loggedInuserId, centreId, userRole) > 0)
                    {
                        response = new ApiResponse(Messages.ResponseSubmitted, true);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                        response.IsError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(PreDeliveryChecklist), ex));

            }
            return response;
        }

        #endregion

        #region To get customer survey json

        [HttpGet]        
        [Route(nameof(CustomerSurvey))]
        [AllowAnonymous]
        public async Task<ApiResponse> CustomerSurvey(string accessToken)
        {
            ApiResponse response = null;
            try
            {
                var stream = accessToken;
                var handler = new JwtSecurityTokenHandler();
                var tokenS = handler.ReadToken(stream) as JwtSecurityToken;
                int deliveryId = Convert.ToInt32(tokenS.Claims.First(claim => claim.Type == Constants.Id).Value);
                int customerId = Convert.ToInt32(tokenS.Claims.First(claim => claim.Type == Constants.CustomerId).Value);
                
                var userRole = CommonFunctions.GetUserRole(accessToken);
                var loggedInuserId = CommonFunctions.GetLoggedInUserId(accessToken);
                var centreId = CommonFunctions.GetCentreId(accessToken);
                
                var customerSurvey = await _surveyComponent.GetCustomerSurveyJson(deliveryId, customerId);
                if (customerSurvey != null && customerSurvey.QuestionResponse != null)
                {
                    customerSurvey.CustomerName = Convert.ToString(tokenS.Claims.First(claim => claim.Type == Constants.CustomerName).Value);
                    customerSurvey.DeliveryDate = Convert.ToDateTime(tokenS.Claims.First(claim => claim.Type == Constants.DeliveryDate).Value);
                    customerSurvey.DeliveryTime = Convert.ToDateTime(tokenS.Claims.First(claim => claim.Type == Constants.DeliveryTime).Value);
                    customerSurvey.Model = Convert.ToString(tokenS.Claims.First(claim => claim.Type == Constants.Model).Value);
                    customerSurvey.CentreName = Convert.ToString(tokenS.Claims.First(claim => claim.Type == Constants.CentreName).Value);
                    response = new ApiResponse(Messages.RecordFetched, customerSurvey);

                    #region Add Audit Log - Customer Survey Clicked

                    await _auditLogComponent.AddAuditLog(AppRoles.Dealer, Messages.ADL_CustomerSurveyClicked.Replace("#CUSTOMER_NAME#", customerSurvey.CustomerName), centreId, userRole, loggedInuserId);

                    #endregion
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, null);
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(CustomerSurvey), ex));

            }
            return response;
        }

        #endregion

        #region To submit customer repsonse

        [HttpPost]        
        [Route(nameof(CustomerResponse))]
        [AllowAnonymous]
        public async Task<ApiResponse> CustomerResponse(CustomerSurveyViewModel customerSurveyViewModel)
        {
            ApiResponse response = null;
            try
            {
                if (ModelState.IsValid)
                {
                    long loggedInuserId = CommonFunctions.GetLoggedInUserId(customerSurveyViewModel.token);
                    var centreId = CommonFunctions.GetCentreId(customerSurveyViewModel.token);
                    var userRole = CommonFunctions.GetUserRole(customerSurveyViewModel.token);

                    if (await _surveyComponent.SubmitSurveyResponse(customerSurveyViewModel, loggedInuserId, centreId, userRole) > 0)
                    {
                        response = new ApiResponse(Messages.ResponseSubmitted, true);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                        response.IsError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(CustomerResponse), ex));

            }
            return response;
        }

        #endregion

        #region To get info sheet json

        [HttpGet]        
        [Route(nameof(InfoSheetInformation))]
        public async Task<ApiResponse> InfoSheetInformation(int deliveryId, int customerId)
        {
            ApiResponse response = null;
            try
            {
                var infoSheetData = await _surveyComponent.GetInfoSheetData(deliveryId, customerId);
                if (infoSheetData != null && infoSheetData.SurveyPreparationJson != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, infoSheetData);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, null);
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(InfoSheetInformation), ex));

            }
            return response;
        }

        #endregion

        #region To get delivery checklist json

        [HttpGet]        
        [Route(nameof(DeliveryChecklist))]
        public async Task<ApiResponse> DeliveryChecklist(int deliveryId)
        {
            ApiResponse response = null;
            try
            {
                var deliveryCheckList = await _surveyComponent.GetDeliveryCheckList(deliveryId);
                if (deliveryCheckList != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, deliveryCheckList);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, null);
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(DeliveryChecklist), ex));

            }
            return response;
        }

        #endregion

        #region To submit delivery checklist

        [HttpPost]        
        [Route(nameof(DeliveryChecklist))]
        public async Task<ApiResponse> DeliveryChecklist(DeliveryCheckListViewModel deliveryCheckListModel)
        {
            ApiResponse response = null;
            try
            {
                if (ModelState.IsValid)
                {
                    var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                    long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                    var userRole = CommonFunctions.GetUserRole(bearerToken);
                    var centreId = CommonFunctions.GetCentreId(bearerToken);

                    if (await _surveyComponent.AddEditDeliveryCheckList(deliveryCheckListModel, loggedInuserId, centreId, userRole) > 0)
                    {
                        response = new ApiResponse(Messages.ResponseSubmitted, true);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                        response.IsError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(DeliveryChecklist), ex));

            }
            return response;
        }

        #endregion

        #region To send documents

        [HttpPost]        
        [Route(nameof(SendDocuments))]
        public async Task<ApiResponse> SendDocuments(SendDocumentViewModel documentViewModel)
        {
            ApiResponse response = null;
            try
            {
                if (ModelState.IsValid)
                {
                    var customerDetails = await _customerComponent.GetCustomerDetail(documentViewModel.CustomerId);

                    if (customerDetails != null)
                    {
                        var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                        long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                        var userRole = CommonFunctions.GetUserRole(bearerToken);
                        var centreId = CommonFunctions.GetCentreId(bearerToken);

                        if (await _documentComponent.SendDocuments(documentViewModel, loggedInuserId, centreId, userRole) > 0)
                        {
                            await DocumentMail(documentViewModel);

                            await _auditLogComponent.AddAuditLog(AppRoles.Dealer, Messages.ADL_DocumentsSent.Replace("#CUSTOMER_NAME#", customerDetails.Name), centreId, userRole, loggedInuserId);

                            response = new ApiResponse(Messages.SendDocumentSucceed, true);
                        }
                        else
                        {
                            response = new ApiResponse(Messages.SendDocumentFailed, null, 400);
                            response.IsError = true;
                        }
                    }
                    else
                    {
                        response = new ApiResponse(Messages.CustomerNotPresent, null, 400);
                        response.IsError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(SendDocuments), ex));

            }
            return response;
        }

        #endregion        

        #endregion

        #region Private methods

        #region Send Customer Survey Email
        private async Task CustomerSurveyMail(SurveyPreparationViewModel surveyPreparationViewModel, long loggedInuserId, long centreId, string userRole)
        {
            string siteUrl = _configuration.GetSection(Constants.SiteUrl).Value;
            
            var tokenResponse = await GetCustomerSurveyToken(surveyPreparationViewModel.DeliveryId);

            var delivery = await _deliveryComponent.GetDeliveryById(centreId, surveyPreparationViewModel.DeliveryId);
            
            if (delivery != null && delivery.SkipSurvey != true && tokenResponse.StatusCode == 200)
            {
                var actualName = delivery.CustomerName;
                var deliveryDate = string.Format("{0:MMMM dd, yyyy}", delivery.DeliveryDate);
                var deliveryTime = delivery.DeliveryTime.ToString("hh:mm tt");
                
                await SendCustomerSurveyEmail(actualName, delivery.CustomerEmail, siteUrl + Constants.CustomerSurveyPath + Convert.ToString(tokenResponse.Result), delivery.Model.Name, delivery.Model.ImagePath, deliveryDate, deliveryTime);

                #region Add Audit Log - Customer Survey Sent

                await _auditLogComponent.AddAuditLog(AppRoles.Dealer, Messages.ADL_CustomerSurveySent.Replace("#CUSTOMER_NAME#", delivery.CustomerName), centreId, userRole, loggedInuserId);

                #endregion
            }
        }

        private async Task SendCustomerSurveyEmail(string name, string email, string link, string carModel, string carImagePath, string deliveryDate, string deliveryTime)
        {
            try
            {
                string templateKey = Constants.CustomerSurveyTemplatePath;

                string emailContent = string.Empty;
                string siteUrl = _configuration.GetSection(Constants.SiteUrl).Value;

                #region Commented Code

                //List<ImageEmbed> imageList = new List<ImageEmbed>();

                //imageList.Add(new ImageEmbed { Id = Constants.CustomerSurveyImageId, Path = _configuration.GetSection(Constants.CloudFrontURL).Value + Constants.CustomerSurveyImagePath });
                //imageList.Add(new ImageEmbed { Id = Constants.LogoImageId, Path = _configuration.GetSection(Constants.CloudFrontURL).Value + Constants.LogoImagePath });
                //imageList.Add(new ImageEmbed { Id = Constants.SurveyBannerImageId, Path = _configuration.GetSection(Constants.CloudFrontURL).Value + Constants.SurveyBannerImagePath });

                //imageList.Add(new ImageEmbed { Id = Constants.SurveyBannerImageId, Path = carImagePath });

                //imageList.Add(new ImageEmbed { Id = Constants.KnowAppImageId, Path = _configuration.GetSection(Constants.CloudFrontURL).Value + Constants.KnowAppImagePath });

                #endregion

                emailContent = await _surveyComponent.GetContentFromS3Object(templateKey);
                
                emailContent = emailContent.Replace("#CloudFrontURL#", _configuration.GetSection(Constants.CloudFrontURL).Value);

                emailContent = emailContent.Replace(Constants.CustomerSurveyLink, link);
                emailContent = emailContent.Replace(Constants.MailUserName, name);
                emailContent = emailContent.Replace(Constants.MailSiteUrl, siteUrl);
                emailContent = emailContent.Replace(Constants.CarModel, carModel);
                emailContent = emailContent.Replace(Constants.DeliveryDateMail, deliveryDate);
                emailContent = emailContent.Replace(Constants.DeliveryTimeMail, deliveryTime);
                
                Task.Run(() => _emailSender.SendEmailAsync(email, Constants.CustomerSurvey, emailContent, null));
                
            }
            catch (Exception ex)
            {
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(AuthController), nameof(SendCustomerSurveyEmail), ex));
            }
        }

        #endregion

        #region To get unique token to be used in filling customer form
        private async Task<ApiResponse> GetCustomerSurveyToken(int deliveryId)
        {
            ApiResponse response = null;
            var request = string.Empty;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);
                var centreId = CommonFunctions.GetCentreId(bearerToken);
                var role = CommonFunctions.GetRole(bearerToken);
                var roleId = CommonFunctions.GetUserRoleId(bearerToken);

                var delivery = await _deliveryComponent.GetDeliveryById(centreId, deliveryId);
                if (delivery != null && delivery.SkipSurvey != true)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection(Constants.JwtSecurityKey).Value));
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                            {
                                new Claim(Constants.Id, delivery.Id.ToString()),
                                new Claim(Constants.CustomerEmail, delivery.CustomerEmail),
                                new Claim(Constants.CustomerId,delivery.CustomerId.ToString()),
                                new Claim(Constants.CustomerName,delivery.CustomerName),
                                new Claim(Constants.SalesConsultant,delivery.SalesConsultant.Name),
                                new Claim(Constants.PorschePro,delivery.PorschePro.Name),
                                new Claim(Constants.ServiceAdvisor,delivery.ServiceAdvisor.Name),
                                new Claim(Constants.ContactNumber,delivery.ContactNumber),
                                new Claim(Constants.DeliveryDate,delivery.DeliveryDate.ToString()),
                                new Claim(Constants.DeliveryTime,delivery.DeliveryTime.ToString()),
                                new Claim(Constants.IsSurveySent,delivery.IsSurveySent.ToString()),
                                new Claim(Constants.Model,delivery.Model.Name),
                                new Claim(Constants.CentreName,delivery.CentreName),
                                new Claim(ClaimTypes.Role, role),
                                new Claim(Constants.RoleId, roleId.ToString()),
                                new Claim(Constants.RoleName, userRole),
                                new Claim(Constants.LoggedInUserId, loggedInuserId.ToString()),
                                new Claim(Constants.CentreId, centreId.ToString())
                            }),
                        Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration.GetSection(Constants.SurveyLinkValidFor).Value)),
                        NotBefore = DateTime.UtcNow,
                        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
                    request = tokenHandler.WriteToken(token);
                    response = new ApiResponse(Messages.TokenCreated, request);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, null, 400);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(SurveyController), nameof(GetCustomerSurveyToken), ex));

            }
            return response;
        }

        #endregion

        #region To send documents in email
        private async Task DocumentMail(SendDocumentViewModel documentViewModel)
        {
            var modelName = String.Empty;
            var stream = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(stream) as JwtSecurityToken;
            var centreId = Convert.ToInt64(tokenS.Claims.First(claim => claim.Type == Constants.CentreId).Value);
            var actualName = String.Empty;
            var customer = await _customerComponent.GetCustomerDetail(documentViewModel.CustomerId);
            
            List<string> linkList = new List<string>();
            List<Attachment> attachmentList = new List<Attachment>();
            MemoryStream ms;
            

            string path = Path.Combine(_configuration.GetSection(Constants.TempFolder).Value, Constants.Documents);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            List<Attachment> attachments = new List<Attachment>();

            foreach (var file in documentViewModel.FileList)
            {

                try
                {
                    var document = await _documentComponent.GetDocumentDetail(file);

                    string docKey = $"{Constants.Documents?.TrimEnd('/').ToLower()}/{document.FileName}";

                    string filePath = Path.Combine(path, document.FileName);

                    ms = await _surveyComponent.GetDocumentFromS3(docKey);

                    using (var strMem = new MemoryStream(ms.ToArray()))
                    {
                        strMem.Position = 0;

                        using (FileStream tempFile = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            strMem.WriteTo(tempFile);
                        }

                        attachments.Add(new Attachment(filePath));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Information, ex.ToString());
                }                                
            }


            foreach (var link in documentViewModel.LinkList)
            {
                var linkData = await _linkComponent.GetLinkDetail(link);
                linkList.Add(linkData.Path);
            }
            
            var delivery = await _deliveryComponent.GetDeliveryById(centreId, documentViewModel.DeliveryId);
            
            if (delivery != null)
            {
                modelName = delivery.Model.Name;
            }

            if (customer != null)
            {
                actualName = customer.Name;
            }

            await SendDocumentMail(actualName, customer.Email, modelName, documentViewModel.DeliveryStatus, attachments, linkList);
        }

        private async Task SendDocumentMail(string name, string email, string modelName, int deliveryStatus, List<Attachment> attachmentList, List<string> linkList)
        {
            try
            {
                string templateKey = string.Empty;

                if (deliveryStatus == (Int32)Enums.DeliveryType.PreDelivery)
                {
                    templateKey = Constants.PreDeliveryTemplatePath;                    
                }
                else
                {
                    templateKey = Constants.PostDeliveryTemplatePath;                   
                }

                string emailContent = string.Empty;

                string siteUrl = _configuration.GetSection(Constants.SiteUrl).Value;

                emailContent = await _surveyComponent.GetContentFromS3Object(templateKey);

                #region Links related code

                var linkListString = string.Empty;
                
                StringBuilder sb = new StringBuilder();
                
                foreach (var link in linkList)
                {
                    linkListString = CommonFunctions.CreateLinkHtml("Testing", link, _configuration.GetSection(Constants.CloudFrontURL).Value + Constants.ConnectImagePath);
                    sb.Append(linkListString);
                }

                linkListString = sb.ToString();

                #endregion

                emailContent = emailContent.Replace("#CloudFrontURL#", _configuration.GetSection(Constants.CloudFrontURL).Value);
                emailContent = emailContent.Replace(Constants.MailUserName, name);
                emailContent = emailContent.Replace(Constants.MailSiteUrl, siteUrl);
                emailContent = emailContent.Replace(Constants.LinkList, linkListString);
                emailContent = emailContent.Replace(Constants.ModelName, modelName);
                
                Task.Run(() => _emailSender.SendEmailAsync(email, Constants.SendDocuments, emailContent, attachmentList, null));

            }
            catch (Exception ex)
            {
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(AuthController), nameof(SendDocumentMail), ex));
            }

        }

        #endregion
        
        #endregion
    }
}
