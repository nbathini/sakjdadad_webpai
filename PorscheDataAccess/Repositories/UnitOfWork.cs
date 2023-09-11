using PorscheDataAccess.DBModels;
using PorscheDataAccess.PorscheContext;

namespace PorscheDataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        #region Private variables
        private readonly PorscheDbContext _context;
        private IGenericRepository<CarModel> _carModelRepository;
        IGenericRepository<Customer> _customerRepository;
        IGenericRepository<Delivery> _deliveryRepository;       
        
        IGenericRepository<UserInfo> _userInfoRepository;
        IGenericRepository<UserTokenInfo> _userTokenInfoRepository;
        IGenericRepository<QuestionType> _questionTypeRepository;
        IGenericRepository<SurveyPreparation> _surveyPreparationRepository;
        IGenericRepository<CustomerSurvey> _customerSurveyRepository;
        IGenericRepository<Centre> _centreRepository;
        
        IGenericRepository<PreDeliveryCheckList> _preDeliveryChecklistRepository;
        IGenericRepository<CentreHistory> _centreHistoryRepository;
        
        IGenericRepository<DeliveryHistory> _deliveryHistoryRepository;
        IGenericRepository<DeliveryType> _deliveryTypeRepository;
        IGenericRepository<DeliveryCheckList> _deliveryCheckListRepository;
        IGenericRepository<FileDetail> _fileDetailRepository;
        IGenericRepository<LinkDetail> _linkDetailRepository;
        IGenericRepository<Modules> _moduleRepository;
        IGenericRepository<RoleModule> _roleModuleRepository;
        IGenericRepository<UserRoleModule> _userRoleModuleRepository;
        
        IGenericRepository<UserCentre> _userCentreRepository;
        IGenericRepository<AuditLog> _auditLogRepository;

        #endregion

        public UnitOfWork(PorscheDbContext context)
        {
            _context = context;
        }
        #region Properties
        public IGenericRepository<CarModel> CarModelRepository
        {
            get
            {
                return _carModelRepository ?? (_carModelRepository = new GenericRepository<CarModel>(_context));
            }
        }
        public IGenericRepository<Customer> CustomerRepository
        {
            get
            {
                return _customerRepository ?? (_customerRepository = new GenericRepository<Customer>(_context));
            }
        }
        public IGenericRepository<Delivery> DeliveryRepository
        {
            get
            {
                return _deliveryRepository ?? (_deliveryRepository = new GenericRepository<Delivery>(_context));
            }
        }
        public IGenericRepository<QuestionType> QuestionTypeRepository
        {
            get
            {
                return _questionTypeRepository ?? (_questionTypeRepository = new GenericRepository<QuestionType>(_context));
            }
        }
        public IGenericRepository<SurveyPreparation> SurveyPreparationRepository
        {
            get
            {
                return _surveyPreparationRepository ?? (_surveyPreparationRepository = new GenericRepository<SurveyPreparation>(_context));
            }
        }
        public IGenericRepository<CustomerSurvey> CustomerSurveyRepository
        {
            get
            {
                return _customerSurveyRepository ?? (_customerSurveyRepository = new GenericRepository<CustomerSurvey>(_context));
            }
        }
        public IGenericRepository<Centre> CentreRepository
        {
            get
            {
                return _centreRepository ?? (_centreRepository = new GenericRepository<Centre>(_context));
            }
        }        
        public IGenericRepository<PreDeliveryCheckList> PreDeliveryChecklistRepository
        {
            get
            {
                return _preDeliveryChecklistRepository ?? (_preDeliveryChecklistRepository = new GenericRepository<PreDeliveryCheckList>(_context));
            }
        }
        public IGenericRepository<CentreHistory> CentreHistoryRepository
        {
            get
            {
                return _centreHistoryRepository ?? (_centreHistoryRepository = new GenericRepository<CentreHistory>(_context));
            }
        }
        public IGenericRepository<DeliveryHistory> DeliveryHistoryRepository
        {
            get
            {
                return _deliveryHistoryRepository ?? (_deliveryHistoryRepository = new GenericRepository<DeliveryHistory>(_context));
            }
        }
        public IGenericRepository<DeliveryType> DeliveryTypeRepository
        {
            get
            {
                return _deliveryTypeRepository ?? (_deliveryTypeRepository = new GenericRepository<DeliveryType>(_context));
            }
        }
        public IGenericRepository<DeliveryCheckList> DeliveryCheckListRepository
        {
            get
            {
                return _deliveryCheckListRepository ?? (_deliveryCheckListRepository = new GenericRepository<DeliveryCheckList>(_context));
            }
        }

        public IGenericRepository<FileDetail> FileDetailRepository
        {
            get
            {
                return _fileDetailRepository ?? (_fileDetailRepository = new GenericRepository<FileDetail>(_context));
            }
        }
        public IGenericRepository<LinkDetail> LinkDetailRepository
        {
            get
            {
                return _linkDetailRepository ?? (_linkDetailRepository = new GenericRepository<LinkDetail>(_context));
            }
        }

        public IGenericRepository<Modules> ModuleRepository
        {
            get
            {
                return _moduleRepository ?? (_moduleRepository = new GenericRepository<Modules>(_context));
            }
        }

        public IGenericRepository<RoleModule> RoleModuleRepository
        {
            get
            {
                return _roleModuleRepository ?? (_roleModuleRepository = new GenericRepository<RoleModule>(_context));
            }
        }

        public IGenericRepository<UserInfo> UserInfoRepository
        {
            get
            {
                return _userInfoRepository ?? (_userInfoRepository = new GenericRepository<UserInfo>(_context));
            }
        }

        public IGenericRepository<UserTokenInfo> UserTokenInfoRepository 
        {
            get
            {
                return _userTokenInfoRepository ?? (_userTokenInfoRepository = new GenericRepository<UserTokenInfo>(_context));
            }
        }

        public IGenericRepository<UserRoleModule> UserRoleModuleRepository
        {
            get
            {
                return _userRoleModuleRepository ?? (_userRoleModuleRepository = new GenericRepository<UserRoleModule>(_context));
            }
        }

        public IGenericRepository<UserCentre> UserCentreRepository
        {
            get
            {
                return _userCentreRepository ?? (_userCentreRepository = new GenericRepository<UserCentre>(_context));
            }
        }

        public IGenericRepository<AuditLog> AuditLogRepository
        {
            get
            {
                return _auditLogRepository ?? (_auditLogRepository = new GenericRepository<AuditLog>(_context));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// save the changes to Db
        /// </summary>
        /// <returns></returns>
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// Asynchronously save the changes to Db
        /// </summary>
        /// <returns></returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Dispose the resources held by UnitOfWork
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
        #endregion
    }
}
