using Microsoft.Extensions.Configuration;
using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheDataAccess.Repositories;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PorscheComponent.Component
{
    public class AdminComponent : IAdminComponent
    {
        #region Private Variables
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        #endregion

        #region Constructor
        public AdminComponent(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        #endregion

        #region Methods

        #region Check Assigned Role Existed Or Not
        public async Task<bool> CheckRoleExisted(string roleName)
        {
            var roleModuleList = (await _unitOfWork.RoleModuleRepository.GetAllAsync()).ToList();
            return roleModuleList.Any(p => p.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Get User Details By User Email Address

        public async Task<UserInfoViewModel> GetUserDetailByUserEmail(string userEmail)
        {
            UserInfoViewModel userDetail = new UserInfoViewModel();
            var userList = (await _unitOfWork.UserInfoRepository.GetAllAsync()).ToList();
            var userEntity = userList.FirstOrDefault(p => p.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase));
            if (userEntity != null)
            {
                userDetail.Id = userEntity.Id;
                userDetail.FirstName = userEntity.FirstName;
                userDetail.LastName = userEntity.LastName;                
                userDetail.IsActive = userEntity.IsActive;
                userDetail.Phone = userEntity.Phone;
                userDetail.Email = userEntity.Email;
                userDetail.RoleName = userEntity.RoleName;
                return userDetail;
            }
            return null;
        }

        #endregion

        #region Add/Edit User Details
        public async Task<int> ManageUserInfo(UserInfoViewModel userInfoViewModel, RoleModuleViewModel roleModule)
        {
            UserInfo userInfo = null;

            userInfo = _unitOfWork.UserInfoRepository.Find(p => p.Email.ToLower().Trim() == userInfoViewModel.Email.ToLower().Trim());

            if (userInfo != null)
            {
                userInfo.FirstName = userInfoViewModel.FirstName;
                userInfo.LastName = userInfoViewModel.LastName;
                userInfo.Email = userInfoViewModel.Email;
                userInfo.Phone = userInfoViewModel.Phone;
                userInfo.RoleName = userInfoViewModel.RoleName;
                userInfo.IsActive = true;
                userInfo.ModifiedBy = 1;
                userInfo.ModifiedDate = DateTime.Now;
                _unitOfWork.UserInfoRepository.Update(userInfo);                
                
            }
            else
            {
                userInfo = new UserInfo();
                userInfo.FirstName = userInfoViewModel.FirstName;
                userInfo.LastName = userInfoViewModel.LastName;
                userInfo.Email = userInfoViewModel.Email;
                userInfo.Phone = userInfoViewModel.Phone;
                userInfo.RoleName = userInfoViewModel.RoleName;
                userInfo.IsActive = true;
                userInfo.CreatedBy = 1;
                userInfo.CreatedDate = DateTime.Now;
                _unitOfWork.UserInfoRepository.Add(userInfo);                
                
            }

            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Add/Edit User Roles
        public async Task<int> ManageUserRoleModule(int userInfoId, int roleId)
        {
            UserRoleModule userRoleModuleEntity = _unitOfWork.UserRoleModuleRepository.Find(p => p.UserInfoId == userInfoId && p.RoleModuleId == roleId);

            if (userRoleModuleEntity == null)
            {
                UserRoleModule userRoleModule = new UserRoleModule();
                userRoleModule.UserInfoId = userInfoId;
                userRoleModule.RoleModuleId = roleId;
                userRoleModule.IsActive = true;
                userRoleModule.CreatedBy = 1;
                userRoleModule.CreatedDate = DateTime.Now;
                _unitOfWork.UserRoleModuleRepository.Add(userRoleModule);
                return await _unitOfWork.SaveChangesAsync();
            }

            return 1;
        }

        #endregion

        #region Add/Edit User Centres
        public async Task<int> ManageUserCentre(int userInfoId, int centreId)
        {
            UserCentre userCentreEntity = _unitOfWork.UserCentreRepository.Find(p => p.UserInfoId == userInfoId && p.CentreId == centreId);

            if (userCentreEntity == null)
            {
                UserCentre userCentre = new UserCentre();
                userCentre.UserInfoId = userInfoId;
                userCentre.CentreId = centreId;
                userCentre.IsActive = true;
                userCentre.CreatedBy = 1;
                userCentre.CreatedDate = DateTime.Now;
                _unitOfWork.UserCentreRepository.Add(userCentre);
                return await _unitOfWork.SaveChangesAsync();
            }

            return 1;
        }

        #endregion

        #region Get User Info ID by User Email Address
        public async Task<int> GetUserInfoId(string userEmail)
        {
            UserInfoViewModel userDetail = new UserInfoViewModel();
            var userList = (await _unitOfWork.UserInfoRepository.GetAllAsync()).ToList();
            var userEntity = userList.FirstOrDefault(p => p.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase) && p.IsActive == true);
            if (userEntity != null)
            {
                return userEntity.Id;
            }
            return 0;
        }

        #endregion

        #region Get User's Centre ID by User ID
        public async Task<int> GetCentreId(int userInfoId)
        {
            UserCentreViewModel userCentreViewModel = new UserCentreViewModel();
            var userList = (await _unitOfWork.UserCentreRepository.GetAllAsync()).ToList();
            var userEntity = userList.FirstOrDefault(p => p.UserInfoId == userInfoId && p.IsActive == true);
            if (userEntity != null)
            {
                return userEntity.CentreId;
            }
            return 0;
        }

        #endregion

        #region Check User Email existed with given email address
        public async Task<bool> CheckUserEmailExisted(string email)
        {
            var userList = (await _unitOfWork.UserInfoRepository.GetAllAsync()).ToList();
            return userList.Any(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        }

        #endregion

        #region Add unique token to validate
        public async Task<int> AddToken(string token, long userId)
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

        #region Validate Token
        public async Task<bool> ValidateToken(string token, long userId)
        {
            return await _unitOfWork.UserTokenInfoRepository.Any(p => p.UserInfo.Id == userId && p.IsActive != false && token == p.Token);
        }

        #endregion

        #endregion
    }
}

