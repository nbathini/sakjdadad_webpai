namespace PorscheUtilities.Utility
{
    public static class Constants
    {
        #region General Constants

        public const string EmptySpace = " ";
        public const string LoggedInUserId = "LoggedInUserId";
        public const string RoleId = "RoleId";
        public const string RoleName = "RoleName";
        public const string CentreId = "CentreId";

        public const string DefaultCentreName = "DefaultCentreName";
        public const string DefaultPhoneNo = "DefaultPhoneNo";
        public const string CloudFrontURL = "CloudFrontURL";
        public const string AWSKMSKeyId = "AWSKMSKeyId";
        public const string SiteUrl = "SiteUrl";
        public const string SiteUrlApi = "SiteUrlApi";
        public const string TempFolder = "TempFolder";
        public const string DBSecretName = "DBSecretName";
        public const string EmailSecretName = "EmailSecretName";
        public const string Region = "Region";
        public const string ResetPasswordPath = "reset-password?token=";
        public const string SetPasswordPath = "set-password?token=";
        public const string AdminResetPasswordPath = "admin/reset-password?token=";
        public const string CustomerSurveyPath = "customer/survey?token=";

        public const string ForgotPasswordImageId = "ctaId";        
        public const string LogoImageId = "logoId";
        public const string SignupImageId = "inviteDealerId";
        public const string CustomerSurveyImageId = "surveyId";        
        public const string SurveyBannerImageId = "bannerId";        
        public const string KnowAppImageId = "appId";        
        public const string PreDeliveryBannerImageId = "parkedId";
        public const string PostDeliveryBannerImageId = "followUpId";
        public const string PreDeliveryLinkImageId = "connectId";        
        public const string SetPasswordLink = "[SetPasswordLink]";
        public const string CustomerSurveyLink = "[CustomerSurveyLink]";
        public const string MailUserName = "[UserName]";
        public const string MailSiteUrl = "[SiteUrl]";
        public const string LinkList = "[LinkList]";
        public const string ModelName = "[ModelName]";
        public const string MailEmail = "[Email]";
        public const string CarModel = "[CarModel]";
        public const string DeliveryDateMail = "[DeliveryDate]";
        public const string DeliveryTimeMail = "[DeliveryTime]";
        public const string ViewModelLog = "View Model : ";
        public const string MethodLog = " , Method : ";
        public const string ErrorLog = " , Error : ";
        public const string Id = "Id";
        public const string CustomerEmail = "CustomerEmail";
        public const string CustomerId = "CustomerId";
        public const string CustomerName = "CustomerName";
        public const string SalesConsultant = "SalesConsultant";
        public const string PorschePro = "PorschePro";
        public const string ServiceAdvisor = "ServiceAdvisor";
        public const string ContactNumber = "ContactNumber";
        public const string DeliveryDate = "DeliveryDate";
        public const string DeliveryTime = "DeliveryTime";
        public const string IsSurveySent = "IsSurveySent";
        public const string Model = "Model";
        public const string CentreName = "CentreName";
        public const string Jsonb = "jsonb";
        public const string Updated = "Updated";
        public const string Deleted = "Deleted";
        public const string Initial = "Initial";
        public const string ForgotPassword = "Porsche - Reset Password";
        public const string Sigup = "Invitation E-mail";
        public const string SendDocuments = "Documents";
        public const string CustomerSurvey = "Porsche Delivery Survey";
        public const string AppSecretEnabled = "AppSecretEnabled";
        public const string EmailFrom = "EmailFrom";
        public const string UserName = "SmtpUserName";
        public const string Password = "SmtpPassword";
        public const string Host = "Host";
        public const string Port = "Port";
        public const string EnableSsl = "EnableSsl";
        public const string FName = "First Name";
        public const string OtherPorscheVehicle = "otherVehicles";
        public const string ElectricVehicle = "electricVehicle";
        public const string TechnicalFeature = "technicalFeature";
        public const string CustomerTime = "deliveryTime";
        public const string Connect = "porscheConnect";
        public const string AreasToFocus = "areasToFocus";
        public const string Documents = "documents";
        public const string CarModels = "carmodels";

        #endregion

        #region Read From AppSettings.json

        public const string JwtSecurityKey = "Constants:SecurityKey";
        public const string JwtValidFor = "Constants:ValidFor";
        public const string SurveyLinkValidFor = "Constants:SurveyLinkValidFor";

        public const string IdealTime = "SectionSettings:IdealTime";
        public const string Intro = "SectionSettings:Intro";
        public const string Interior = "SectionSettings:Interior";
        public const string PCMSetup = "SectionSettings:PCMSetup";
        public const string PorscheConnect = "SectionSettings:PorscheConnect";
        public const string Exterior = "SectionSettings:Exterior";
        public const string Charging = "SectionSettings:Charging";
        public const string Administration = "SectionSettings:Administration";
        public const string Wrapup = "SectionSettings:Wrapup";

        public const string SmtpEmailFrom = "SMTPSettings:EmailFrom";
        public const string SmtpEmailTo = "SMTPSettings:EmailTo";
        public const string SmtpEnableSsl = "SMTPSettings:EnableSsl";

        public const string BucketName = "BucketName";

        #endregion

        #region Template Paths

        public const string ForgotPasswordMailTemplatePath = "template/forgot-password.html";
        public const string PreDeliveryTemplatePath = "template/pre-delivery.html";
        public const string SignupTemplatePath = "template/invite-dealer.html";
        public const string CustomerSurveyTemplatePath = "template/send-survey-email.html";
        public const string PostDeliveryTemplatePath = "template/post-delivery.html";

        #endregion

        #region Images Paths

        public const string ForgotPasswordImagePath = "images/cta.png";
        public const string LogoImagePath = "images/porsche-logo.png";
        public const string SignupImagePath = "images/invite-dealer-cta.png";
        public const string CustomerSurveyImagePath = "images/survey-cta.png";
        public const string SurveyBannerImagePath = "images/survey-banner.jpg";
        public const string KnowAppImagePath = "images/good-to-know-app.png";
        public const string PreDeliveryBannerImagePath = "images/porsche-taycan-4-s-parked.jpg";
        public const string PostDeliveryBannerImagePath = "images/post-delivery-follow-up-banner.jpg";
        public const string ConnectImagePath = "images/porsche-connect.png";

        #endregion

    }
    public static class Messages
    {
        public const string PasswordMinLengthMessage = "The {0} cannot be longer than {1} and less than {2} characters";
        public const string InvalidObject = "Invalid object";
        public const string FileNotSelected = "No file selected!";
        public const string FileNotPresent = "No file present!";
        public const string FilesUploaded = "File uploaded successfully!";
        public const string FileDeleted = "File deleted successfully";
        public const string ErrorOccurred = "An error occurred! Please try again.";
        public const string NoRecordAvailable = "No record available";
        public const string LoginSuccess = "Login successful";
        public const string InactiveAccount = "Your account is Inactive. Please contact the PPN administrator";
        public const string InactiveCentre = "Your Porsche center is Inactive. Please contact the PPN administrator";
        public const string InvalidAccessToken = "Invalid access token. Please contact the PPN administrator";
        public const string InvalidUserInfo = "User’s email should not be empty. Please contact the PPN administrator";
        public const string InvalidUserOrg = "User’s organization should not be empty. Please contact the PPN administrator";
        public const string InvalidRole = "Invalid role name. Please contact the PPN administrator";
        public const string WrongLoginPassword = "Invalid email or password";
        public const string PasswordChanged = "Password changed successfully";
        public const string PasswordReset = "Thank You! Your password has been updated.";
        public const string WrongCurrentPassword = "Invalid current password";
        public const string PasswordRequired = "Password is required";
        public const string EmailNotExist = "Alert! The Email ID is not registered";
        public const string EmailRequired = "Email ID is required";
        public const string MailSent = "Thank you! Kindly check you mailbox for a link to reset your password";
        public const string RecordFetched = "Record(s) fetched successfully";
        public const string RecordInserted = "Record added successfully";
        public const string CentreActivated = "Center activated successfully!";
        public const string CentreInActivated = "Center inactivated successfully!";
        public const string StaffDeleted = "Staff deleted successfully!";
        public const string EmailAlreadyExist = "This email already exists!";
        public const string ContactAlreadyExist = "This contact number already exists!";
        public const string DeliveryInserted = "Delivery added successfully!";
        public const string DeliveryUpdated = "Delivery updated successfully!";
        public const string DeliveryDeleted = "Delivery deleted successfully!";
        public const string InvalidEmailAddress = "Invalid email address";
        public const string InvalidContactNumber = "Invalid contact number";
        public const string WrongNewPassword = "Old and new password cannot be same!";
        public const string SurveySent = "Survey sent successfully!";
        public const string PorcheCentreAdded = "Porsche centre added successfully!";
        public const string PorscheCentreUpdated = "Porsche center updated successfully!";
        public const string StaffUpdated = "Staff updated successfully!";
        public const string StaffAdded = "Staff added successfully!";
        public const string CentreNotExist = "Your account no longer exists. Please contact the PPN administrator";
        public const string ResponseSubmitted = "Response submitted successfully!";
        public const string CustomerSurveySent = "Delivery cannot be edited/deleted as customer survey is sent out";
        public const string PendingDeliveries = "Failed to inactivate as there are pending deliveries";
        public const string TokenCreated = "Token created successfully";
        public const string InvalidToken = "Link has expired!";
        public const string LinkNotAdded = "Link is not selected!";
        public const string LinkAdded = "Link added successfully!";
        public const string LinkDeleted = "Link deleted successfully!";
        public const string LinkNotPresent = "Link not present!";
        public const string CustomerNotPresent = "Customer not present!";
        public const string SendDocumentFailed = "Failed to send documents!";
        public const string SendDocumentSucceed = "Documents sent successfully!";
        public const string DeliveryCheckListConfigSucceed = "Delivery checklist configuration is updated successfully!";
        public const string DeliveryCheckListConfigFailed = "Failed to update the delivery checklist configuration!";

        public const string CarModelAdded = "New car model added successfully!";
        public const string CarModelExists = "Car model already exists!";
        public const string CarModelUpdated = "Car model updated successfully!";
        public const string CarModelDeleted = "Car model deleted successfully!";
        public const string CarModelActiveted = "Car model activated successfully!";
        public const string CarModelInActivated = "Car model inactivated successfully!";
        public const string CarModelFailedDeleted = "Failed to inactivate car model as there are pending deliveries!";
        public const string CarModelImageNotSelected = "Car model image is not selected";
        public const string CarModelErrorOccurred = "An error occurred while uploading the car model image!";

        public const string RoleModuleAdded = "New role added successfully!";
        public const string RoleModuleExists = "Given role name already exists!";
        public const string RoleModuleUpdated = "Role updated successfully!";
        public const string RoleModuleActive = "Role activated successfully!";
        public const string RoleActiveSession = "The role cannot be inactivated as a user session is active";
        public const string RoleModuleInActive = "Role inactivated successfully!";
        public const string RoleModuleFailedDeleted = "Unable to delete role since it is assigned to user!";
        public const string RoleAssignment = "Role is not assigned to user. Please contact the PPN administrator";

        public const string UserInfoInserted = "New user information added successfully!";
        public const string UserInfoUpdated = "User information updated successfully!";
        public const string UserRoleCheck = "Assigned role is not avaialable in the PCL Delivery application. Please contact the PPN administrator ";
        public const string PPNUserAuthSuccess = "PPN user authentication successful!";
        public const string PPNUserAuthFail = "PPN user authentication failed!";
        public const string InvalidCentre = "Invalid centre name. Please contact the PPN administrator";
        public const string UserActiveted = "User activated successfully!";
        public const string UserInActivated = "User inactivated successfully!";

        public const string CentreNameExists = "Given center name already exists!";
        public const string CentrePhoneExists = "Given center contact number already exists!";


        #region Audit Log Messages

        public const string AuditLogAdded = "Audit log added successfully!";
        
        public const string ADL_CustomerAdded = "Customer '#CUSTOMER_NAME#' was created";
        public const string ADL_CustomerUpdated = "Customer '#CUSTOMER_NAME#' was updated";

        public const string ADL_DeliveryAdded = "Delivery of a Car Model '#CARMODEL#' created for Customer '#CUSTOMER_NAME#'";
        public const string ADL_DeliveryUpdated = "Delivery of a Car Model '#CARMODEL#' updated for Customer '#CUSTOMER_NAME#'";
        
        public const string ADL_DeliveryDeleted = "Delivery deleted for Customer '#CUSTOMER_NAME#'.";

        public const string ADL_CustomerSurveySent = "Survey sent to Customer '#CUSTOMER_NAME#'";
        public const string ADL_CustomerSurveyResent = "Survey resent to Customer '#CUSTOMER_NAME#'";

        public const string ADL_CustomerSurveyClicked = "Customer '#CUSTOMER_NAME#' clicked on the survey";
        public const string ADL_CustomerSurveyCompleted = "Customer '#CUSTOMER_NAME#' completed the survey";

        public const string ADL_DeliveryPreparationDetailsAdded = "Delivery preparation details added for Customer '#CUSTOMER_NAME#'";
        public const string ADL_DeliveryPreparationDetailsUpdated = "Delivery preparation details updated for Customer '#CUSTOMER_NAME#'";

        public const string ADL_InfoSheetAdded = "Info sheet details added for Customer '#CUSTOMER_NAME#'";
        public const string ADL_InfoSheetUpdated = "Info sheet details updated for Customer '#CUSTOMER_NAME#'";

        public const string ADL_PreDeliveryCheckListAdded = "Pre-delivery Checklist added for Customer '#CUSTOMER_NAME#'";
        public const string ADL_PreDeliveryCheckListUpdated = "Pre-delivery Checklist updated for Customer '#CUSTOMER_NAME#'";

        public const string ADL_DeliveryCheckListAdded = "Delivery Checklist added for Customer '#CUSTOMER_NAME#'";
        public const string ADL_DeliveryCheckListUpdated = "Delivery Checklist updated for Customer '#CUSTOMER_NAME#'";

        public const string ADL_CustomerFollowupDocsAdded = "Customer follow-up documents added for Customer '#CUSTOMER_NAME#'";
        public const string ADL_CustomerFollowupDocsUpdated = "Customer follow-up documents updated for Customer '#CUSTOMER_NAME#'";
                
        public const string ADL_DocumentsSent = "Documents sent to Customer '#CUSTOMER_NAME#'";
        public const string ADL_DocumentsResent = "Documents resent to Customer '#CUSTOMER_NAME#'";

        public const string ADL_DocumentConfigUpdate = "Document configuration is updated";

        public const string ADL_CarModelCreate = "Car model '#CarModel#' is created";
        public const string ADL_CarModelUpdate = "Car model '#CarModel#' is updated";
        public const string ADL_CarModelInactive = "Car model '#CarModel#' is inactivated";
        public const string ADL_CarModelActive = "Car model #CarModel# is activated";

        public const string ADL_PorscheCentreCreate = "Porsche center '#PorscheCentre#' is created";
        public const string ADL_PorscheCentreUpdate = "Porsche center '#PorscheCentre#' is updated";
        public const string ADL_PorscheCentreInactive = "Porsche center '#PorscheCentre#' is inactivated";
        public const string ADL_PorscheCentreActive = "Porsche center '#PorscheCentre#' is activated";

        public const string ADL_DocumentAdded = "Document '#DocName#' is added";
        public const string ADL_DocumentDeleted = "Document '#DocName#' is deleted";
        public const string ADL_LinkAdded = "Link '#LinkURL#' is added";
        public const string ADL_LinkDeleted = "Link '#LinkURL#' is deleted";

        #endregion

    }
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Dealer = "Dealer";
    }

    public static class DealerJobRoles
    {
        public const string PorschePro = "Porsche Pro";
        public const string ServiceAdvisor = "Service Advisor";
        public const string SalesConsultant = "Sales Consultant";
        public const string SalesManager = "Sales Manager";
        public const string ServiceManager = "Service Manager";
        public const string GeneralManager = "General Manager";
    }
}
