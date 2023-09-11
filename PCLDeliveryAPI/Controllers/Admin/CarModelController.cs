using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using PorscheComponent.Interface;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PorscheAPI.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.Admin)]
    public class CarModelController : ControllerBase
    {
        #region Private Variables

        private readonly ICarModelComponent _carModelComponent;        
        private readonly IConfiguration _configuration;
        private readonly ILogger<CarModelController> _logger;

        #endregion

        #region Constructor
        public CarModelController(ICarModelComponent carModelComponent, IConfiguration configuration, ILogger<CarModelController> logger)
        {
            _carModelComponent = carModelComponent;            
            _configuration = configuration;
            _logger = logger;

        }

        #endregion

        #region API's

        #region Get All Car Models List

        [HttpGet]
        public async Task<ApiResponse> Get([FromQuery] PagingParameter pagingParameter)
        {
            ApiResponse response = null;
            try
            {
                var carModelsList = await _carModelComponent.GetCarModels(pagingParameter);
                if (carModelsList != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, carModelsList);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, carModelsList);
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CarModelController), nameof(Get), ex));
            }
            return response;
        }

        #endregion

        #region Get Car Model by ID

        [HttpGet("{id}")]     
        public async Task<ApiResponse> Get(long id)
        {
            ApiResponse response = null;
            try
            {
                var carModel = await _carModelComponent.GetCarModelById(id);
                if (carModel != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, carModel);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, carModel);
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CarModelController), nameof(Get), ex));
            }
            return response;
        }

        #endregion

        #region Add New Car Model

        [HttpPost]
        public async Task<ApiResponse> Post(List<IFormFile> files, int Id, string carModelName, string? carImagePath, string? carModelDesc)
        {
            ApiResponse response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    var carModel = await _carModelComponent.GetCarModelByName(carModelName);

                    if (carModel != null)
                    {
                        response = new ApiResponse(Messages.CarModelExists, null, 200);
                        response.IsError = true;
                    }
                    else
                    {
                        CarViewModel carViewModel = new CarViewModel();
                        carViewModel.Id = Id;
                        carViewModel.Name = carModelName;
                        carViewModel.Description = carModelDesc;

                        if (files != null & files.Count() > 0)
                        {
                            var result = await UploadCarModelImages(files, carModelName);

                            if (result.Count() > 0)
                            {
                                carViewModel.ImagePath = result[0].ImageFullPath;

                                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                                var loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                                var roleName = CommonFunctions.GetUserRole(bearerToken);

                                if (await _carModelComponent.AddEditCarModel(carViewModel, roleName, loggedInuserId) > 0)
                                {
                                    response = new ApiResponse(Messages.CarModelAdded, true);

                                }
                                else
                                {
                                    response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                                    response.IsError = true;
                                }
                            }
                            else
                            {
                                response = new ApiResponse(Messages.CarModelErrorOccurred, null, 400);
                                response.IsError = true;
                            }

                        }
                        else
                        {
                            response = new ApiResponse(Messages.CarModelImageNotSelected, null, 400);
                            response.IsError = true;
                        }
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
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CarModelController), nameof(Post), ex));
            }
            return response;

        }

        #endregion

        #region Update Car Model

        [HttpPut]
        public async Task<ApiResponse> Put(List<IFormFile> files, int Id, string carModelName, string? carImagePath, string? carModelDesc)
        {
            ApiResponse response = null;
            try
            {
                if (ModelState.IsValid)
                {
                    var carModel = await _carModelComponent.GetCarModelByIdName(Id, carModelName);

                    if (carModel != null)
                    {
                        response = new ApiResponse(Messages.CarModelExists, null, 200);
                        response.IsError = true;
                    }
                    else
                    {
                        carModel = await _carModelComponent.GetCarModelById(Id);

                        if (carModel != null)
                        {
                            CarViewModel carViewModel = new CarViewModel();
                            carViewModel.Id = Id;
                            carViewModel.Name = carModelName;
                            carViewModel.Description = carModelDesc;
                            carViewModel.ImagePath = carModel.ImagePath;

                            if (files != null & files.Count() > 0)
                            {
                                var result = await UploadCarModelImages(files, carModelName);

                                if (result != null && result.Count() > 0)
                                {
                                    carViewModel.ImagePath = result[0].ImageFullPath;

                                    var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                                    var loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                                    var roleName = CommonFunctions.GetUserRole(bearerToken);

                                    if (await _carModelComponent.AddEditCarModel(carViewModel, roleName, loggedInuserId) > 0)
                                    {
                                        response = new ApiResponse(Messages.CarModelUpdated, true);
                                    }
                                    else
                                    {
                                        response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                                        response.IsError = true;
                                    }
                                }
                                else
                                {
                                    response = new ApiResponse(Messages.CarModelErrorOccurred, null, 400);
                                    response.IsError = true;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(carImagePath))
                                {
                                    var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                                    var loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                                    var roleName = CommonFunctions.GetUserRole(bearerToken);

                                    if (await _carModelComponent.AddEditCarModel(carViewModel, roleName, loggedInuserId) > 0)
                                    {
                                        response = new ApiResponse(Messages.CarModelUpdated, true);
                                    }
                                    else
                                    {
                                        response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                                        response.IsError = true;
                                    }
                                }
                                else
                                {
                                    response = new ApiResponse(Messages.CarModelImageNotSelected, null, 400);
                                    response.IsError = true;
                                }
                            }
                        }
                        else
                        {
                            response = new ApiResponse(Messages.InvalidObject, null, 400);
                            response.IsError = true;
                        }
                    }
                    
                }
                else
                {
                    response = new ApiResponse(Messages.InvalidObject, null, 400);
                    response.IsError = true;
                }
            }
            catch (DbUpdateException ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CentreController), nameof(Put), ex));
                _logger.LogError(ex.InnerException?.Message);
                _logger.LogError(ex.InnerException?.StackTrace);
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CentreController), nameof(Put), ex));
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);

            }
            return response;
        }

        #endregion

        #region Delete Car Model

        [HttpDelete("{id}/{isActive}")]
        public async Task<ApiResponse> Delete(int id, bool isActive)
        {
            ApiResponse response = null;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                var loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var roleName = CommonFunctions.GetUserRole(bearerToken);

                if (await _carModelComponent.DeleteCarModel(id, isActive, roleName, loggedInuserId) > 0)
                {
                    if (isActive)
                    {
                        response = new ApiResponse(Messages.CarModelActiveted, true);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.CarModelInActivated, true);
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.CarModelFailedDeleted, null, 400);
                    response.IsError = true;
                }

            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("FK_Delivery_CarModel_ModelId"))
                {
                    response = new ApiResponse(Messages.CarModelFailedDeleted, null, 400);
                    response.IsError = true;
                }
                else
                {
                    response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                    response.IsError = true;
                    _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CentreController), nameof(Delete), ex));
                }
                

            }
            return response;
        }

        #endregion

        #endregion

        #region Private Methods

        #region Upload Car Model Images

        private async Task<List<CarImage>> UploadCarModelImages(List<IFormFile> postedFiles, string subPath)
        {
            List<CarImage> fileUploadResults = new List<CarImage>();

            foreach (IFormFile postedFile in postedFiles)
            {
                #region S3 Upload

                string bucketName = _configuration.GetSection(Constants.BucketName).Value;

                string fileName = Path.GetFileName(postedFile.FileName);
                
                fileName = Path.GetFileNameWithoutExtension(postedFile.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(postedFile.FileName);

                var isUploaded = await _carModelComponent.UploadFileToS3(postedFile, bucketName, Constants.CarModels, fileName);

                if (isUploaded)
                {
                    _logger.LogInformation($"File: {fileName} uploaded successfully");
                    fileUploadResults.Add(new CarImage() { ImageName = fileName, ImageFullPath = _configuration.GetSection(Constants.CloudFrontURL).Value + Constants.CarModels + @"/" + fileName, ImageSubPath = subPath, ImageSize = postedFile.Length });
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
