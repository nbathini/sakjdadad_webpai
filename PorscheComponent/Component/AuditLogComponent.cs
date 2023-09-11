using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheDataAccess.Repositories;
using PorscheUtilities.HelperClass;
using PorscheUtilities.Models;

namespace PorscheComponent.Component
{
    public class AuditLogComponent : IAuditLogComponent
    {
        #region Private Variables

        private readonly IUnitOfWork _unitOfWork;

        #endregion

        #region Constructor
        public AuditLogComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        #endregion

        #region Methods

        #region Add Audit Log
        public async Task<int> ManageAuditLog(AuditLogViewModel auditLog, long? userId)
        {
            AuditLog auditLogEntity = new AuditLog();

            auditLogEntity.EventName = auditLog.EventName;
            auditLogEntity.ActivityDate = DateTime.Now.Date;
            auditLogEntity.ActivityTime = DateTime.Now.ToString("hh:mm tt");
            auditLogEntity.CentreId = auditLog.CentreId;
            auditLogEntity.RoleName = auditLog.RoleName;
            auditLogEntity.ModuleName = auditLog.ModuleName;
            auditLogEntity.IsActive = true;
            auditLogEntity.CreatedBy = Convert.ToInt32(userId);
            auditLogEntity.CreatedDate = DateTime.Now;
            _unitOfWork.AuditLogRepository.Add(auditLogEntity);


            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Get Audit Log for Web Page
        public async Task<PagedList<AuditLogViewModel>> GetAuditLog(PagingParameter pagingParameter)
        {
            var auditLogList = await _unitOfWork.AuditLogRepository.GetAllAsync();
            var userInfoList = await _unitOfWork.UserInfoRepository.GetAllAsync();
            var centreList = await _unitOfWork.CentreRepository.GetAllAsync();

            var auditLogs = (from auditLog in auditLogList
                                join userInfo in userInfoList on auditLog.CreatedBy equals userInfo.Id
                                join centre in centreList on auditLog.CentreId equals centre.Id
                                into Centres
                               from defaultCentre in Centres.DefaultIfEmpty()
                             where auditLog.IsActive == true
                                select new AuditLogViewModel
                                {
                                    Id = auditLog.Id,
                                    EventName = auditLog.EventName,
                                    ActivityDate = auditLog.ActivityDate,
                                    ActivityTime = auditLog.ActivityTime,
                                    CentreId = auditLog.CentreId,
                                    CentreName = defaultCentre != null ? defaultCentre.Name : string.Empty,
                                    RoleName = auditLog.RoleName,
                                    ModuleName = auditLog.ModuleName,
                                    IsActive = auditLog.IsActive,
                                    CreatedBy = auditLog.CreatedBy,
                                    PerformedBy = userInfo.FirstName + (string.IsNullOrEmpty(userInfo.LastName) ? string.Empty : (" " + userInfo.LastName)),
                                    CreatedDate = auditLog.CreatedDate
                                }
                                ).OrderByDescending(x => x.Id).ToList();


            if (!string.IsNullOrWhiteSpace(pagingParameter.Search))
            {
                SearchRecords(ref auditLogs, pagingParameter.Search);
            }
            return PagedList<AuditLogViewModel>.ToPagedList(auditLogs, pagingParameter.PageNumber, pagingParameter.PageSize);
        }

        #endregion

        #region Get Audit Log for Export to Excel
        public async Task<List<AuditLogViewModel>> GetAllAuditLog()
        {
            var auditLogList = await _unitOfWork.AuditLogRepository.GetAllAsync();
            var userInfoList = await _unitOfWork.UserInfoRepository.GetAllAsync();
            var centreList = await _unitOfWork.CentreRepository.GetAllAsync();

            var auditLogs = (from auditLog in auditLogList
                             join userInfo in userInfoList on auditLog.CreatedBy equals userInfo.Id
                             join centre in centreList on auditLog.CentreId equals centre.Id
                             into Centres
                             from defaultCentre in Centres.DefaultIfEmpty()
                             where auditLog.IsActive == true
                             select new AuditLogViewModel
                             {
                                 Id = auditLog.Id,
                                 EventName = auditLog.EventName,
                                 ActivityDate = auditLog.ActivityDate,
                                 ActivityTime = auditLog.ActivityTime + " EST",
                                 CentreId = auditLog.CentreId,
                                 CentreName = defaultCentre != null ? defaultCentre.Name : string.Empty,
                                 RoleName = auditLog.RoleName,
                                 ModuleName = auditLog.ModuleName,
                                 IsActive = auditLog.IsActive,
                                 CreatedBy = auditLog.CreatedBy,
                                 PerformedBy = userInfo.FirstName + (string.IsNullOrEmpty(userInfo.LastName) ? string.Empty : (" " + userInfo.LastName)),
                                 CreatedDate = auditLog.CreatedDate
                             }
                                ).OrderByDescending(x => x.Id).ToList();

            return auditLogs;
        }

        #endregion

        #region Add Audit Log
        public async Task<int> AddAuditLog(string moduleName, string eventName, long centreId, string roleName, long userId)
        {
            AuditLog auditLogEntity = new AuditLog();

            auditLogEntity.EventName = eventName;
            auditLogEntity.ActivityDate = DateTime.Now.Date;
            auditLogEntity.ActivityTime = DateTime.Now.ToString("hh:mm tt");
            auditLogEntity.CentreId = centreId;
            auditLogEntity.RoleName = roleName;
            auditLogEntity.ModuleName = moduleName;
            auditLogEntity.IsActive = true;
            auditLogEntity.CreatedBy = Convert.ToInt32(userId);
            auditLogEntity.CreatedDate = DateTime.Now;
            _unitOfWork.AuditLogRepository.Add(auditLogEntity);


            return await _unitOfWork.SaveChangesAsync();
            
        }

        #endregion

        #region Audit Log Search Functionality 
        private void SearchRecords(ref List<AuditLogViewModel> auditLogs, string searchString)
        {
            if (!auditLogs.Any() || string.IsNullOrWhiteSpace(searchString))
                return;

            auditLogs = auditLogs.Where(o => (Convert.ToString(o.EventName).ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant()) 
                                                    || Convert.ToString(o.CentreName).ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                                    || Convert.ToString(o.PerformedBy).ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                                    || Convert.ToString(o.ModuleName).ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                                    || string.Format("{0:dd-MMM-yy}", o.ActivityDate).ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                                    || string.Format("{0:hh:mm tt}", o.ActivityTime).ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant()))).ToList();
        }

        #endregion

        #endregion
    }
}
