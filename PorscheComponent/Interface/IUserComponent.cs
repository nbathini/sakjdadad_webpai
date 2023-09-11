using PorscheUtilities.Models;

namespace PorscheComponent.Interface
{
    public interface IUserComponent
    {
        Task<UserDetailViewModel> GetUserDetailByUserEmail(string userEmail);
        Task<bool> CheckEmailExists(string email);
        Task<bool> CheckAccountActive(long userId, string roleName);
        Task<int> AddToken(string token, long userId, string userRole);
        Task<bool> ValidateToken(string token, long userId, string userRole);
        Task<int> ActiveInactivateUserInfo(long centreId, int userId, bool isActive, int loginUserId);
        Task<UserInfoViewModel> GetUserInfoById(long centreId, long userId);
    }
}
