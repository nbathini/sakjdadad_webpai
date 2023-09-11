using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PorscheAPI.Controllers.Admin;
using PorscheComponent.Interface;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PCLDeliveryAPI.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.Admin)]
    public class RoleModuleController : ControllerBase
    {
        #region Private Variables
        private readonly IRoleModuleComponent _roleModuleComponent;
        private readonly ILogger<RoleModuleController> _logger;
        #endregion

        #region Constructor
        public RoleModuleController(IRoleModuleComponent roleModuleComponent, ILogger<RoleModuleController> logger)
        {
            _roleModuleComponent = roleModuleComponent;            
            _logger = logger;
        }
        #endregion

        #region API's

        #region Get All Role Modules List

        [HttpGet]
        public async Task<ApiResponse> Get([FromQuery] PagingParameter pagingParameter)
        {
            ApiResponse response = null;
            try
            {
                var roleModulesList = await _roleModuleComponent.GetRoleModules(pagingParameter);

                if (roleModulesList != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, roleModulesList);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, roleModulesList);
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(RoleModuleController), nameof(Get), ex));
            }
            return response;
        }

        #endregion

        #region Get Role Module by ID

        [HttpGet("{id}")]
        public async Task<ApiResponse> Get(int id)
        {
            ApiResponse response = null;
            try
            {
                var roleModule = await _roleModuleComponent.GetRoleModuleById(id);
                if (roleModule != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, roleModule);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, roleModule);
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(RoleModuleController), nameof(Get), ex));
            }
            return response;
        }

        #endregion

        #region Add New Role Module

        [HttpPost]        
        public async Task<ApiResponse> Post(RoleModuleViewModel roleModuleViewModel)
        {
            ApiResponse response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    if (roleModuleViewModel != null)
                    {
                        RoleModuleViewModel roleModuleViewModel1 = await _roleModuleComponent.GetRoleModuleByName(roleModuleViewModel.RoleName);
                        
                        if (roleModuleViewModel1 != null)
                        {
                            response = new ApiResponse(Messages.RoleModuleExists, null, 400);
                            response.IsError = true;
                        }
                        else
                        {
                            var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                            var loginUserId = CommonFunctions.GetLoggedInUserId(bearerToken);

                            if (await _roleModuleComponent.AddEditRoleModule(roleModuleViewModel, Convert.ToInt32(loginUserId)) > 0)
                            {
                                response = new ApiResponse(Messages.RoleModuleAdded, true);

                            }
                            else
                            {
                                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
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
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(DocumentController), nameof(Post), ex));
            }
            return response;

        }

        #endregion

        #region Update Role Module

        [HttpPut]        
        public async Task<ApiResponse> Put(RoleModuleViewModel roleModuleViewModel)
        {
            ApiResponse response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    if (roleModuleViewModel != null)
                    {

                        RoleModuleViewModel roleModuleViewModel1 = await _roleModuleComponent.GetRoleModuleByIdName(roleModuleViewModel.Id, roleModuleViewModel.RoleName);

                        if (roleModuleViewModel1 != null)
                        {
                            response = new ApiResponse(Messages.RoleModuleExists, null, 400);
                            response.IsError = true;
                        }
                        else
                        {
                            var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                            var loginUserId = CommonFunctions.GetLoggedInUserId(bearerToken);

                            if (await _roleModuleComponent.AddEditRoleModule(roleModuleViewModel, Convert.ToInt32(loginUserId)) > 0)
                            {
                                response = new ApiResponse(Messages.RoleModuleUpdated, true);

                            }
                            else
                            {
                                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
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
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(RoleModuleController), nameof(Put), ex));
            }
            return response;

        }

        #endregion

        #region Active/Inactive Role Module

        [HttpDelete("{roleId}/{isActive}")]        
        public async Task<ApiResponse> Delete(int roleId, bool isActive)
        {
            ApiResponse response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    if (roleId > 0)
                    {
                        var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                        var loginUserId = CommonFunctions.GetLoggedInUserId(bearerToken);
                        var userRoleId = CommonFunctions.GetUserRoleId(bearerToken);

                        if (!string.IsNullOrEmpty(userRoleId) && Convert.ToInt32(userRoleId) == roleId && isActive == false)
                        {
                            response = new ApiResponse(Messages.RoleActiveSession, null, 400);
                            response.IsError = true;
                        }
                        else
                        {
                            if (await _roleModuleComponent.DeleteRoleModule(roleId, isActive, Convert.ToInt32(loginUserId)) > 0)
                            {
                                response = new ApiResponse(isActive ? Messages.RoleModuleActive : Messages.RoleModuleInActive, true);

                            }
                            else
                            {
                                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
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
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(RoleModuleController), nameof(Delete), ex));
            }
            return response;

        }

        #endregion

        #endregion
    }
}
