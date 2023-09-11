using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheDataAccess.Repositories;
using PorscheUtilities.HelperClass;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PorscheComponent.Component
{
    public class CentreComponent : ICentreComponent
    {
        #region Private Variables
        private readonly IUnitOfWork _unitOfWork;        
        private readonly IUserComponent _userComponent;
        #endregion

        #region Constructor
        public CentreComponent(IUnitOfWork unitOfWork, IUserComponent userComponent)
        {
            _unitOfWork = unitOfWork;
            _userComponent = userComponent;
        }

        #endregion

        #region Methods

        #region Fetch all the Centres list
        public async Task<PagedList<CentreViewModel>> GetCentres(PagingParameter pagingParameter)
        {
            var centreList = await _unitOfWork.CentreRepository.GetAllAsync();
            var staffList = await _unitOfWork.UserInfoRepository.GetAllAsync();
            var userCentreList = await _unitOfWork.UserCentreRepository.GetAllAsync();
            var centreStaffList = (from centre in centreList
                                   let cCount =
                                   (

                                   from userCentre in userCentreList
                                   where centre.Id == userCentre.CentreId //&& userCentre.IsActive != false
                                   select userCentre
                                   ).Count()
                                   select new
                                   {
                                       CentreId = centre.Id,
                                       StaffCount = cCount
                                   }).ToList();
            var centreCompleteList = (from centre in centreList
                                      join centreStaff in centreStaffList on centre.Id equals centreStaff.CentreId into cs
                                      from cntrStaff in cs.DefaultIfEmpty()
                                      //where centre.IsActive != false
                                      orderby centre.Id descending
                                      select new CentreViewModel
                                      {
                                          Id = centre.Id,
                                          Name = centre.Name,
                                          Email = centre.Email,
                                          ContactNumber = centre.Phone,
                                          IsActive = centre.IsActive,
                                          Staff = cntrStaff.StaffCount
                                      }).OrderByDescending(x => x.Id).ToList();

            if (!string.IsNullOrWhiteSpace(pagingParameter.Search))
            {
                SearchRecords(ref centreCompleteList, pagingParameter.Search);
            }
            return PagedList<CentreViewModel>.ToPagedList(centreCompleteList, pagingParameter.PageNumber, pagingParameter.PageSize);
        }

        #endregion

        #region Fetch Centre by Centre Id
        public async Task<CentreViewModel> GetCentreById(long centreId)
        {
            CentreViewModel centreViewModel = null;
            var centre = await _unitOfWork.CentreRepository.FirstOrDefault(p => p.Id == centreId);
            if (centre != null)
            {
                centreViewModel = new CentreViewModel();
                centreViewModel.Id = centre.Id;
                centreViewModel.Name = centre.Name;
                centreViewModel.Email = centre.Email;
                centreViewModel.ContactNumber = centre.Phone;
                centreViewModel.IsActive = centre.IsActive;
            }
            return centreViewModel;
        }
        #endregion

        #region Fetch Staff By Centre Id
        public async Task<List<StaffViewModel>> GetStaffByCentreId(long centreId)
        {
            var centreList = await _unitOfWork.CentreRepository.GetAllAsync();
            var usersList = await _unitOfWork.UserInfoRepository.GetAllAsync();
            var userCentreList = await _unitOfWork.UserCentreRepository.GetAllAsync();

            var staffList = (from user in usersList
                             join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                             where userCentre.CentreId == centreId //&& userCentre.IsActive == true && user.IsActive == true
                             select new StaffViewModel
                             {
                                 Id = user.Id,
                                 Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty),
                                 Designation = user.RoleName,
                                 Email = user.Email,
                                 ContactNumber = user.Phone,
                                 IsActive = user.IsActive
                             }).ToList();
            
            return staffList;

        }

        #endregion

        #region Fetch Centre by Centre Name
        public async Task<CentreViewModel> GetCentreByName(string centreName)
        {
            CentreViewModel centreViewModel = null;
            var centre = await _unitOfWork.CentreRepository.FirstOrDefault(p => p.Name.Trim().ToLower() == centreName.Trim().ToLower());
            if (centre != null)
            {
                centreViewModel = new CentreViewModel();
                centreViewModel.Id = centre.Id;
                centreViewModel.Name = centre.Name;
                centreViewModel.Email = centre.Email;
                centreViewModel.ContactNumber = centre.Phone;
                centreViewModel.IsActive = centre.IsActive;
            }
            return centreViewModel;
        }

        #endregion

        #region Delete Centre by Centre ID
        public async Task<int> DeleteCentre(long centreId, bool isActive, string roleName, long userId)
        {
            int returnVal = 0;
            var centreEntity = _unitOfWork.CentreRepository.Find(p => p.Id == centreId);
            if (centreEntity != null)
            {
                var userCentreList = await _unitOfWork.UserCentreRepository.GetWhere(p => p.CentreId == centreId);

                if (userCentreList.Any())
                {
                    foreach (var user in userCentreList)
                    {
                        var result = await _userComponent.ActiveInactivateUserInfo(centreId, user.UserInfoId, isActive, Convert.ToInt32(userId));
                        
                        if (result == 0)
                            return 0;
                    }
                }

                try
                {
                    centreEntity.IsActive = isActive;
                    centreEntity.ModifiedBy = Convert.ToInt32(userId);
                    centreEntity.ModifiedDate = DateTime.Now;

                    _unitOfWork.CentreRepository.Update(centreEntity);

                    if (isActive)
                    {
                        AuditLogCentre(Messages.ADL_PorscheCentreActive.Replace("#PorscheCentre#", centreEntity.Name), 0, roleName, userId);
                    }
                    else
                    {
                        AuditLogCentre(Messages.ADL_PorscheCentreInactive.Replace("#PorscheCentre#", centreEntity.Name), 0, roleName, userId);
                    }
                    

                    returnVal = await _unitOfWork.SaveChangesAsync();

                    AuditTrailCentre(centreEntity, userId, Constants.Deleted);
                }
                catch (Exception ex)
                {
                    return returnVal;
                }
                
            }
            return returnVal;
        }

        #endregion

        #region Manage Centre
        public async Task<int> AddUpdateCentre(CentreViewModel centreViewModel, string roleName, long? userId)
        {
            Centre centreEntity = null;
            if (centreViewModel.Id > 0)
            {
                centreEntity = _unitOfWork.CentreRepository.Find(p => p.Id == centreViewModel.Id);
                if (centreEntity != null)
                {
                    AuditTrailCentre(centreEntity, userId, Constants.Initial);
                }
            }
            else
            {
                centreEntity = new Centre();
            }
            centreEntity.Name = centreViewModel.Name;

            if (centreViewModel.Id > 0)
            {
                centreEntity.ModifiedBy = Convert.ToInt32(userId);
                centreEntity.ModifiedDate = DateTime.Now;
                _unitOfWork.CentreRepository.Update(centreEntity);

                AuditLogCentre(Messages.ADL_PorscheCentreUpdate.Replace("#PorscheCentre#", centreEntity.Name), 0, roleName, userId.Value);

                AuditTrailCentre(centreEntity, userId, Constants.Updated);
            }
            else
            {
                centreEntity.Phone = centreViewModel.ContactNumber;
                centreEntity.Email = centreViewModel.Email;
                centreEntity.IsActive = true;
                centreEntity.CreatedBy = Convert.ToInt32(userId);
                centreEntity.CreatedDate = DateTime.Now;
                _unitOfWork.CentreRepository.Add(centreEntity);

                AuditLogCentre(Messages.ADL_PorscheCentreCreate.Replace("#PorscheCentre#", centreEntity.Name), 0, roleName, userId.Value);
            }
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Check whether a centre with given email exists to avoid duplicacy
        public async Task<bool> CheckEmailExists(string email)
        {
            var centreList = (await _unitOfWork.CentreRepository.GetAllAsync()).ToList();
            return centreList.Any(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Check whether a centre with given name exists to avoid duplicacy

        public async Task<bool> CheckNameExists(string name)
        {
            var centreList = (await _unitOfWork.CentreRepository.GetAllAsync()).ToList();
            return centreList.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> CheckNameExistsEdit(long centreId, string name)
        {
            var centreList = (await _unitOfWork.CentreRepository.GetAllAsync()).ToList();
            return centreList.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && p.Id != centreId);
        }

        #endregion

        #region Check whether a centre with given phone exists to avoid duplicacy
        public async Task<bool> CheckPhoneExists(string phone)
        {
            var centreList = (await _unitOfWork.CentreRepository.GetAllAsync()).ToList();
            return centreList.Any(p => p.Phone.Equals(phone, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region To maintain delete/inactive history of centre
        private void AuditTrailCentre(Centre centreEntity, long? userId, string status)
        {
            CentreHistory historyEntity = new CentreHistory();
            historyEntity.Name = centreEntity.Name;
            historyEntity.CentreId = centreEntity.Id;
            historyEntity.Phone = centreEntity.Phone;
            historyEntity.Email = centreEntity.Email;
            historyEntity.Status = status;
            historyEntity.IsActive = centreEntity.IsActive;
            historyEntity.CreatedBy = Convert.ToInt32(userId);
            historyEntity.CreatedDate = DateTime.Now;
            _unitOfWork.CentreHistoryRepository.Add(historyEntity);
        }

        #endregion

        #region To search reccords case-insesitive
        private void SearchRecords(ref List<CentreViewModel> centreList, string searchString)
        {
            if (!centreList.Any() || string.IsNullOrWhiteSpace(searchString))
                return;
            centreList = centreList.Where(o => o.Name.ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant()) ||
                                    o.Email.ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant()) ||
                                    Convert.ToString(o.Staff).ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant()) ||
                                    o.ContactNumber.ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                   ).ToList();
        }

        #endregion

        #region Add Audit Log For Centre

        private void AuditLogCentre(string eventName, long centreId, string roleName, long userId)
        {
            AuditLog auditLogEntity = new AuditLog();

            auditLogEntity.EventName = eventName;
            auditLogEntity.ActivityDate = DateTime.Now.Date;
            auditLogEntity.ActivityTime = DateTime.Now.ToString("hh:mm tt");
            auditLogEntity.CentreId = null;
            auditLogEntity.RoleName = roleName;
            auditLogEntity.ModuleName = AppRoles.Admin;
            auditLogEntity.IsActive = true;
            auditLogEntity.CreatedBy = Convert.ToInt32(userId);
            auditLogEntity.CreatedDate = DateTime.Now;

            _unitOfWork.AuditLogRepository.Add(auditLogEntity);
        }

        #endregion

        #endregion
    }
}
