using PorscheUtilities.Models;

namespace PorscheComponent.Interface
{
    public interface ICustomerComponent
    {
        Task<CustomerViewModel> GetCustomerDetail(long id);
    }
}
