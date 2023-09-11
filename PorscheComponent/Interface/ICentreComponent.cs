using PorscheUtilities.HelperClass;
using PorscheUtilities.Models;

namespace PorscheComponent.Interface
{
    public interface ICentreComponent
    {
        Task<PagedList<CentreViewModel>> GetCentres(PagingParameter pagingParameter);
        Task<CentreViewModel> GetCentreById(long centreId);
        Task<List<StaffViewModel>> GetStaffByCentreId(long centreId);
        Task<CentreViewModel> GetCentreByName(string centreName);
        Task<int> DeleteCentre(long centreId, bool isActive, string roleName, long userId);
        Task<int> AddUpdateCentre(CentreViewModel centreViewModel, string roleName, long? userId);
        Task<bool> CheckEmailExists(string email);
        Task<bool> CheckNameExists(string name);
        Task<bool> CheckPhoneExists(string phone);
        Task<bool> CheckNameExistsEdit(long centreId, string name);        
    }
}
