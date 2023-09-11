using Newtonsoft.Json;
using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheDataAccess.Repositories;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PorscheComponent.Component
{
    public class DeliveryComponent : IDeliveryComponent
    {
        #region Private Variables

        private readonly IUnitOfWork _unitOfWork;

        #endregion

        #region Constructor
        public DeliveryComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #endregion

        #region Methods

        #region Fetch delivery details in between specified dates
        public async Task<List<DeliveryViewModel>> GetDelivery(long centreId, DateTime startDate, DateTime endDate)
        {
            var customerList = await _unitOfWork.CustomerRepository.GetAllAsync();
            var carModelList = await _unitOfWork.CarModelRepository.GetAllAsync();
            var centreList = await _unitOfWork.CentreRepository.GetAllAsync();
            var usersList = await _unitOfWork.UserInfoRepository.GetAllAsync();
            var userCentreList = await _unitOfWork.UserCentreRepository.GetAllAsync();

            var consultantList = (from user in usersList
                                  join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                                  where userCentre.CentreId == centreId && user.RoleName.Trim().ToLower() == DealerJobRoles.SalesConsultant.Trim().ToLower() && userCentre.IsActive == true && user.IsActive == true
                                  select new UserInfoViewModel
                                  {
                                      Id = user.Id,
                                      Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty)
                                  }).ToList();

            var proList = (from user in usersList
                           join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                           where userCentre.CentreId == centreId && user.RoleName.Trim().ToLower() == DealerJobRoles.PorschePro.Trim().ToLower() && userCentre.IsActive == true && user.IsActive == true
                           select new UserInfoViewModel
                           {
                               Id = user.Id,
                               Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty)
                           }).ToList();

            var advisorList = (from user in usersList
                               join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                               where userCentre.CentreId == centreId && user.RoleName.Trim().ToLower() == DealerJobRoles.ServiceAdvisor.Trim().ToLower() && userCentre.IsActive == true && user.IsActive == true
                               select new UserInfoViewModel
                               {
                                   Id = user.Id,
                                   Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty)
                               }).ToList();
            

            var deliveryList = await _unitOfWork.DeliveryRepository.GetAllAsync();
            var deliveryTypeList = await _unitOfWork.DeliveryTypeRepository.GetAllAsync();
            var deliveryCheckList = await _unitOfWork.DeliveryCheckListRepository.GetAllAsync();

            var deliverystatusList = (from delivery in deliveryList
                                      join deliveryCheck in deliveryCheckList on delivery.Id equals deliveryCheck.Delivery.Id into del
                                      join centre in centreList on delivery.Centre.Id equals centre.Id into centrre
                                      from deliveryEntity in del.DefaultIfEmpty()
                                      where delivery.DeliveryDate >= startDate && delivery.DeliveryDate <= endDate && delivery.Centre.Id == centreId
                                      select new DeliveryCheckListViewModel
                                      {
                                          Id = deliveryEntity?.Id ?? 0,
                                          DeliveryId = delivery.Id,
                                          QuestionResponse = deliveryEntity?.QuestionResponse ?? String.Empty,
                                          DeliveryStatus = 0,
                                          SkipSurvey = delivery.SkipSurvey
                                      }).ToList();
            foreach (var delivery in deliverystatusList)
            {
                delivery.DeliveryStatus = await GetDeliveryStatus(delivery);
            }

            if (consultantList.Any() && proList.Any() && advisorList.Any())
            {

                return (from customer in customerList
                        join delivery in deliveryList on customer.Id equals delivery.Customer.Id
                        join model in carModelList on delivery.Model?.Id equals model.Id
                        join advisor in advisorList on delivery.ServiceAdvisor.Id equals advisor.Id into ad
                        join pro in proList on delivery.PorschePro.Id equals pro.Id into pr
                        join consultant in consultantList on delivery.SalesConsultant?.Id equals consultant.Id into cons
                        join centre in centreList on delivery.Centre.Id equals centre.Id into centrre
                        join deliveryType in deliveryTypeList on delivery.DeliveryType?.Id equals deliveryType.Id into dt
                        join deliveryStatus in deliverystatusList on delivery.Id equals deliveryStatus.DeliveryId into del
                        from adv in ad.DefaultIfEmpty()
                        from prro in pr.DefaultIfEmpty()
                        from consult in cons.DefaultIfEmpty()
                        from centreEntity in centrre.DefaultIfEmpty()
                        from type in dt.DefaultIfEmpty()
                        from deliverStat in del.DefaultIfEmpty()
                        where delivery.DeliveryDate >= startDate && delivery.DeliveryDate <= endDate && delivery.Centre.Id == centreId && delivery.IsActive == true
                        orderby delivery.Id descending
                        select new DeliveryViewModel
                        {
                            Id = delivery.Id,
                            CustomerId = customer.Id,
                            CustomerName = customer.Name,
                            CustomerEmail = customer.Email,
                            ContactNumber = customer.ContactNumber,
                            SalesConsultant = new UserInfoViewModel() { Id = consult?.Id ?? 0, Name = consult.Name ?? String.Empty },
                            ServiceAdvisor = new UserInfoViewModel() { Id = adv?.Id ?? 0, Name = adv.Name ?? String.Empty },
                            PorschePro = new UserInfoViewModel() { Id = prro?.Id ?? 0, Name = prro.Name ?? String.Empty },
                            DeliveryDate = delivery.DeliveryDate,
                            DeliveryTime = delivery.DeliveryTime,
                            IsSurveySent = delivery.IsSurveySent,
                            Model = new CarViewModel { Id = model?.Id ?? 0, Name = model?.Name ?? String.Empty, ImagePath = model?.ImagePath ?? String.Empty },
                            DeliveryType = new DeliveryTypeViewModel() { Id = type?.Id ?? 0, Name = type?.Name ?? String.Empty },
                            SkipSurvey = delivery.SkipSurvey,
                            DeliveryStatus = deliverStat?.DeliveryStatus ?? 0
                        }).OrderByDescending(x => x.Id).ToList();
            }
            return new List<DeliveryViewModel>();
        }

        #endregion

        #region Add new delivery and customer
        public async Task<int> AddDelivery(long centreId, DeliveryViewModel deliveryViewModel, string userRole, long loggedInuserId)
        {

            var consultantEntity = _unitOfWork.UserInfoRepository.Find(p => p.Id == deliveryViewModel.SalesConsultant.Id);
            var proEntity = _unitOfWork.UserInfoRepository.Find(p => p.Id == deliveryViewModel.PorschePro.Id);
            var advisorEntity = _unitOfWork.UserInfoRepository.Find(p => p.Id == deliveryViewModel.ServiceAdvisor.Id);


            var centreEntity = _unitOfWork.CentreRepository.Find(p => p.Id == centreId);

            if (consultantEntity != null && proEntity != null && advisorEntity != null && centreEntity != null)
            {
                #region Add Customer Details

                Customer customerEntity = new Customer();
                customerEntity.Email = deliveryViewModel.CustomerEmail;
                customerEntity.Name = deliveryViewModel.CustomerName;
                customerEntity.ContactNumber = deliveryViewModel.ContactNumber;
                customerEntity.IsActive = true;
                customerEntity.CreatedBy = Convert.ToInt32(loggedInuserId);
                customerEntity.CreatedDate=DateTime.Now;
                _unitOfWork.CustomerRepository.Add(customerEntity);

                #endregion

                #region Add Delivery

                Delivery delivery = new Delivery();
                delivery.Customer = customerEntity;
                delivery.SalesConsultant = consultantEntity;
                delivery.ServiceAdvisor = advisorEntity;
                delivery.PorschePro = proEntity;
                delivery.Model = _unitOfWork.CarModelRepository.Find(p => p.Id == deliveryViewModel.Model.Id);
                delivery.DeliveryDate = deliveryViewModel.DeliveryDate;
                delivery.DeliveryTime = deliveryViewModel.DeliveryTime;
                delivery.IsSurveySent = false;
                delivery.Centre = centreEntity;
                delivery.SkipSurvey = deliveryViewModel.SkipSurvey;
                delivery.DeliveryType = _unitOfWork.DeliveryTypeRepository.Find(p => p.Id == deliveryViewModel.DeliveryType.Id);
                delivery.IsActive = true;
                delivery.CreatedBy = Convert.ToInt32(loggedInuserId);
                delivery.CreatedDate = DateTime.Now;
                _unitOfWork.DeliveryRepository.Add(delivery);

                #endregion

                #region Add Audit Log - Add Customer

                AuditLogDelivery(Messages.ADL_CustomerAdded.Replace("#CUSTOMER_NAME#", deliveryViewModel.CustomerName), centreId, userRole, loggedInuserId);

                #endregion

                #region Add Audit Log - Add Delivery

                AuditLogDelivery(Messages.ADL_DeliveryAdded.Replace("#CARMODEL#", delivery.Model.Name).Replace("#CUSTOMER_NAME#", deliveryViewModel.CustomerName), centreId, userRole, loggedInuserId);

                #endregion

                
            }

            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Edit delivery
        public async Task<int> EditDelivery(long centreId, DeliveryViewModel deliveryViewModel, long? userId, string userRole, string userRoleId)
        {
            var consultantEntity = _unitOfWork.UserInfoRepository.Find(p => p.Id == deliveryViewModel.SalesConsultant.Id);
            var proEntity = _unitOfWork.UserInfoRepository.Find(p => p.Id == deliveryViewModel.PorschePro.Id);
            var advisorEntity = _unitOfWork.UserInfoRepository.Find(p => p.Id == deliveryViewModel.ServiceAdvisor.Id);
            var centreEntity = _unitOfWork.CentreRepository.Find(p => p.Id == centreId);
            Customer customerEntity = null;
            Delivery deliveryEntity = null;

            if (consultantEntity != null && proEntity != null && advisorEntity != null && centreEntity != null)
            {
                if (deliveryViewModel.Id > 0)
                {
                    DeliveryViewModel delivery = await GetDeliveryById(centreId, deliveryViewModel.Id);
                    
                    if (delivery != null)
                    {
                        deliveryEntity = _unitOfWork.DeliveryRepository.Find(p => p.Id == deliveryViewModel.Id);
                        customerEntity = _unitOfWork.CustomerRepository.Find(p => p.Id == deliveryViewModel.CustomerId);
                    }
                    if (deliveryEntity != null && customerEntity != null && !deliveryEntity.IsSurveySent)
                    {
                        
                        AuditTrailDelivery(delivery, userId, Constants.Initial, Convert.ToInt32(userRoleId), centreId);

                        #region Add Audit Log - Edit Customer

                        if (customerEntity.Name != deliveryViewModel.CustomerName || customerEntity.Email != deliveryViewModel.CustomerEmail || customerEntity.ContactNumber != deliveryViewModel.ContactNumber)
                        {
                            AuditLogDelivery(Messages.ADL_CustomerUpdated.Replace("#CUSTOMER_NAME#", deliveryViewModel.CustomerName), centreId, userRole, Convert.ToInt32(userId));
                        }

                        #endregion

                        #region Customer Details Update

                        customerEntity.Email = deliveryViewModel.CustomerEmail;
                        customerEntity.Name = deliveryViewModel.CustomerName;
                        customerEntity.ContactNumber = deliveryViewModel.ContactNumber;
                        customerEntity.ModifiedBy = (userId != null ? Convert.ToInt32(userId) : 0);
                        customerEntity.ModifiedDate = DateTime.Now;
                        _unitOfWork.CustomerRepository.Update(customerEntity);

                        #endregion

                        #region Update Delivery Details

                        deliveryEntity.SalesConsultant = consultantEntity;
                        deliveryEntity.ServiceAdvisor = advisorEntity;
                        deliveryEntity.PorschePro = proEntity;
                        deliveryEntity.Model = _unitOfWork.CarModelRepository.Find(p => p.Id == deliveryViewModel.Model.Id);
                        deliveryEntity.DeliveryDate = deliveryViewModel.DeliveryDate;
                        deliveryEntity.DeliveryTime = deliveryViewModel.DeliveryTime;
                        deliveryEntity.Centre = centreEntity;
                        deliveryEntity.IsSurveySent = deliveryEntity.IsSurveySent;
                        deliveryEntity.SkipSurvey = deliveryViewModel.SkipSurvey;
                        deliveryEntity.DeliveryType = _unitOfWork.DeliveryTypeRepository.Find(p => p.Id == deliveryViewModel.DeliveryType.Id);
                        deliveryEntity.ModifiedBy = (userId != null ? Convert.ToInt32(userId) : 0);
                        deliveryEntity.ModifiedDate = DateTime.Now;
                        _unitOfWork.DeliveryRepository.Update(deliveryEntity);

                        #endregion

                        AuditTrailDelivery(deliveryViewModel, userId, Constants.Updated, Convert.ToInt32(userRoleId), centreId);

                        #region Add Audit Log - Edit Delivery

                        AuditLogDelivery(Messages.ADL_DeliveryUpdated.Replace("#CARMODEL#", delivery.Model.Name).Replace("#CUSTOMER_NAME#", deliveryViewModel.CustomerName), centreId, userRole, Convert.ToInt32(userId));

                        #endregion
                    }
                }

            }
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Delete delivery only when survey not sent
        public async Task<int> DeleteDelivery(long centreId, int deliveryId, long userId, string userRole, string userRoleId)
        {
            var deliveryEntity = _unitOfWork.DeliveryRepository.Find(p => p.Id == deliveryId);

            if (deliveryEntity != null && deliveryEntity.IsSurveySent != true)
            {
                var delivery = await GetDeliveryById(centreId, deliveryId);

                if (delivery != null)
                {
                    #region Delete/Inactive Survey Preparation

                    var surveyPrepEntity = _unitOfWork.SurveyPreparationRepository.Find(p => p.Delivery.Id == deliveryId);
                    if (surveyPrepEntity != null)
                    {
                        surveyPrepEntity.IsActive = false;
                        surveyPrepEntity.ModifiedBy = (userId != null ? Convert.ToInt32(userId) : 0);
                        surveyPrepEntity.ModifiedDate = DateTime.Now;
                        _unitOfWork.SurveyPreparationRepository.Update(surveyPrepEntity);
                    }

                    #endregion

                    #region Delete/Inactive Pre Delivery 

                    var preDeliveryEntity = _unitOfWork.PreDeliveryChecklistRepository.Find(p => p.Delivery.Id == deliveryId);
                    if (preDeliveryEntity != null)
                    {
                        preDeliveryEntity.IsActive = false;
                        preDeliveryEntity.ModifiedBy = (userId != null ? Convert.ToInt32(userId) : 0);
                        preDeliveryEntity.ModifiedDate = DateTime.Now;
                        _unitOfWork.PreDeliveryChecklistRepository.Update(preDeliveryEntity);
                    }

                    #endregion

                    #region Delete Delivery and Audit Trail Delivery

                    deliveryEntity.IsActive = false;
                    deliveryEntity.ModifiedBy = (userId != null ? Convert.ToInt32(userId) : 0);
                    deliveryEntity.ModifiedDate = DateTime.Now;
                    _unitOfWork.DeliveryRepository.Update(deliveryEntity);

                    AuditTrailDelivery(delivery, userId, Constants.Deleted, Convert.ToInt32(userRoleId), centreId);

                    #endregion

                    #region Add Audit Log - Delivery Deleted

                    AuditLogDelivery(Messages.ADL_DeliveryDeleted.Replace("#CUSTOMER_NAME#", delivery.CentreName), centreId, userRole, Convert.ToInt32(userId));

                    #endregion

                    return await _unitOfWork.SaveChangesAsync();
                }
            }
            return 0;
        }

        #endregion

        #region Get Car models list
        public async Task<List<CarViewModel>> GetCarModels()
        {
            var modelList = await _unitOfWork.CarModelRepository.GetAllAsync();
            return (from model in modelList where model.IsActive == true
                    select new CarViewModel { Id = model.Id, Name = model.Name, ImagePath = model.ImagePath, Description = model.Description }).OrderByDescending(x => x.Id).ToList();
        }

        #endregion

        #region To fetch delivery details based on Id and centreId
        public async Task<DeliveryViewModel> GetDeliveryById(long centreId, long deliveryId)
        {
            var customerList = await _unitOfWork.CustomerRepository.GetAllAsync();
            var carModelList = await _unitOfWork.CarModelRepository.GetAllAsync();
            var centreList = await _unitOfWork.CentreRepository.GetAllAsync();
            var userCentreList = await _unitOfWork.UserCentreRepository.GetAllAsync();
            
            
            var usersList = await _unitOfWork.UserInfoRepository.GetAllAsync();

            var consultantList = (from user in usersList
                                  join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                                  where userCentre.CentreId == centreId && user.RoleName.Trim().ToLower() == DealerJobRoles.SalesConsultant.Trim().ToLower() && userCentre.IsActive == true && user.IsActive == true
                                  select new UserInfoViewModel
                                  {
                                      Id = user.Id,
                                      Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty)
                                  }).ToList();

            var proList = (from user in usersList
                           join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                           where userCentre.CentreId == centreId && user.RoleName.Trim().ToLower() == DealerJobRoles.PorschePro.Trim().ToLower() && userCentre.IsActive == true && user.IsActive == true
                           select new UserInfoViewModel
                           {
                               Id = user.Id,
                               Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty)
                           }).ToList();

            var advisorList = (from user in usersList
                               join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                               where userCentre.CentreId == centreId && user.RoleName.Trim().ToLower() == DealerJobRoles.ServiceAdvisor.Trim().ToLower() && userCentre.IsActive == true && user.IsActive == true
                               select new UserInfoViewModel
                               {
                                   Id = user.Id,
                                   Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty)
                               }).ToList();

            var deliveryList = await _unitOfWork.DeliveryRepository.GetAllAsync();
            var deliveryTypeList = await _unitOfWork.DeliveryTypeRepository.GetAllAsync();
            if (consultantList.Any() && proList.Any() && advisorList.Any())
            {
                return (from customer in customerList
                        join delivery in deliveryList on customer.Id equals delivery.Customer.Id
                        join deliveryType in deliveryTypeList on delivery.DeliveryType?.Id equals deliveryType.Id into dt
                        join consultant in consultantList on delivery.SalesConsultant?.Id equals consultant.Id
                        join pro in proList on delivery.PorschePro.Id equals pro.Id
                        join advisor in advisorList on delivery.ServiceAdvisor.Id equals advisor.Id
                        join model in carModelList on delivery.Model?.Id equals model.Id into cd
                        join centre in centreList on delivery.Centre.Id equals centre.Id into centrre
                        from carModel in cd.DefaultIfEmpty()
                        from centreEntity in centrre.DefaultIfEmpty()
                        from type in dt.DefaultIfEmpty()
                        where delivery.Id == deliveryId && delivery.Centre.Id == centreId && carModel.IsActive == true
                        select new DeliveryViewModel
                        {
                            Id = delivery.Id,
                            CustomerId = customer.Id,
                            CustomerName = customer.Name,
                            CustomerEmail = customer.Email,
                            ContactNumber = customer.ContactNumber,
                            SalesConsultant = new UserInfoViewModel() { Id = consultant?.Id ?? 0, Name = consultant.Name ?? String.Empty },
                            ServiceAdvisor = new UserInfoViewModel() { Id = advisor?.Id ?? 0, Name = advisor.Name ?? String.Empty },
                            PorschePro = new UserInfoViewModel() { Id = pro?.Id ?? 0, Name = pro.Name ?? String.Empty },
                            DeliveryDate = delivery.DeliveryDate,
                            DeliveryTime = delivery.DeliveryTime,
                            IsSurveySent = delivery.IsSurveySent,
                            Model = new CarViewModel { Id = carModel?.Id ?? 0, Name = carModel?.Name ?? String.Empty, ImagePath = carModel.ImagePath ?? string.Empty, IsActive=carModel.IsActive },
                            SkipSurvey = delivery.SkipSurvey,
                            DeliveryType = new DeliveryTypeViewModel { Id = type?.Id ?? 0, Name = type?.Name ?? String.Empty },
                            CentreName = centreEntity?.Name ?? String.Empty
                        }).FirstOrDefault();
            }
            return null;
        }

        #endregion

        #region To get all sales consultant list of a centre
        public async Task<List<UserInfoViewModel>> GetConsultantList(long centreId)
        {
            var usersList = await _unitOfWork.UserInfoRepository.GetAllAsync();
            var userCentreList = await _unitOfWork.UserCentreRepository.GetAllAsync();


            return (from user in usersList
                    join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                    where userCentre.CentreId == centreId && user.RoleName.Trim().ToLower() == DealerJobRoles.SalesConsultant.Trim().ToLower() && userCentre.IsActive == true && user.IsActive == true
                    select new UserInfoViewModel
                    {
                        Id = user.Id,
                        Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty)
                    }).ToList();
        }

        #endregion

        #region To get all porsche pro list of a centre
        public async Task<List<UserInfoViewModel>> GetPorscheProList(long centreId)
        {
            var usersList = await _unitOfWork.UserInfoRepository.GetAllAsync();
            var userCentreList = await _unitOfWork.UserCentreRepository.GetAllAsync();


            return (from user in usersList
                    join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                    where userCentre.CentreId == centreId && user.RoleName.Trim().ToLower() == DealerJobRoles.PorschePro.Trim().ToLower() && userCentre.IsActive == true && user.IsActive == true
                    select new UserInfoViewModel
                    {
                        Id = user.Id,
                        Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty)
                    }).ToList();

            //var userJobRoleList = await _unitOfWork.UserJobRoleRepository.GetAllAsync();
            //var usersList = await _unitOfWork.UserInfoRepository.GetAllAsync();

            //return (from userJobRole in userJobRoleList
            //        join user in usersList on userJobRole.UserInfoId equals user.Id
            //        where userJobRole.JobRoleId == (Int32)Enums.Designation.PorshePro && userJobRole.IsActive == true && user.IsActive == true
            //        select new UserInfoViewModel
            //        {
            //            Id = user.Id,
            //            Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty)

            //        }).ToList();            
        }

        #endregion

        #region To get all service advisor list of a centre
        public async Task<List<UserInfoViewModel>> GetAdvisorList(long centreId)
        {
            var usersList = await _unitOfWork.UserInfoRepository.GetAllAsync();
            var userCentreList = await _unitOfWork.UserCentreRepository.GetAllAsync();

            return (from user in usersList
                    join userCentre in userCentreList on user.Id equals userCentre.UserInfoId
                    where userCentre.CentreId == centreId && user.RoleName.Trim().ToLower() == DealerJobRoles.ServiceAdvisor.Trim().ToLower() && userCentre.IsActive == true && user.IsActive == true
                    select new UserInfoViewModel
                    {
                        Id = user.Id,
                        Name = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? (" " + user.LastName) : string.Empty)
                    }).ToList();
        }

        #endregion

        #region To maintain delete history of Delivery deleted/inactivated
        private void AuditTrailDelivery(DeliveryViewModel deliveryEntity, long? userId, string status, int userRole, long centreId)
        {
            DeliveryHistory historyEntity = new DeliveryHistory();
            historyEntity.DeliveryId = deliveryEntity.Id;
            historyEntity.CustomerId = deliveryEntity.CustomerId;
            historyEntity.SalesConsultantId = deliveryEntity.SalesConsultant.Id;
            historyEntity.ServiceAdvisorId = deliveryEntity.ServiceAdvisor.Id;
            historyEntity.PorscheProId = deliveryEntity.PorschePro.Id;
            historyEntity.ModelId = deliveryEntity.Model.Id;
            historyEntity.DeliveryDate = deliveryEntity.DeliveryDate;
            historyEntity.DeliveryTime = deliveryEntity.DeliveryTime;
            historyEntity.IsSurveySent = deliveryEntity.IsSurveySent;
            historyEntity.CentreId = centreId;
            historyEntity.SkipSurvey = deliveryEntity.SkipSurvey;
            historyEntity.CreatedBy = Convert.ToInt32(userId);
            historyEntity.CreatedDate = DateTime.Now;
            historyEntity.Status = status;
            historyEntity.UserRole = userRole;
            historyEntity.DeliveryTypeId = deliveryEntity.DeliveryType.Id;          
            

            _unitOfWork.DeliveryHistoryRepository.Add(historyEntity);
        }

        #endregion

        #region Add Audit Log Delivery

        private void AuditLogDelivery(string eventName, long centreId, string roleName, long userId)
        {
            AuditLog auditLogEntity = new AuditLog();

            auditLogEntity.EventName = eventName;
            auditLogEntity.ActivityDate = DateTime.Now.Date;
            auditLogEntity.ActivityTime = DateTime.Now.ToString("hh:mm tt");
            auditLogEntity.CentreId = centreId;
            auditLogEntity.RoleName = roleName;
            auditLogEntity.ModuleName = AppRoles.Dealer;
            auditLogEntity.IsActive = true;
            auditLogEntity.CreatedBy = Convert.ToInt32(userId);
            auditLogEntity.CreatedDate = DateTime.Now;

            _unitOfWork.AuditLogRepository.Add(auditLogEntity);
        }

        #endregion

        #region Get delivery type list
        public async Task<List<DeliveryTypeViewModel>> GetDeliveryType()
        {
            var typeList = await _unitOfWork.DeliveryTypeRepository.GetAllAsync();
            return (from type in typeList
                    select new DeliveryTypeViewModel
                    {
                        Id = type.Id,
                        Name = type.Name
                    }).ToList();
        }

        #endregion

        #region Get current delivery status

        private async Task<int> GetDeliveryStatus(DeliveryCheckListViewModel delivery)
        {
            List<DeliveryCheckListJsonModel> deliveryCheckDeserialized = null;
            var existingCheckList = await _unitOfWork.DeliveryCheckListRepository.FirstOrDefault(p => p.Id == delivery.Id);
            if (existingCheckList == null && delivery.SkipSurvey != false)
            {
                return (Int32)Enums.DeliveryStatus.DeliveryPending;
            }
            else if (existingCheckList == null && delivery.SkipSurvey == false)
            {
                return 0;
            }
            else
            {
                deliveryCheckDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(existingCheckList.QuestionResponse);
                var followUpList = deliveryCheckDeserialized.SelectMany(x => x.Fields).SelectMany(v => v.Questions).Select(x => x.MoveToFollowUp).ToList();
                var deliveryPendingList = deliveryCheckDeserialized.SelectMany(x => x.Fields).SelectMany(v => v.Questions).Select(x => x.Answer).ToList();

                if (followUpList.Any(x => x == true))
                {
                    return (Int32)Enums.DeliveryStatus.CustomerFollowUpPending;
                }
                if (deliveryPendingList.Any(x => x == null || x.Equals("false", StringComparison.OrdinalIgnoreCase)))
                {
                    return (Int32)Enums.DeliveryStatus.DeliveryPending;
                }
                if (deliveryPendingList.All(x => x.Equals("true", StringComparison.OrdinalIgnoreCase)))
                {
                    return (Int32)Enums.DeliveryStatus.DeliveryDone;
                }
            }
            return 0;
        }

        #endregion

        #region Convert Imange URL to Base64 String
        public async Task<string> ImageUrlToBase64(string imageUrl)
        {
            try
            {
                using var httClient = new HttpClient();
                var imageBytes = await httClient.GetByteArrayAsync(imageUrl);
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

            return string.Empty;
            
        }
        #endregion

        #endregion

    }
}
