using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PorscheComponent.Interface;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PorscheAPI.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.Admin)]
    public class DocumentController : ControllerBase
    {
        #region Private Variables

        private readonly ILogger<DocumentController> _logger;
        private readonly IDocumentComponent _documentComponent;
        private readonly IConfiguration _configuration;

        #endregion

        #region Constructor
        public DocumentController(IDocumentComponent documentComponent, IConfiguration configuration, ILogger<DocumentController> logger)
        {
            _documentComponent = documentComponent;            
            _configuration = configuration;
            _logger = logger;
        }

        #endregion

        #region API's

        #region Upload documents

        [HttpPost, DisableRequestSizeLimit]        
        public async Task<ApiResponse> Post(List<IFormFile> files, string subPath)
        {
            ApiResponse response = null;
            try
            {
                if (files.Count() > 0)
                {
                    var result = await UploadFiles(files, subPath);
                    if (result.Count() > 0)
                    {
                        var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                        long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                        var centreId = CommonFunctions.GetCentreId(bearerToken);
                        var userRole = CommonFunctions.GetUserRole(bearerToken);

                        if (await _documentComponent.UploadDocuments(result, loggedInuserId, centreId, userRole) > 0)
                            response = new ApiResponse(Messages.FilesUploaded, result);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                        response.IsError = true;
                    }

                }
                else
                {
                    response = new ApiResponse(Messages.FileNotSelected, null, 400);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DocumentController), nameof(Post), ex));
            }
            return response;
        }

        #endregion

        #region Download documents

        [HttpGet("{id}")]        
        public async Task<ApiResponse> Get(long id)
        {
            ApiResponse response = null;
            try
            {
                if (id > 0)
                {
                    var path = await _documentComponent.GetFilePath(id);

                    if (!string.IsNullOrEmpty(path))
                    {
                        response = new ApiResponse(Messages.RecordFetched, path);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.FileNotPresent, null, 400)
                        {
                            IsError = true
                        };
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.FileNotSelected, null, 400);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DocumentController), nameof(Get), ex));
            }
            return response;
        }

        #endregion

        #region Delete documents

        [HttpDelete("{id}")]        
        public async Task<ApiResponse> Delete(long id)
        {
            ApiResponse response = null;
            try
            {
                if (id > 0)
                {
                    var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                    long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                    var centreId = CommonFunctions.GetCentreId(bearerToken);
                    var userRole = CommonFunctions.GetUserRole(bearerToken);

                    if (await _documentComponent.DeleteDocument(id, loggedInuserId, centreId, userRole) > 0)
                    {
                        response = new ApiResponse(Messages.FileDeleted, null, 200);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.FileNotPresent, null, 400);
                        response.IsError = true;
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.InvalidObject, null, 400);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DocumentController), nameof(Delete), ex));
            }
            return response;
        }

        #endregion

        #region Get master delivery checklist json

        [HttpGet]
        [Route(nameof(CheckList))]        
        public async Task<ApiResponse> CheckList()
        {
            ApiResponse response = null;
            try
            {
                var questionJson = await _documentComponent.GetCheckListJson();
                if (!string.IsNullOrEmpty(questionJson))
                {
                    response = new ApiResponse(Messages.RecordFetched, questionJson);
                }
                else
                {
                    response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DocumentController), nameof(CheckList), ex));
            }
            return response;
        }

        #endregion

        #region Manage Delivery Check List
        
        [HttpPost]
        [Route(nameof(ManageCheckList))]        
        public async Task<ApiResponse> ManageCheckList(List<DeliveryCheckListJsonModel> deliveryCheckListJsonModelList)
        {
            ApiResponse response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    if (deliveryCheckListJsonModelList != null)
                    {
                        var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                        long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                        var centreId = CommonFunctions.GetCentreId(bearerToken);
                        var userRole = CommonFunctions.GetUserRole(bearerToken);

                        if (await _documentComponent.ManageCheckList(deliveryCheckListJsonModelList, loggedInuserId, userRole) > 0)
                        {
                            response = new ApiResponse(Messages.DeliveryCheckListConfigSucceed, true);
                        }
                        else
                        {
                            response = new ApiResponse(Messages.DeliveryCheckListConfigFailed, null, 400);
                            response.IsError = true;
                        }
                    }
                    else
                    {
                        response = new ApiResponse(Messages.InvalidObject, null, 400);
                        response.IsError = true;
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.InvalidObject, null, 400);
                    response.IsError = true;
                }
            }
            catch(Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DocumentController), nameof(ManageCheckList), ex));
            }

            return response;
        }
        #endregion

        #endregion

        #region Private Methods

        #region Upload files in S3 Bucket
        private async Task<List<Document>> UploadFiles(List<IFormFile> postedFiles, string subPath)
        {
            List<Document> fileUploadResults = new List<Document>();

            foreach (IFormFile postedFile in postedFiles)
            {
                #region S3 Upload

                string bucketName = _configuration.GetSection(Constants.BucketName).Value;

                string fileName = Path.GetFileName(postedFile.FileName);

                fileName = Path.GetFileNameWithoutExtension(postedFile.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(postedFile.FileName);

                var isUploaded = await _documentComponent.UploadFileToS3(postedFile, bucketName, Constants.Documents, fileName);

                if (isUploaded)
                {
                    _logger.LogInformation($"File: {fileName} uploaded successfully");
                    fileUploadResults.Add(new Document() { FileName = fileName, FileFullPath = _configuration.GetSection(Constants.CloudFrontURL).Value + Constants.Documents + @"/" + fileName, FileType = postedFile.ContentType, FileSubPath = subPath, FileSize = postedFile.Length, IsSent = false });
                }
                else
                {
                    _logger.LogInformation($"File: {fileName} not uploaded");
                }

                #endregion
            }
            return fileUploadResults;
        }

        #endregion

        #endregion


    }
}
