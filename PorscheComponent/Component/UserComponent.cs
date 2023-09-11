using Microsoft.Extensions.Configuration;
using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheDataAccess.Repositories;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PorscheComponent.Component
{
    public class UserComponent : IUserComponent
    {
        #region Private Variables
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public UserComponent(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }
        #endregion

        #region Methods

        #region To get centre or user details based on email
        public async Task<UserDetailViewModel> GetUserDetailByUserEmail(string userEmail)
        {
            UserDetailViewModel userDetail = new UserDetailViewModel();
            var userList = (await _unitOfWork.UserInfoRepository.GetAllAsync()).ToList();
            var userEntity = userList.FirstOrDefault(p => p.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase) && p.IsActive == true);
            if (userEntity != null)
            {
                userDetail.Id = userEntity.Id;
                userDetail.FirstName = userEntity.FirstName;
                userDetail.LastName = userEntity.LastName;
                userDetail.IsActive = userEntity.IsActive;
                userDetail.Phone = userEntity.Phone;
                userDetail.Email = userEntity.Email;
                userDetail.RoleName = userEntity.RoleName;
            }
            return userDetail;
        }

        #endregion

        #region Get User Info By Centre ID and User ID
        public async Task<UserInfoViewModel> GetUserInfoById(long centreId, long userId)
        {
            var userList = await _unitOfWork.UserInfoRepository.GetAllAsync();
            var userCentreList = await _unitOfWork.UserCentreRepository.GetAllAsync();
            var userRoleList = await _unitOfWork.UserRoleModuleRepository.GetAllAsync();
            var roleList = await _unitOfWork.RoleModuleRepository.GetAllAsync();
            var centreList = await _unitOfWork.CentreRepository.GetAllAsync();

            return (from user in userList
                    join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                    join centre in centreList on userCentre.CentreId equals centre.Id
                    join userRole in userRoleList on user.Id equals userRole.UserInfoId
                    join role in roleList on userRole.RoleModuleId equals role.Id
                    where user.Id == userId && centre.Id == centreId
                    select new UserInfoViewModel
                    {
                        Id = user.Id,
                        Name = user.FirstName + ((user.LastName != null) ? (" " + user.LastName) : string.Empty),
                        Email = user.Email,
                        Phone = user.Phone,
                        IsActive = user.IsActive,
                        //Designation = new JobRoleViewModel() { Id = role?.Id ?? 0, Name = role?.RoleName ?? String.Empty },
                        //CentreId = centre.Id,
                        RoleName = user.RoleName
                    }).FirstOrDefault();
        }

        #endregion

        #region Check whether user email is active or not
        public async Task<bool> CheckEmailExists(string email)
        {
            var userInfoList = (await _unitOfWork.UserInfoRepository.GetAllAsync()).ToList();
            
            if (userInfoList != null)
            {
                return userInfoList.Any(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && p.IsActive != false);
            }
            return false;
        }

        #endregion

        #region To check whether user account is active or not
        
        public async Task<bool> CheckAccountActive(long userId, string roleName)
        {
            var userInfoList = await _unitOfWork.UserInfoRepository.GetAllAsync();
            var roleList = await _unitOfWork.RoleModuleRepository.GetAllAsync();
            var userRoles = await _unitOfWork.UserRoleModuleRepository.GetAllAsync();
            
            if (userInfoList != null && roleList != null &&  userRoles != null) 
            {
                return (from user in userInfoList
                        join userRole in userRoles on user.Id equals userRole.UserInfoId
                        join role in roleList on userRole.RoleModuleId equals role.Id
                        where role.IsActive == true && userRole.IsActive == true && user.IsActive == true
                        select new UserInfoViewModel
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email

                        }).Any(p => p.Id == userId);

            }

            return false;
        }

        #endregion

        #region Add unique token to validate
        public async Task<int> AddToken(string token, long userId, string userRole)
        {
            UserTokenInfo userTokenInfo = new UserTokenInfo();

            var userEntity = _unitOfWork.UserInfoRepository.Find(p => p.Id == userId && p.IsActive != false);
            
            if (userEntity != null)
            {
                userTokenInfo.UserInfo = userEntity;
                userTokenInfo.Token = token;
                userTokenInfo.CreatedDate = DateTime.Now;
                userTokenInfo.ExpireDate = DateTime.Now.AddDays(Convert.ToInt32(_configuration.GetSection(Constants.JwtValidFor).Value));
                userTokenInfo.IsActive = true;
                _unitOfWork.UserTokenInfoRepository.Add(userTokenInfo);

                return await _unitOfWork.SaveChangesAsync();
            }
            return 0;
        }

        #endregion

        #region Validate token
        public async Task<bool> ValidateToken(string token, long userId, string userRole)
        {
            return true;
            
        }

        #endregion

        #region Inactivate User Info
        public async Task<int> ActiveInactivateUserInfo(long centreId, int userId, bool isActive, int loginUserId)
        {
            UserInfo userInfo = null;

            userInfo = _unitOfWork.UserInfoRepository.Find(p => p.Id == userId);

            if (userInfo != null)
            {
                userInfo.IsActive = isActive;
                userInfo.ModifiedBy = loginUserId;
                userInfo.ModifiedDate = DateTime.Now;
                _unitOfWork.UserInfoRepository.Update(userInfo);

                return await _unitOfWork.SaveChangesAsync(); 
            }


            return 0;
        }

        #endregion

        #endregion
    }

}
