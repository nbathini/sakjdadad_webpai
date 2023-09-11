using PorscheUtilities.Models;

namespace PorscheComponent.Interface
{
    public interface IAdminComponent
    {
        Task<int> AddToken(string token, long userId);
        Task<bool> ValidateToken(string token, long userId);
        Task<bool> CheckRoleExisted(string roleName);
        Task<UserInfoViewModel> GetUserDetailByUserEmail(string userEmail);
        Task<int> ManageUserInfo(UserInfoViewModel userInfoViewModel, RoleModuleViewModel roleModule);
        Task<int> ManageUserRoleModule(int userInfoId, int roleId);
        Task<int> ManageUserCentre(int userInfoId, int centreId);
        Task<int> GetUserInfoId(string userEmail);
        Task<int> GetCentreId(int userInfoId);
        Task<bool> CheckUserEmailExisted(string email);
        
    }
}
