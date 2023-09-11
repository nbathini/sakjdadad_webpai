using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PorscheComponent.Interface;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PCLDeliveryAPI.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.Admin)]
    public class AuditLogController : ControllerBase
    {
        #region Private Variables

        private readonly IAuditLogComponent _auditLogComponent;
        private readonly ILogger<AuditLogController> _logger;

        #endregion

        #region Constructor
        public AuditLogController(IAuditLogComponent auditLogComponent, ILogger<AuditLogController> logger)
        {
            _auditLogComponent = auditLogComponent;
            _logger = logger;
        }
        #endregion

        #region API's

        #region Get All Audit Logs

        [HttpGet]
        public async Task<ApiResponse> Get([FromQuery] PagingParameter pagingParameter)
        {
            ApiResponse response = null;
            try
            {
                var auditLogList = await _auditLogComponent.GetAuditLog(pagingParameter);
                if (auditLogList != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, auditLogList);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, auditLogList);
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(AuditLogController), nameof(Get), ex));
            }
            return response;
        }

        [HttpGet("GetAll")]        
        public async Task<ApiResponse> GetAll()
        {
            ApiResponse response = null;
            try
            {
                var auditLogList = await _auditLogComponent.GetAllAuditLog();
                if (auditLogList != null)
                {
                    response = new ApiResponse(Messages.RecordFetched, auditLogList);
                }
                else
                {
                    response = new ApiResponse(Messages.NoRecordAvailable, auditLogList);
                }

            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(AuditLogController), nameof(GetAll), ex));
            }
            return response;
        }

        #endregion

        #region Add Audit Log

        [HttpPost]        
        public async Task<ApiResponse> Post(AuditLogViewModel auditLogViewModel)
        {
            ApiResponse response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    #region Add Audit Log
                    
                    var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                    var loggedInuserId = CommonFunctions.GetLoggedInUserId(bearerToken);

                    AuditLogViewModel auditLogView = new AuditLogViewModel();

                    auditLogView.EventName = auditLogViewModel.EventName;
                    auditLogView.CentreId = auditLogViewModel.CentreId;
                    auditLogView.ModuleName = AppRoles.Dealer;

                    if (await _auditLogComponent.ManageAuditLog(auditLogView, loggedInuserId) > 0)
                    {
                        response = new ApiResponse(Messages.AuditLogAdded, true);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                        response.IsError = true;
                    }

                    #endregion
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
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(AuditLogController), nameof(Post), ex));
            }
            return response;

        }

        #endregion

        #endregion
    }
}
