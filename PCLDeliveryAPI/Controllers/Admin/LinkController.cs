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
    public class LinkController : ControllerBase
    {
        #region Private Variables

        private readonly ILogger<LinkController> _logger;
        private readonly ILinkComponent _linkComponent;

        #endregion

        #region Constructor
        public LinkController(ILinkComponent linkComponent, ILogger<LinkController> logger)
        {
            _linkComponent = linkComponent;
            _logger = logger;
        }

        #endregion

        #region API's

        #region Upload Links
        [HttpPost]
        public async Task<ApiResponse> Post(LinkViewModel linkViewModel)
        {
            ApiResponse response = null;
            try
            {
                if (!string.IsNullOrEmpty(linkViewModel.Link))
                {
                    long userId = CommonFunctions.GetLoggedInUserId(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));
                    string roleName = CommonFunctions.GetUserRole(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

                    if (await _linkComponent.UploadLink(linkViewModel, roleName, userId) > 0)
                    {
                        response = new ApiResponse(Messages.LinkAdded, true);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                        response.IsError = true;
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.LinkNotAdded, null, 400);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(LinkController), nameof(Post), ex));
            }
            return response;
        }

        #endregion

        #region Delete link
        [HttpDelete("{id}")]        
        public async Task<ApiResponse> Delete(long id)
        {
            ApiResponse response = null;
            try
            {
                if (id > 0)
                {
                    long userId = CommonFunctions.GetLoggedInUserId(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));
                    string roleName = CommonFunctions.GetUserRole(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

                    if (await _linkComponent.DeleteLink(id, roleName, userId) > 0)
                    {
                        response = new ApiResponse(Messages.LinkDeleted, null, 200);
                    }
                    else
                    {
                        response = new ApiResponse(Messages.LinkNotPresent, null, 400);
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

        #endregion
    }
}
