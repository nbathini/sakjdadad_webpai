using PorscheUtilities.HelperClass;
using PorscheUtilities.Models;

namespace PorscheComponent.Interface
{
    public interface IRoleModuleComponent
    {
        Task<PagedList<RoleModuleViewModel>> GetRoleModules(PagingParameter pagingParameter);
        Task<RoleModuleViewModel> GetRoleModuleById(int roleId);
        Task<RoleModuleViewModel> GetRoleModuleByName(string roleName);
        Task<RoleModuleViewModel> GetRoleModuleByIdName(int roleId, string romeName);
        Task<int> DeleteRoleModule(int roleId, bool isActive, int userId);
        Task<int> AddEditRoleModule(RoleModuleViewModel roleModuleViewModel, int loginUserId);
        Task<bool> GetUserRoleModuleByUserIdRoleId(int userId, int roleId);
    }
}
