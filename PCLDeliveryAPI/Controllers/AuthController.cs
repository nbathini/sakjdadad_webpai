using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLDeliveryAPI;
using PorscheComponent.Interface;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PorscheAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        #region Private Variables

        private readonly IConfiguration _configuration;
        private readonly IAdminComponent _adminComponent;
        private readonly IRoleModuleComponent _roleModuleComponent;
        private readonly ICentreComponent _centreComponent;
        private readonly ILogger<AuthController> _logger;

        #endregion

        #region Constructor
        public AuthController(IAdminComponent adminComponent, IRoleModuleComponent roleModuleComponent, ICentreComponent centreComponent, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _adminComponent = adminComponent;
            _roleModuleComponent = roleModuleComponent;
            _centreComponent = centreComponent;
            _configuration = configuration;
            _logger = logger;
        }

        #endregion

        #region API's

        #region Application Health Check API

        [HttpGet("HealthCheck")]
        [AllowAnonymous]
        public async Task<ApiResponse> HealthCheck()
        {
            ApiResponse response = null;
            try
            {
                response = new ApiResponse("Success", new { healthcheck = "We are good. Thank you." });
            }
            catch (Exception ex)
            {
                response = new ApiResponse(ex.ToString(), null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(AuthController), nameof(HealthCheck), ex));
            }
            return response;
        }

        #endregion

        #region Get PPN User Info

        [HttpGet("GetPPNUserInfo")]
        [AllowAnonymous]
        public async Task<ApiResponse> GetPPNUserInfo()
        {
            ApiResponse response = null;
            try
            {
                string res = string.Empty;
                string email = string.Empty;
                string phone = string.Empty;
                string mobile = string.Empty;
                string centreName = string.Empty;
                string displayName = string.Empty;
                string org_id = string.Empty;

                #region Read ALB (Application Load Balancer) Amazon OIDC Headers to get SSO User Details

                var accesstoken = Request.Headers["x-amzn-oidc-accesstoken"].ToString().Replace("Bearer ", "");
                var identity = Request.Headers["x-amzn-oidc-identity"].ToString().Replace("Bearer ", "");
                var data = Request.Headers["x-amzn-oidc-data"].ToString().Replace("Bearer ", "");

                var jwt_headers1 = data.Split(".")[0];
                byte[] databyte1 = Convert.FromBase64String(jwt_headers1);
                string decoded_jwt_headers1 = System.Text.Encoding.UTF8.GetString(databyte1);

                var jwt_headers2 = data.Split(".")[1];
                byte[] databyte2 = Convert.FromBase64String(jwt_headers2);
                string decoded_jwt_headers2 = System.Text.Encoding.UTF8.GetString(databyte2);

                //var OIDCDataJWTPayload = JsonConvert.DeserializeObject<OIDCDataJWTPayload>(decoded_jwt_headers2);

                var jwt_JSONObj = JObject.Parse(decoded_jwt_headers2);

                var app_roles = jwt_JSONObj["app_roles"];

                OIDCDataJWTPayload OIDCDataJWTPayload;

                if (app_roles.Type == JTokenType.String) // If app_roles node is String Type
                {
                    OIDCDataJWTPayload = new OIDCDataJWTPayload();
                    List<string> app_roles_array = new List<string>();
                    OIDCDataJWTPayload.sub = jwt_JSONObj["sub"].ToString();
                    OIDCDataJWTPayload.name = jwt_JSONObj["name"].ToString();
                    OIDCDataJWTPayload.given_name = jwt_JSONObj["given_name"].ToString();
                    OIDCDataJWTPayload.locale = jwt_JSONObj["locale"].ToString();
                    OIDCDataJWTPayload.family_name = jwt_JSONObj["family_name"].ToString();
                    app_roles_array.Add(Convert.ToString(((JValue)jwt_JSONObj["app_roles"]).Value));
                    OIDCDataJWTPayload.app_roles = app_roles_array;
                }
                else // If app_roles node is Array Type
                {
                    OIDCDataJWTPayload = JsonConvert.DeserializeObject<OIDCDataJWTPayload>(decoded_jwt_headers2);
                }

                #region Checking Account Roles which contains with "ppn_approle_pcl_"

                List<string> app_roles_list = new List<string>();

                if (OIDCDataJWTPayload.app_roles != null && OIDCDataJWTPayload.app_roles.Count > 0)
                {
                    foreach (string app_role in OIDCDataJWTPayload.app_roles)
                    {
                        if (app_role.ToLower().Contains("ppn_approle_pcl_"))
                        {
                            app_roles_list.Add(app_role);
                        }
                    }
                }

                OIDCDataJWTPayload.app_roles = app_roles_list;

                #endregion

                //Removing the default app role i.e. ppn_approle_pcl_delivery_app_application_role from the roles list if user has more than one role
                if (OIDCDataJWTPayload.app_roles != null && OIDCDataJWTPayload.app_roles.Count > 1)
                {
                    OIDCDataJWTPayload.app_roles.Remove(_configuration.GetSection("AppDefaultRole").Value);
                }

                #endregion

                #region Get Access Token Secret from AWS Secret Manager
                IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_configuration.GetSection("Region").Value));

                MasterDataSecrets mdsec_AccessToken = new MasterDataSecrets();

                GetSecretValueRequest accessTokenReq = new GetSecretValueRequest
                {
                    SecretId = _configuration.GetSection("AccessTokenSecretName").Value
                };

                GetSecretValueResponse accessTokenSecRes = client.GetSecretValueAsync(accessTokenReq).Result;

                if (accessTokenSecRes != null && accessTokenSecRes.SecretString != null)
                {
                    mdsec_AccessToken = JsonConvert.DeserializeObject<MasterDataSecrets>(accessTokenSecRes.SecretString);
                }

                #endregion

                #region Get Master Data API Access Token

                AccessToken accessToken = null;

                HttpMessageHandler handler = new HttpClientHandler();

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(mdsec_AccessToken.base_address),
                    Timeout = new TimeSpan(0, 2, 0)
                };

                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(mdsec_AccessToken.client_id + ":" + mdsec_AccessToken.secret_id);
                string val = System.Convert.ToBase64String(plainTextBytes);

                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);

                var list = new List<KeyValuePair<string, string>>() {
                                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                                    new KeyValuePair<string, string>("ppn_resource", "urn:ppn:ppn-masterdata-api")
                                };

                using (var content = new FormUrlEncodedContent(list))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                    HttpResponseMessage httpResponse_AccessToken = await httpClient.PostAsync(mdsec_AccessToken.url, content);

                    if (httpResponse_AccessToken.IsSuccessStatusCode)
                    {
                        var readTask = httpResponse_AccessToken.Content.ReadAsStringAsync().ConfigureAwait(false);
                        res = readTask.GetAwaiter().GetResult();
                        accessToken = JsonConvert.DeserializeObject<AccessToken>(res.ToString());

                    }

                }

                #endregion

                #region Get User Details including email, phone and organization

                if (accessToken != null)
                {
                    Root root = new Root();

                    #region Get User Data Secret from AWS Secret Manager

                    MasterDataSecrets mdsec_UserData = new MasterDataSecrets();

                    GetSecretValueRequest userDataReq = new GetSecretValueRequest
                    {
                        SecretId = _configuration.GetSection("UserDataSecretName").Value
                    };

                    GetSecretValueResponse userDataSecRes = client.GetSecretValueAsync(userDataReq).Result;

                    if (userDataSecRes != null && userDataSecRes.SecretString != null)
                    {
                        mdsec_UserData = JsonConvert.DeserializeObject<MasterDataSecrets>(userDataSecRes.SecretString);
                    }

                    #endregion

                    #region Get User Contact Details from Master Data API

                    var httpClientUserData = new HttpClient(handler)
                    {
                        BaseAddress = new Uri(mdsec_UserData.base_address),
                        Timeout = new TimeSpan(0, 2, 0)
                    };

                    httpClientUserData.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.access_token);
                    httpClientUserData.DefaultRequestHeaders.Add("X-Porsche-Client-Id", mdsec_UserData.client_id);
                    httpClientUserData.DefaultRequestHeaders.Add("X-Porsche-Client-Secret", mdsec_UserData.secret_id);

                    HttpResponseMessage httpResponseUserData = await httpClientUserData.GetAsync(mdsec_UserData.url.Replace(":id", OIDCDataJWTPayload.sub));

                    if (httpResponseUserData.IsSuccessStatusCode)
                    {
                        var readTask = httpResponseUserData.Content.ReadAsStringAsync().ConfigureAwait(false);
                        res = readTask.GetAwaiter().GetResult();
                        root = JsonConvert.DeserializeObject<Root>(res.ToString());
                    }

                    if (root != null && root.data != null && root.data.attributes!= null && root.data.attributes.contact != null)
                    {
                        email = root.data.attributes.mail;
                        phone = root.data.attributes.contact.phone != null ? root.data.attributes.contact.phone.ToString() : string.Empty;
                        mobile = root.data.attributes.contact.mobile != null ? root.data.attributes.contact.mobile.ToString() : string.Empty;

                        if (root.data.relationships != null && root.data.relationships.organization != null && root.data.relationships.organization.data != null)
                        {
                            org_id = root.data.relationships.organization.data.id != null ? root.data.relationships.organization.data.id.ToString() : string.Empty;
                        }
                        
                    }

                    #endregion

                    #region Get User Oraganization Details from Master Data API

                    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(org_id))
                    {
                        #region Get User Organization Data Secret from AWS Secret Manager

                        MasterDataSecrets mdsec_UserOrgData = new MasterDataSecrets();

                        GetSecretValueRequest userOrgReq = new GetSecretValueRequest
                        {
                            SecretId = _configuration.GetSection("UserOrgDataSecretName").Value
                        };

                        GetSecretValueResponse userOrgSecRes = client.GetSecretValueAsync(userOrgReq).Result;

                        if (userOrgSecRes != null && userOrgSecRes.SecretString != null)
                        {
                            mdsec_UserOrgData = JsonConvert.DeserializeObject<MasterDataSecrets>(userOrgSecRes.SecretString);
                        }

                        #endregion

                        #region Get User Oraganization Details from Master Data API

                        var httpClientUserOrgData = new HttpClient(handler)
                        {
                            BaseAddress = new Uri(mdsec_UserOrgData.base_address),
                            Timeout = new TimeSpan(0, 2, 0)
                        };

                        httpClientUserOrgData.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.access_token);
                        httpClientUserOrgData.DefaultRequestHeaders.Add("X-Porsche-Client-Id", mdsec_UserOrgData.client_id);
                        httpClientUserOrgData.DefaultRequestHeaders.Add("X-Porsche-Client-Secret", mdsec_UserOrgData.secret_id);

                        HttpResponseMessage httpResponseUserOrgData = await httpClientUserOrgData.GetAsync(mdsec_UserOrgData.url.Replace(":id", org_id));

                        if (httpResponseUserOrgData.IsSuccessStatusCode)
                        {
                            var readTask = httpResponseUserOrgData.Content.ReadAsStringAsync().ConfigureAwait(false);
                            res = readTask.GetAwaiter().GetResult();
                            root = JsonConvert.DeserializeObject<Root>(res.ToString());
                        }

                        if (root != null && root.data != null && root.data.attributes != null)
                        {
                            centreName = root.data.attributes.companyName != null ? root.data.attributes.companyName.ToString() : string.Empty;
                            displayName = root.data.attributes.displayName != null ? root.data.attributes.displayName.ToString() : string.Empty;

                            response = new ApiResponse(Messages.LoginSuccess, new { sub = OIDCDataJWTPayload.sub, name = OIDCDataJWTPayload.name, given_name = OIDCDataJWTPayload.given_name, locale = OIDCDataJWTPayload.locale, family_name = OIDCDataJWTPayload.family_name, email = email, phone = phone, mobile = mobile, centrename = centreName, displayname = displayName, app_roles = string.Join(",", OIDCDataJWTPayload.app_roles.ToArray()) });
                        }
                        else
                        {
                            response = new ApiResponse(Messages.InvalidUserOrg, null, 400);
                            response.IsError = true;
                        }

                        #endregion
                    }
                    else
                    {
                        response = new ApiResponse(Messages.InvalidUserInfo, null, 400);
                        response.IsError = true;
                    }

                    #endregion

                }
                else
                {
                    response = new ApiResponse(Messages.InvalidAccessToken, null, 400);
                    response.IsError = true;
                }

                #endregion

            }
            catch (Exception ex)
            {
                response = new ApiResponse(ex.ToString(), null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(AuthController), nameof(GetPPNUserInfo), ex));
            }

            return response;
        }

        #endregion

        #region Validate PPN User Access

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(ValidatePPNUserAccess))]
        public async Task<ApiResponse> ValidatePPNUserAccess([FromBody] UserInfoViewModel userInfoViewModel)
        {
            ApiResponse response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    string usrRole = string.Empty;
                    string email= string.Empty;

                    #region Set Default User Role (If User is having more than one role)
                    if (userInfoViewModel.RoleName.Contains(","))
                    {
                        usrRole = userInfoViewModel.RoleName.Split(',')[0];
                        userInfoViewModel.RoleName = usrRole;
                    }
                    #endregion

                    var roleModule = await _roleModuleComponent.GetRoleModuleByName(userInfoViewModel.RoleName);

                    //Check Role exists or not in PCL Delivery Application

                    if (roleModule != null && roleModule.IsActive) //If Role exists
                    {
                        CentreViewModel centreViewModel = new CentreViewModel();
                        
                        long centreId = 0;

                        if (roleModule.Modules.Id == 2) // Dealer User
                        {
                            centreViewModel = await _centreComponent.GetCentreByName(!string.IsNullOrEmpty(userInfoViewModel.CentreName) ? userInfoViewModel.CentreName : _configuration.GetSection(Constants.DefaultCentreName).Value);

                            // Check Centre/Organization is exists or not in PCL Delivery Application

                            if (centreViewModel != null)
                            {
                                if (centreViewModel.IsActive) // Centre/Organization is existed
                                {
                                    centreId = centreViewModel.Id;

                                    var userInfo = await _adminComponent.GetUserDetailByUserEmail(userInfoViewModel.Email);

                                    // Check Dealer User is existed or not in PCL Delivery Application
                                    if (userInfo != null)
                                    {
                                        if (userInfo.IsActive) // User existed and is active
                                        {
                                            if (await _adminComponent.ManageUserInfo(userInfoViewModel, roleModule) > 0)
                                            {
                                                var userId = await _adminComponent.GetUserInfoId(userInfoViewModel.Email);

                                                userInfoViewModel.Id = userId;

                                                var roleModuleID = await _adminComponent.ManageUserRoleModule(userId, roleModule.Id);

                                                if (roleModule.Modules.Id == 2)
                                                {
                                                    var userCentreID = await _adminComponent.ManageUserCentre(userId, Convert.ToInt32(centreId));
                                                }

                                                var request = string.Empty;
                                                var tokenHandler = new JwtSecurityTokenHandler();
                                                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection(Constants.JwtSecurityKey).Value));
                                                var tokenDescriptor = new SecurityTokenDescriptor
                                                {
                                                    Subject = new ClaimsIdentity(new Claim[]
                                                    {
                                                        new Claim(ClaimTypes.Name, userInfoViewModel.FirstName + Constants.EmptySpace + userInfoViewModel.LastName),
                                                        new Claim(ClaimTypes.Role, roleModule.Modules.ModuleName),
                                                        new Claim(Constants.RoleId, roleModule.Id.ToString()),
                                                        new Claim(Constants.RoleName, userInfoViewModel.RoleName),
                                                        new Claim(Constants.LoggedInUserId, userInfoViewModel.Id.ToString()),
                                                        new Claim(Constants.CentreId, centreId.ToString())
                                                    }),
                                                    Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection(Constants.JwtValidFor).Value)),
                                                    NotBefore = DateTime.UtcNow,
                                                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                                                };
                                                var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
                                                request = tokenHandler.WriteToken(token);

                                                await _adminComponent.AddToken(request, userId);

                                                response = new ApiResponse(Messages.PPNUserAuthSuccess, new { firstName = userInfoViewModel.FirstName, lastName = userInfoViewModel.LastName, email = userInfoViewModel.Email, phone = userInfoViewModel.Phone, roleInfo = new { Id = roleModule.Id, Name = roleModule.RoleName, IsCreate = roleModule.IsCreate, IsEdit = roleModule.IsEdit, IsView = roleModule.IsView, IsDelete = roleModule.IsDelete, roleGroup = roleModule.Modules.ModuleName }, accessToken = request });
                                            }
                                            else //No access to PCL Delivery Application due to User update failed
                                            {
                                                response = new ApiResponse(Messages.PPNUserAuthFail, null, 400);
                                                response.IsError = true;
                                            }
                                        }
                                        else // User existed and is Inactive (No access to PCL Delivery Application)
                                        {
                                            response = new ApiResponse(Messages.InactiveAccount, null, 400);
                                            response.IsError = true;
                                        }

                                    }
                                    else //New User insertion in PCL Delivery Application
                                    {
                                        if (await _adminComponent.ManageUserInfo(userInfoViewModel, roleModule) > 0)
                                        {
                                            var userId = await _adminComponent.GetUserInfoId(userInfoViewModel.Email);

                                            userInfoViewModel.Id = userId;

                                            var roleModuleID = await _adminComponent.ManageUserRoleModule(userId, roleModule.Id);

                                            if (roleModule.Modules.Id == 2)
                                            {
                                                var userCentreID = await _adminComponent.ManageUserCentre(userId, Convert.ToInt32(centreId));
                                            }

                                            var request = string.Empty;
                                            var tokenHandler = new JwtSecurityTokenHandler();
                                            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection(Constants.JwtSecurityKey).Value));
                                            var tokenDescriptor = new SecurityTokenDescriptor
                                            {
                                                Subject = new ClaimsIdentity(new Claim[]
                                                {
                                        new Claim(ClaimTypes.Name, userInfoViewModel.FirstName + Constants.EmptySpace + userInfoViewModel.LastName),
                                        new Claim(ClaimTypes.Role, roleModule.Modules.ModuleName),
                                        new Claim(Constants.RoleId, roleModule.Id.ToString()),
                                        new Claim(Constants.RoleName, userInfoViewModel.RoleName),
                                        new Claim(Constants.LoggedInUserId, userInfoViewModel.Id.ToString()),
                                        new Claim(Constants.CentreId, centreId.ToString())
                                                }),
                                                Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection(Constants.JwtValidFor).Value)),
                                                NotBefore = DateTime.UtcNow,
                                                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                                            };
                                            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
                                            request = tokenHandler.WriteToken(token);

                                            await _adminComponent.AddToken(request, userId);

                                            response = new ApiResponse(Messages.PPNUserAuthSuccess, new { firstName = userInfoViewModel.FirstName, lastName = userInfoViewModel.LastName, email = userInfoViewModel.Email, phone = userInfoViewModel.Phone, roleInfo = new { Id = roleModule.Id, Name = roleModule.RoleName, IsCreate = roleModule.IsCreate, IsEdit = roleModule.IsEdit, IsView = roleModule.IsView, IsDelete = roleModule.IsDelete, roleGroup = roleModule.Modules.ModuleName }, accessToken = request });
                                        }
                                        else //No access to PCL Delivery Application due to User insertion failed
                                        {
                                            response = new ApiResponse(Messages.PPNUserAuthFail, null, 400);
                                            response.IsError = true;
                                        }
                                    }
                                }
                                else // Centre/Organization is existed but is Inactivated
                                {
                                    response = new ApiResponse(Messages.InactiveCentre, null, 400);
                                    response.IsError = true;
                                }                                
                            }
                            else // Centre/Organization is NOT existed
                            {
                                response = new ApiResponse(Messages.InvalidCentre, null, 400);
                                response.IsError = true;
                            }                            
                        }
                        else  // Admin User
                        {

                            var userInfo = await _adminComponent.GetUserDetailByUserEmail(userInfoViewModel.Email);

                            // Check Admin User is existed or not in PCL Delivery Application
                            if (userInfo != null)
                            {
                                if (userInfo.IsActive) // User existed and is active
                                {
                                    if (await _adminComponent.ManageUserInfo(userInfoViewModel, roleModule) > 0)
                                    {
                                        var userId = await _adminComponent.GetUserInfoId(userInfoViewModel.Email);

                                        userInfoViewModel.Id = userId;

                                        var roleModuleID = await _adminComponent.ManageUserRoleModule(userId, roleModule.Id);

                                        if (roleModule.Modules.Id == 2)
                                        {
                                            var userCentreID = await _adminComponent.ManageUserCentre(userId, Convert.ToInt32(centreId));
                                        }

                                        var request = string.Empty;
                                        var tokenHandler = new JwtSecurityTokenHandler();
                                        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection(Constants.JwtSecurityKey).Value));
                                        var tokenDescriptor = new SecurityTokenDescriptor
                                        {
                                            Subject = new ClaimsIdentity(new Claim[]
                                            {
                                        new Claim(ClaimTypes.Name, userInfoViewModel.FirstName + Constants.EmptySpace + userInfoViewModel.LastName),
                                        new Claim(ClaimTypes.Role, roleModule.Modules.ModuleName),
                                        new Claim(Constants.RoleId, roleModule.Id.ToString()),
                                        new Claim(Constants.RoleName, userInfoViewModel.RoleName),
                                        new Claim(Constants.LoggedInUserId, userInfoViewModel.Id.ToString()),
                                        new Claim(Constants.CentreId, centreId.ToString())
                                            }),
                                            Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection(Constants.JwtValidFor).Value)),
                                            NotBefore = DateTime.UtcNow,
                                            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                                        };
                                        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
                                        request = tokenHandler.WriteToken(token);

                                        await _adminComponent.AddToken(request, userId);

                                        response = new ApiResponse(Messages.PPNUserAuthSuccess, new { firstName = userInfoViewModel.FirstName, lastName = userInfoViewModel.LastName, email = userInfoViewModel.Email, phone = userInfoViewModel.Phone, roleInfo = new { Id = roleModule.Id, Name = roleModule.RoleName, IsCreate = roleModule.IsCreate, IsEdit = roleModule.IsEdit, IsView = roleModule.IsView, IsDelete = roleModule.IsDelete, roleGroup = roleModule.Modules.ModuleName }, accessToken = request });
                                    }
                                    else //No access to PCL Delivery Application due to Admin User update failed
                                    {
                                        response = new ApiResponse(Messages.PPNUserAuthFail, null, 400);
                                        response.IsError = true;
                                    }
                                }
                                else
                                {
                                    response = new ApiResponse(Messages.InactiveAccount, null, 400);
                                    response.IsError = true;
                                }
                            }
                            else //New Admin User insertion in PCL Delivery Application
                            {
                                if (await _adminComponent.ManageUserInfo(userInfoViewModel, roleModule) > 0)
                                {
                                    var userId = await _adminComponent.GetUserInfoId(userInfoViewModel.Email);

                                    userInfoViewModel.Id = userId;

                                    var roleModuleID = await _adminComponent.ManageUserRoleModule(userId, roleModule.Id);

                                    if (roleModule.Modules.Id == 2)
                                    {
                                        var userCentreID = await _adminComponent.ManageUserCentre(userId, Convert.ToInt32(centreId));
                                    }

                                    var request = string.Empty;
                                    var tokenHandler = new JwtSecurityTokenHandler();
                                    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection(Constants.JwtSecurityKey).Value));
                                    var tokenDescriptor = new SecurityTokenDescriptor
                                    {
                                        Subject = new ClaimsIdentity(new Claim[]
                                        {
                                            new Claim(ClaimTypes.Name, userInfoViewModel.FirstName + Constants.EmptySpace + userInfoViewModel.LastName),
                                            new Claim(ClaimTypes.Role, roleModule.Modules.ModuleName),
                                            new Claim(Constants.RoleId, roleModule.Id.ToString()),
                                            new Claim(Constants.RoleName, userInfoViewModel.RoleName),
                                            new Claim(Constants.LoggedInUserId, userInfoViewModel.Id.ToString()),
                                            new Claim(Constants.CentreId, centreId.ToString())
                                        }),
                                        Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection(Constants.JwtValidFor).Value)),
                                        NotBefore = DateTime.UtcNow,
                                        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                                    };
                                    var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
                                    request = tokenHandler.WriteToken(token);

                                    await _adminComponent.AddToken(request, userId);

                                    response = new ApiResponse(Messages.PPNUserAuthSuccess, new { firstName = userInfoViewModel.FirstName, lastName = userInfoViewModel.LastName, email = userInfoViewModel.Email, phone = userInfoViewModel.Phone, roleInfo = new { Id = roleModule.Id, Name = roleModule.RoleName, IsCreate = roleModule.IsCreate, IsEdit = roleModule.IsEdit, IsView = roleModule.IsView, IsDelete = roleModule.IsDelete, roleGroup = roleModule.Modules.ModuleName }, accessToken = request });
                                }
                                else //No access to PCL Delivery Application due to User insertion failed
                                {
                                    response = new ApiResponse(Messages.PPNUserAuthFail, null, 400);
                                    response.IsError = true;
                                }
                            }

                        }                        
                    }
                    else //If Role NOT exists
                    {
                        response = new ApiResponse(Messages.InvalidRole, null, 400);
                        response.IsError = true;
                    }
                }
                else //Invalid request
                {
                    response = new ApiResponse(Messages.InvalidObject, null, 400);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(AuthController), nameof(ValidatePPNUserAccess), ex));

            }
            return response;
        }

        #endregion

        #region Login (Need to comment in PROD build)

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(Login))]
        public async Task<ApiResponse> Login([FromBody] UserLoginViewModel userViewModel)
        {
            ApiResponse response = null;
            try
            {
                if (ModelState.IsValid)
                {
                    response = await Authenticate(userViewModel);                   
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
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(AuthController), nameof(Login), ex));

            }
            return response;
        }

        #endregion

        #endregion

        #region Private Method (Need to comment in PROD build)

        private async Task<ApiResponse> Authenticate(UserLoginViewModel userViewModel)
        {
            var request = string.Empty;
            ApiResponse response = null;
            try
            {
                var user = await _adminComponent.GetUserDetailByUserEmail(userViewModel.UserEmail);

                if (user != null)
                {
                    if (user.IsActive)
                    {
                        long centreId = 0;
                        
                        var userRole = await _roleModuleComponent.GetRoleModuleByName(user.RoleName);                        

                        if (userRole != null && userRole.IsActive)
                        {
                            if (await _roleModuleComponent.GetUserRoleModuleByUserIdRoleId(user.Id, userRole.Id))
                            {
                                if (userRole.Modules.Id == 2)
                                {
                                    centreId = await _adminComponent.GetCentreId(user.Id);
                                }

                                var tokenHandler = new JwtSecurityTokenHandler();
                                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection(Constants.JwtSecurityKey).Value));
                                var tokenDescriptor = new SecurityTokenDescriptor
                                {
                                    Subject = new ClaimsIdentity(new Claim[]
                                    {
                                        new Claim(ClaimTypes.Name, user.FirstName),
                                        new Claim(ClaimTypes.Role, userRole.Modules.ModuleName),
                                        new Claim(Constants.RoleId, userRole.Id.ToString()),
                                        new Claim(Constants.RoleName, userRole.RoleName),
                                        new Claim(Constants.LoggedInUserId, user.Id.ToString()),
                                        new Claim(Constants.CentreId, centreId.ToString())
                                    }),
                                    Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection(Constants.JwtValidFor).Value)),
                                    NotBefore = DateTime.UtcNow,
                                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                                };
                                var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
                                request = tokenHandler.WriteToken(token);

                                await _adminComponent.AddToken(request, user.Id);

                                response = new ApiResponse(Messages.LoginSuccess, new { firstName = user.FirstName, lastName = user.LastName, email = user.Email, phone = user.Phone, roleInfo = new { Id = userRole.Id, Name = userRole.RoleName, IsCreate = userRole.IsCreate, IsEdit = userRole.IsEdit, IsView = userRole.IsView, IsDelete = userRole.IsDelete, roleGroup = userRole.Modules.ModuleName }, accessToken = request });
                            }
                            else
                            {
                                response = new ApiResponse(Messages.RoleAssignment, null, 400);
                                response.IsError = true;
                            }
                        }
                        else
                        {
                            response = new ApiResponse(Messages.InvalidRole, null, 400);
                            response.IsError = true;
                        }                        
                    }
                    else
                    {
                        response = new ApiResponse(Messages.InactiveAccount, null, 400);
                        response.IsError = true;
                    }
                }
                else
                {
                    response = new ApiResponse(Messages.WrongLoginPassword, null, 400);
                    response.IsError = true;
                }
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Messages.ErrorOccurred, null, 400);
                response.IsError = true;
                _logger.LogError(CommonFunctions.LogErrorMessage(nameof(AuthController), nameof(Authenticate), ex));

            }
            return response;
        }

        #endregion
        
    }
}
