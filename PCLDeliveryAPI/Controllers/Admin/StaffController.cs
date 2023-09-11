using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PorscheComponent.Interface;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PCLDeliveryAPI.Controllers.Admin
{
    [Route("api/admin/centre/{centreId}/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.Admin)]
    public class StaffController : ControllerBase
    {
        #region Private Variables
        private readonly ICentreComponent _centreComponent;
        private readonly IUserComponent _userComponent;
        private readonly ILogger<StaffController> _logger;
        #endregion

        #region Constructor
        public StaffController(ICentreComponent centreComponent, IUserComponent userComponent, ILogger<StaffController> logger)
        {
            _centreComponent = centreComponent;
            _userComponent  = userComponent;
            _logger = logger;
        }
        #endregion

        #region API's

        #region Get porsche centre staff details by centre id

        [HttpGet]
        public async Task<ApiResponse> Get(long centreId, [FromQuery] PagingParameter pagingParameter)
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
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(StaffController), nameof(Get), ex));
            }
            return response;
        }

        #endregion

        #region Active/Inactive User Info

        [HttpDelete("{id}/{isActive}")]        
        public async Task<ApiResponse> Delete(long centreId, long id, bool isActive)
        {
            ApiResponse response = null;
            try
            {
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                var loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                var roleName = CommonFunctions.GetUserRole(bearerToken);

                if (await _userComponent.ActiveInactivateUserInfo(centreId, Convert.ToInt32(id), isActive, Convert.ToInt32(loggedInuserId)) > 0)
                {
                    if (isActive)
                    {
                        response = new ApiResponse(Messages.UserActiveted, true);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.UserInActivated, true);
                    }                    
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
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(StaffController), nameof(Delete), ex));

            }
            return response;
        }

        #endregion

        #endregion
    }
}
