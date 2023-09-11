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
    public class CentreController : ControllerBase
    {
        #region Private Variables

        private readonly ICentreComponent _centreComponent;
        private readonly ILogger<CentreController> _logger;

        #endregion

        #region Constructor
        public CentreController(ICentreComponent centreComponent, ILogger<CentreController> logger)
        {
            _centreComponent = centreComponent;
            _logger = logger;
        }

        #endregion

        #region API's

        #region Get porsche centres list

        [HttpGet]        
        public async Task<ApiResponse> Get([FromQuery] PagingParameter pagingParameter)
        {
            ApiResponse response = null;
            try
            {
                var centreList = await _centreComponent.GetCentres(pagingParameter);
                if (centreList.Count() > 0)
                {
                    response = new ApiResponse(Messages.RecordFetched, centreList);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, centreList);
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CentreController), nameof(Get), ex));
            }
            return response;
        }

        #endregion

        #region Get porsche centre details by id

        [HttpGet("{id}")]        
        public async Task<ApiResponse> Get(long id)
        {
            ApiResponse response = null;
            try
            {
                var porscheCentre = await _centreComponent.GetCentreById(id);
                if (porscheCentre != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, porscheCentre);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, porscheCentre);
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CentreController), nameof(Get), ex));
            }
            return response;
        }

        #endregion

        #region Get staff details by porsche centre id

        [HttpGet("GetStaffByCentreId/{centreId}")]        
        public async Task<ApiResponse> GetStaffByCentreId(long centreId)
        {
            ApiResponse response = null;
            try
            {
                var porscheCentre = await _centreComponent.GetStaffByCentreId(centreId);
                if (porscheCentre != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, porscheCentre);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, porscheCentre);
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CentreController), nameof(GetStaffByCentreId), ex));
            }
            return response;
        }

        #endregion

        #region Add porsche centre

        [HttpPost]
        public async Task<ApiResponse> Post([FromBody] CentreViewModel centreViewModel)
        {
            ApiResponse response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    if (!await _centreComponent.CheckEmailExists(centreViewModel.Email))
                    {
                        if (!await _centreComponent.CheckNameExists(centreViewModel.Name))
                        {
                            if (!await _centreComponent.CheckPhoneExists(centreViewModel.ContactNumber))
                            {
                                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                                var loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                                var roleName = CommonFunctions.GetUserRole(bearerToken);

                                if (await _centreComponent.AddUpdateCentre(centreViewModel, roleName, loggedInuserId) > 0)
                                {
                                    response = new ApiResponse(Messages.PorcheCentreAdded, true);                                    
                                }
                                else
                                {
                                    response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                                    response.IsError = true;
                                }
                            }
                            else
                            {
                                response = new ApiResponse(Messages.CentrePhoneExists, null, 400);
                                response.IsError = true;
                            }
                        }
                        else
                        {
                            response = new ApiResponse(Messages.CentreNameExists, null, 400);
                            response.IsError = true;
                        }
                    }
                    else
                    {
                        response = new ApiResponse(Messages.EmailAlreadyExist, null, 400);
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
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CentreController), nameof(Post), ex));
            }
            return response;
        }

        #endregion

        #region Update porsche centre

        [HttpPut]
        public async Task<ApiResponse> Put([FromBody] CentreViewModel centreViewModel)
        {
            ApiResponse response = null;
            try
            {
                if (ModelState.IsValid)
                {
                    var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                    var loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                    var roleName = CommonFunctions.GetUserRole(bearerToken);

                    if (!await _centreComponent.CheckNameExistsEdit(centreViewModel.Id, centreViewModel.Name))
                    {
                        if (await _centreComponent.AddUpdateCentre(centreViewModel, roleName, loggedInuserId) > 0)
                        {
                            response = new ApiResponse(Messages.PorscheCentreUpdated, true);
                        }
                        else
                        {
                            response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                            response.IsError = true;
                        }
                    }
                    else
                    {
                        response = new ApiResponse(Messages.CentreNameExists, null, 400);
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
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CentreController), nameof(Put), ex));

            }
            return response;
        }

        #endregion

        #region Deactivate porche centre

        [HttpDelete("{id}/{isActive}")]        
        public async Task<ApiResponse> Delete(long id, bool isActive)
        {
            ApiResponse response = null;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                var loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var roleName = CommonFunctions.GetUserRole(bearerToken);

                if (await _centreComponent.DeleteCentre(id, isActive, roleName, loggedInuserId) > 0)
                {
                    if (isActive)
                    {
                        response = new ApiResponse(Messages.CentreActivated, true);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.CentreInActivated, true);
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.PendingDeliveries, null, 400);
                    response.IsError = true;
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(CentreController), nameof(Delete), ex));

            }
            return response;
        }

        #endregion

        #endregion
        
    }
}
