using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PorscheAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.Dealer)]
    public class DeliveryController : ControllerBase
    {
        #region Private Variables

        private readonly IDeliveryComponent _deliveryComponent;
        private readonly IUserComponent _userComponent;
        private readonly ILogger<DeliveryController> _logger;

        #endregion

        #region Constructor
        public DeliveryController(ILogger<DeliveryController> logger, IDeliveryComponent deliveryComponent, IUserComponent userComponent)
        {
            _deliveryComponent = deliveryComponent;
            _userComponent = userComponent;
            _logger = logger;
        }

        #endregion

        #region API's

        #region Get delivery details

        [HttpGet]
        public async Task<ApiResponse> Get([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            
            ApiResponse response = null;

            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);

                if (await _userComponent.CheckAccountActive(loggedInuserId, userRole))
                {
                    var centreId = CommonFunctions.GetCentreId(bearerToken);

                    if (ModelState.IsValid)
                    {

                        var deliveryList = await _deliveryComponent.GetDelivery(centreId, startDate, endDate);
                        if (deliveryList.Any())
                        {
                            response = new ApiResponse(Messages.RecordFetched, deliveryList);
                        }
                        else
                        {
                            response = new ApiResponse(Messages.NoRecordAvailable, deliveryList);
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
                    response = new ApiResponse(Messages.InactiveAccount, null, 401);
                    response.IsError = true;
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DeliveryController), nameof(Get), ex));

            }
            return response;
        }

        #endregion

        #region Add delivery details

        [HttpPost]        
        public async Task<ApiResponse> Post([FromBody] DeliveryViewModel deliveryModel)
        {
            ApiResponse response = null;

            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);

                if (await _userComponent.CheckAccountActive(loggedInuserId, userRole))
                {
                    var centreId = CommonFunctions.GetCentreId(bearerToken);

                    if (ModelState.IsValid)
                    {
                        if (await _deliveryComponent.AddDelivery(centreId, deliveryModel, userRole, loggedInuserId) > 0)
                        {
                            response = new ApiResponse(Messages.DeliveryInserted, true);
                        }
                        else
                        {
                            response = new ApiResponse(Messages.ErrorOccurred, null, 400);
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
                    response = new ApiResponse(Messages.InactiveAccount, null, 401);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DeliveryController), nameof(Post), ex));

            }
            return response;
        }

        #endregion

        #region Edit delivery details

        [HttpPut]        
        public async Task<ApiResponse> Put([FromBody] DeliveryViewModel deliveryModel)
        {
            ApiResponse response = null;

            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);

                if (await _userComponent.CheckAccountActive(loggedInuserId, userRole))
                {
                    var centreId = CommonFunctions.GetCentreId(bearerToken);
                    var userRoleId = CommonFunctions.GetUserRoleId(bearerToken);

                    if (ModelState.IsValid)
                    {
                        //deliveryModel.Model.Base64Image = string.Empty;

                        if (await _deliveryComponent.EditDelivery(centreId, deliveryModel, loggedInuserId, userRole, userRoleId) > 0)
                        {
                            response = new ApiResponse(Messages.DeliveryUpdated, true);
                        }
                        else
                        {
                            response = new ApiResponse(Messages.CustomerSurveySent, null, 400);
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
                    response = new ApiResponse(Messages.InactiveAccount, null, 401);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DeliveryController), nameof(Put), ex));

            }
            return response;
        }

        #endregion

        #region Delete delivery

        [HttpDelete("{deliveryId}")]        
        public async Task<ApiResponse> Delete(int deliveryId)
        {
            ApiResponse response = null;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);
                var centreId = CommonFunctions.GetCentreId(bearerToken);
                var userRoleId = CommonFunctions.GetUserRoleId(bearerToken);

                if (await _deliveryComponent.DeleteDelivery(centreId, deliveryId, loggedInuserId, userRole, userRoleId) > 0)
                {
                    response = new ApiResponse(Messages.DeliveryDeleted, true);
                }
                else
                {
                    response = new ApiResponse(Messages.CustomerSurveySent, null, 400);
                    response.IsError = true;
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DeliveryController), nameof(Delete), ex));

            }
            return response;
        }

        #endregion

        #region Get car model details

        [HttpGet]
        [Route(nameof(CarModel))]
        
        public async Task<ApiResponse> CarModel()
        {
            ApiResponse response = null;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);
                

                if (await _userComponent.CheckAccountActive(loggedInuserId, userRole))
                {
                    var modelList = await _deliveryComponent.GetCarModels();
                    if (modelList.Count() > 0)
                    {
                        response = new ApiResponse(Messages.RecordFetched, modelList);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.NoRecordAvailable, modelList);
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.InactiveAccount, null, 401);
                    response.IsError = true;
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DeliveryController), nameof(CarModel), ex));

            }
            return response;
        }

        #endregion

        #region Get delivery type details

        [HttpGet]        
        [Route(nameof(Type))]        
        public async Task<ApiResponse> Type()
        {
            ApiResponse response = null;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);

                if (await _userComponent.CheckAccountActive(loggedInuserId, userRole))
                {
                    var typeList = await _deliveryComponent.GetDeliveryType();
                    if (typeList.Count() > 0)
                    {
                        response = new ApiResponse(Messages.RecordFetched, typeList);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.NoRecordAvailable, typeList);
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.InactiveAccount, null, 401);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DeliveryController), nameof(Type), ex));

            }
            return response;
        }

        #endregion

        #region Get Consultant list

        [HttpGet]        
        [Route(nameof(Consultant))]
        public async Task<ApiResponse> Consultant()
        {
            ApiResponse response = null;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);

                if (await _userComponent.CheckAccountActive(loggedInuserId, userRole))
                {
                    var centreId = CommonFunctions.GetCentreId(bearerToken);

                    var consultantList = await _deliveryComponent.GetConsultantList(centreId);
                    if (consultantList.Count() > 0)
                    {
                        response = new ApiResponse(Messages.RecordFetched, consultantList);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.NoRecordAvailable, consultantList);
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.InactiveAccount, null, 401);
                    response.IsError = true;
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DeliveryController), nameof(Consultant), ex));
            }
            return response;
        }

        #endregion

        #region Get Porsche pro list

        [HttpGet]        
        [Route(nameof(PorschePro))]
        public async Task<ApiResponse> PorschePro()
        {
            ApiResponse response = null;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);

                if (await _userComponent.CheckAccountActive(loggedInuserId, userRole))
                {
                    var centreId = CommonFunctions.GetCentreId(bearerToken);

                    var proList = await _deliveryComponent.GetPorscheProList(centreId);
                    if (proList.Count() > 0)
                    {
                        response = new ApiResponse(Messages.RecordFetched, proList);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.NoRecordAvailable, proList);
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.InactiveAccount, null, 401);
                    response.IsError = true;
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DeliveryController), nameof(PorschePro), ex));
            }
            return response;
        }

        #endregion

        #region Get Advisor list

        [HttpGet]        
        [Route(nameof(Advisor))]
        public async Task<ApiResponse> Advisor()
        {
            ApiResponse response = null;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);

                if (await _userComponent.CheckAccountActive(loggedInuserId, userRole))
                {
                    var centreId = CommonFunctions.GetCentreId(bearerToken);

                    var advisorList = await _deliveryComponent.GetAdvisorList(centreId);
                    if (advisorList.Count() > 0)
                    {
                        response = new ApiResponse(Messages.RecordFetched, advisorList);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.NoRecordAvailable, advisorList);
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.InactiveAccount, null, 401);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DeliveryController), nameof(Advisor), ex));
            }
            return response;
        }

        #endregion

        #region Get delivery details by id

        [HttpGet("{deliveryId}")]
        public async Task<ApiResponse> Get(int deliveryId)
        {
            ApiResponse response = null;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                long loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var userRole = CommonFunctions.GetUserRole(bearerToken);

                if (await _userComponent.CheckAccountActive(loggedInuserId, userRole))
                {
                    var centreId = CommonFunctions.GetCentreId(bearerToken);

                    var delivery = await _deliveryComponent.GetDeliveryById(centreId, deliveryId);

                    if (delivery != null)
                    {
                        //delivery.Model.Base64Image = await _deliveryComponent.ImageUrlToBase64(delivery.Model.ImagePath);

                        response = new ApiResponse(Messages.RecordFetched, delivery);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.NoRecordAvailable, delivery, 400);
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.InactiveAccount, null, 401);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DeliveryController), nameof(Get), ex));
            }
            return response;
        }

        #endregion


        #endregion       

    }
}
