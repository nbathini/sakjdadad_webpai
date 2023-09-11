using PorscheDataAccess.DBModels;

namespace PorscheDataAccess.Repositories
{
    public interface IUnitOfWork
    {
        #region Properties
        IGenericRepository<CarModel> CarModelRepository { get; }
        IGenericRepository<Customer> CustomerRepository { get; }
        IGenericRepository<Delivery> DeliveryRepository { get; }
        IGenericRepository<UserInfo> UserInfoRepository { get; }
        IGenericRepository<UserTokenInfo> UserTokenInfoRepository { get; }
        IGenericRepository<QuestionType> QuestionTypeRepository { get; }
        IGenericRepository<SurveyPreparation> SurveyPreparationRepository { get; }
        IGenericRepository<CustomerSurvey> CustomerSurveyRepository { get; }
        IGenericRepository<Centre> CentreRepository { get; }        
        IGenericRepository<PreDeliveryCheckList> PreDeliveryChecklistRepository { get; }
        IGenericRepository<CentreHistory> CentreHistoryRepository { get; }
        IGenericRepository<DeliveryHistory> DeliveryHistoryRepository { get; }
        IGenericRepository<DeliveryType> DeliveryTypeRepository { get; }
        IGenericRepository<DeliveryCheckList> DeliveryCheckListRepository { get; }
        IGenericRepository<FileDetail> FileDetailRepository { get; }
        IGenericRepository<LinkDetail> LinkDetailRepository { get; }
        IGenericRepository<Modules> ModuleRepository { get; }
        IGenericRepository<RoleModule> RoleModuleRepository { get; }
        IGenericRepository<UserRoleModule> UserRoleModuleRepository { get; }
        IGenericRepository<UserCentre> UserCentreRepository { get; }
        IGenericRepository<AuditLog> AuditLogRepository { get; }

        #endregion

        #region Methods
        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        #endregion
    }
}
