using PorscheComponent.Interface;
using PorscheDataAccess.Repositories;
using PorscheUtilities.Models;

namespace PorscheComponent.Component
{
    public class CustomerComponent : ICustomerComponent
    {
        #region Private Variables

        private readonly IUnitOfWork _unitOfWork;

        #endregion

        #region Constructor
        public CustomerComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #endregion

        #region Methods

        #region Get Customer Details By Custloner ID
        public async Task<CustomerViewModel> GetCustomerDetail(long id)
        {
            var customer = await _unitOfWork.CustomerRepository.FirstOrDefault(x => x.Id == id);
            CustomerViewModel customerViewModel = new CustomerViewModel();
            if (customer != null)
            {
                customerViewModel.Id = customer.Id;
                customerViewModel.Name = customer.Name;
                customerViewModel.Email = customer.Email;
                customerViewModel.ContactNumber = customer.ContactNumber;
            }
            return customerViewModel;
        }

        #endregion

        #endregion
    }
}
