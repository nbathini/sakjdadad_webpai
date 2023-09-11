using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheDataAccess.Repositories;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;
using System.Net;

namespace PorscheComponent.Component
{
    public class SurveyComponent : ISurveyComponent
    {
        #region Private Variables
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IDeliveryComponent _deliveryComponent;
        private readonly IAmazonS3 _s3Client;
        #endregion

        #region Constructor
        public SurveyComponent(IUnitOfWork unitOfWork, IConfiguration configuration, IDeliveryComponent deliveryComponent, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _deliveryComponent = deliveryComponent;
            _s3Client = s3Client;
        }

        #endregion

        #region Methods

        #region To add or edit delivery prepataion details for a delivery
        public async Task<int> AddEditDeliveryPreparation(SurveyPreparationViewModel surveyPreparationViewModel, long loggedInuserId, long centreId, string userRole)
        {
            SurveyPreparation surveyPreparation = null;
            var delivery = _unitOfWork.DeliveryRepository.Find(p => p.Id == surveyPreparationViewModel.DeliveryId);
            var existingSurveyPrep = await _unitOfWork.SurveyPreparationRepository.FirstOrDefault(p => p.Delivery.Id == surveyPreparationViewModel.DeliveryId);

            var customer = _unitOfWork.CustomerRepository.Find(p => p.Id == surveyPreparationViewModel.CustomerId);

            if (delivery != null && customer != null)
            {
                if (existingSurveyPrep != null)
                {
                    #region Delivery Preparation Details Updated

                    existingSurveyPrep.QuestionResponse = surveyPreparationViewModel.QuestionResponse;
                    existingSurveyPrep.ModifiedBy = Convert.ToInt32(loggedInuserId);
                    existingSurveyPrep.ModifiedDate = DateTime.Now;
                    _unitOfWork.SurveyPreparationRepository.Update(existingSurveyPrep);

                    #endregion

                    #region Audit Log - Delivery Preparation Details Updated

                    AuditLogSurvey(Messages.ADL_DeliveryPreparationDetailsUpdated.Replace("#CUSTOMER_NAME#", customer.Name), centreId, userRole, loggedInuserId);

                    #endregion
                }
                else
                {
                    #region Delivery Preparation Details Added

                    surveyPreparation = new SurveyPreparation();
                    surveyPreparation.Delivery = delivery;
                    surveyPreparation.QuestionResponse = surveyPreparationViewModel.QuestionResponse;
                    surveyPreparation.IsActive = true;
                    surveyPreparation.CreatedBy = Convert.ToInt32(loggedInuserId);
                    surveyPreparation.CreatedDate = DateTime.Now;
                    _unitOfWork.SurveyPreparationRepository.Add(surveyPreparation);

                    #endregion

                    #region Audit Log - Delivery Preparation Details Added & Info Sheet Added

                    AuditLogSurvey(Messages.ADL_DeliveryPreparationDetailsAdded.Replace("#CUSTOMER_NAME#", customer.Name), centreId, userRole, loggedInuserId);
                    AuditLogSurvey(Messages.ADL_InfoSheetAdded.Replace("#CUSTOMER_NAME#", customer.Name), centreId, userRole, loggedInuserId);

                    #endregion
                }
                if (!delivery.SkipSurvey)
                {
                    AddCustomerSurvey(surveyPreparationViewModel, loggedInuserId, centreId, userRole);
                    
                }
            }
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Update status of customer survey sent or not
        public async Task<int> UpdateCustomerSurveyStatus(long deliveryId, long loggedInuserId, long centreId, string userRole)
        {
            var delivery = await _unitOfWork.DeliveryRepository.FirstOrDefault(p => p.Id == deliveryId);
            if (delivery != null && delivery.IsSurveySent == false)
            {
                delivery.IsSurveySent = true;
                delivery.ModifiedBy = Convert.ToInt32(loggedInuserId);
                delivery.ModifiedDate = DateTime.Now;
                _unitOfWork.DeliveryRepository.Update(delivery);
            }
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region To get reponse json for info sheet
        public async Task<InfoSheetViewModel> GetInfoSheetData(int deliveryId, int customerId)
        {
            InfoSheetViewModel infoSheetData = null;
            var surveyPreparation = await _unitOfWork.SurveyPreparationRepository.FirstOrDefault(p => p.Delivery.Id == deliveryId);
            var customerSurvey = await _unitOfWork.CustomerSurveyRepository.FirstOrDefault(p => p.Delivery.Id == deliveryId && p.Customer.Id == customerId);
            if (surveyPreparation != null)
            {
                infoSheetData = new InfoSheetViewModel();
                infoSheetData.SurveyPreparationJson = surveyPreparation.QuestionResponse;
                infoSheetData.CustomerSurveyJson = customerSurvey?.QuestionResponse ?? String.Empty;
                infoSheetData.IsSurveySubmitted = customerSurvey?.IsResponseSubmitted ?? false;
            }
            return infoSheetData;
        }

        #endregion

        #region Get question json for delivery preparation
        public async Task<SurveyPreparationViewModel> GetDeliveryPreparation(int deliveryId)
        {
            var existingSurveyPrep = await _unitOfWork.SurveyPreparationRepository.FirstOrDefault(p => p.Delivery.Id == deliveryId);
            var question = _unitOfWork.QuestionTypeRepository.Find(p => p.Id == (Int32)Enums.QuestionType.DeliveryPreparation);
            SurveyPreparationViewModel surveyPreparation = new SurveyPreparationViewModel();
            if (existingSurveyPrep != null)
            {
                surveyPreparation.Id = existingSurveyPrep.Id;
                surveyPreparation.QuestionResponse = existingSurveyPrep.QuestionResponse;
            }
            else
            {
                surveyPreparation.QuestionResponse = question?.QuestionJson;
            }
            return surveyPreparation;
        }

        #endregion

        #region Get question json for customer survey form
        public async Task<CustomerSurveyViewModel> GetCustomerSurveyJson(int deliveryId, int customerId)
        {
            var existingCustomerSurvey = await _unitOfWork.CustomerSurveyRepository.FirstOrDefault(p => p.Delivery.Id == deliveryId && p.Customer.Id == customerId);
            CustomerSurveyViewModel customerSurvey = new CustomerSurveyViewModel();
            if (existingCustomerSurvey != null)
            {
                customerSurvey.Id = existingCustomerSurvey.Id;
                customerSurvey.QuestionResponse = existingCustomerSurvey.QuestionResponse;
                customerSurvey.IsResponseSubmitted = existingCustomerSurvey.IsResponseSubmitted;
            }
            return customerSurvey;
        }

        #endregion

        #region Submit response json for customer survey form
        public async Task<int> SubmitSurveyResponse(CustomerSurveyViewModel customerSurveyViewModel, long loggedInuserId, long centreId, string userRole)
        {
            var existingCustomerSurvey = await _unitOfWork.CustomerSurveyRepository.FirstOrDefault(p => p.Id == customerSurveyViewModel.Id);
            
            if (existingCustomerSurvey != null)
            {
                #region Survey Response Submitted

                existingCustomerSurvey.QuestionResponse = customerSurveyViewModel.QuestionResponse;
                existingCustomerSurvey.IsResponseSubmitted = true;
                existingCustomerSurvey.ModifiedBy = Convert.ToInt32(loggedInuserId);
                existingCustomerSurvey.ModifiedDate = DateTime.Now;
                _unitOfWork.CustomerSurveyRepository.Update(existingCustomerSurvey);

                #endregion

                #region Audit Log for Info Sheet Updated & Customer Survey Completed

                AuditLogSurvey(Messages.ADL_InfoSheetUpdated.Replace("#CUSTOMER_NAME#", customerSurveyViewModel.CustomerName), centreId, userRole, loggedInuserId);

                AuditLogSurvey(Messages.ADL_CustomerSurveyCompleted.Replace("#CUSTOMER_NAME#", customerSurveyViewModel.CustomerName), centreId, userRole, loggedInuserId);

                #endregion

            }
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region To get pre delivery check list question json
        public async Task<PreDeliveryCheckListModel> GetPreDeliveryCheckList(int deliveryId)
        {
            var existingCheckList = await _unitOfWork.PreDeliveryChecklistRepository.FirstOrDefault(p => p.Delivery.Id == deliveryId);
            var deliveryEntity = await _unitOfWork.DeliveryRepository.FirstOrDefault(p => p.Id == deliveryId);
            var question = _unitOfWork.QuestionTypeRepository.Find(p => p.Id == (Int32)Enums.QuestionType.PreDeliveryCheckList);
            PreDeliveryCheckListModel preDeliveryList = new PreDeliveryCheckListModel();
            if (deliveryEntity != null)
            {
                preDeliveryList.DeliveryId = deliveryEntity.Id;
                if (existingCheckList != null)
                {
                    preDeliveryList.Id = existingCheckList.Id;
                    preDeliveryList.DeliveryId = existingCheckList.Delivery.Id;
                    preDeliveryList.QuestionResponse = existingCheckList.QuestionResponse;
                }
                else
                {
                    preDeliveryList.QuestionResponse = question?.QuestionJson;
                }
            }
            return preDeliveryList;
        }

        #endregion

        #region To submit predelivery check list json response
        public async Task<int> AddEditPreDeliveryCheckList(PreDeliveryCheckListModel preDeliveryChecklistModel, long loggedInuserId, long centreId, string userRole)
        {
            var existingCheckList = await _unitOfWork.PreDeliveryChecklistRepository.FirstOrDefault(p => p.Delivery.Id == preDeliveryChecklistModel.DeliveryId);
            
            var deliveryEntity = await _unitOfWork.DeliveryRepository.FirstOrDefault(p => p.Id == preDeliveryChecklistModel.DeliveryId);

            var deliveryViewModel = await _deliveryComponent.GetDeliveryById(centreId, preDeliveryChecklistModel.DeliveryId);

            if (deliveryEntity != null && deliveryViewModel != null)
            {
                if (existingCheckList != null)
                {
                    #region Pre Delivery Check List Updated

                    if (existingCheckList.QuestionResponse != preDeliveryChecklistModel.QuestionResponse)
                    {
                        AuditLogSurvey(Messages.ADL_PreDeliveryCheckListUpdated.Replace("#CUSTOMER_NAME#", deliveryViewModel.CustomerName), centreId, userRole, loggedInuserId);
                    }

                    existingCheckList.QuestionResponse = preDeliveryChecklistModel.QuestionResponse;
                    existingCheckList.ModifiedBy = Convert.ToInt32(loggedInuserId);
                    existingCheckList.ModifiedDate = DateTime.Now;
                    _unitOfWork.PreDeliveryChecklistRepository.Update(existingCheckList);

                    #endregion

                    
                }
                else
                {
                    #region Pre Delivery Check List Added

                    PreDeliveryCheckList preDeliveryCheckList = new PreDeliveryCheckList();
                    preDeliveryCheckList.Delivery = deliveryEntity;
                    preDeliveryCheckList.QuestionResponse = preDeliveryChecklistModel.QuestionResponse;
                    preDeliveryCheckList.IsActive = true;
                    preDeliveryCheckList.CreatedBy = Convert.ToInt32(loggedInuserId);
                    preDeliveryCheckList.CreatedDate = DateTime.Now;
                    _unitOfWork.PreDeliveryChecklistRepository.Add(preDeliveryCheckList);

                    #endregion

                    AuditLogSurvey(Messages.ADL_PreDeliveryCheckListAdded.Replace("#CUSTOMER_NAME#", deliveryViewModel.CustomerName), centreId, userRole, loggedInuserId);
                }
            }
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Get delivery checklist based on deliveryId
        public async Task<DeliveryCheckListViewModel> GetDeliveryCheckList(int deliveryId)
        {
            DeliveryCheckListViewModel deliveryCheck = new DeliveryCheckListViewModel();

            var deliveryCheckList = await _unitOfWork.DeliveryCheckListRepository.FirstOrDefault(p => p.Delivery.Id == deliveryId);
            if (deliveryCheckList != null)
            {
                deliveryCheck.Id = deliveryCheckList.Id;
                deliveryCheck.DeliveryId = deliveryId;
                deliveryCheck.QuestionResponse = await GetUpdatedJson(deliveryCheckList.QuestionResponse, deliveryCheckList.Id);
            }
            else
            {
                var surveyPreparation = await _unitOfWork.SurveyPreparationRepository.FirstOrDefault(p => p.Delivery.Id == deliveryId);
                var customerSurvey = await _unitOfWork.CustomerSurveyRepository.FirstOrDefault(p => p.Delivery.Id == deliveryId && p.IsResponseSubmitted == true);

                var deliveryCheckListJson = await GetCalculatedTime(surveyPreparation, customerSurvey);
                deliveryCheck.DeliveryId = deliveryId;
                deliveryCheck.QuestionResponse = deliveryCheckListJson;
            }
            return deliveryCheck;
        }

        #endregion

        #region To add edit delivery checklist
        public async Task<int> AddEditDeliveryCheckList(DeliveryCheckListViewModel deliveryCheckListViewModel, long loggedInuserId, long centreId, string userRole)
        {
            var deliveryViewModel = await _deliveryComponent.GetDeliveryById(centreId, deliveryCheckListViewModel.DeliveryId);

            var existingChecList = await _unitOfWork.DeliveryCheckListRepository.FirstOrDefault(p => p.Id == deliveryCheckListViewModel.Id);
            
            var deliveryEntity = await _unitOfWork.DeliveryRepository.FirstOrDefault(p => p.Id == deliveryCheckListViewModel.DeliveryId);

            if (deliveryEntity != null && deliveryViewModel != null)
            {
                List<DeliveryCheckListJsonModel> deliveryCheckListDeserialized = null;
                List<DeliveryCheckListJsonModel> existingChecListDeserialized = null;

                deliveryCheckListDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(deliveryCheckListViewModel.QuestionResponse);
                

                var deliveryCheckListFollowUpList = deliveryCheckListDeserialized.SelectMany(x => x.Fields).SelectMany(v => v.Questions).Select(x => x.MoveToFollowUp).ToList();
                var deliveryPendingList = deliveryCheckListDeserialized.SelectMany(x => x.Fields).SelectMany(v => v.Questions).Select(x => x.Answer).ToList();

                var delFollowCount = deliveryCheckListFollowUpList.Where(x => x == true).ToList().Count;
                var delPendingCount = deliveryPendingList.Where(x => x == null || x.Equals("false", StringComparison.OrdinalIgnoreCase)).ToList().Count;

                if (existingChecList != null)
                {
                    existingChecListDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(existingChecList.QuestionResponse);

                    var existingChecListFollowUpList = existingChecListDeserialized.SelectMany(x => x.Fields).SelectMany(v => v.Questions).Select(x => x.MoveToFollowUp).ToList();
                    var existingChecListPendingList = existingChecListDeserialized.SelectMany(x => x.Fields).SelectMany(v => v.Questions).Select(x => x.Answer).ToList();
                    
                    var exFollowCount = existingChecListFollowUpList.Where(x => x == true).ToList().Count;
                    var exPendingCount = existingChecListPendingList.Where(x => x == null || x.Equals("false", StringComparison.OrdinalIgnoreCase)).ToList().Count;


                    if (delFollowCount != exFollowCount || delPendingCount != exPendingCount)
                    {
                        AuditLogSurvey(Messages.ADL_DeliveryCheckListUpdated.Replace("#CUSTOMER_NAME#", deliveryViewModel.CustomerName), centreId, userRole, loggedInuserId);
                    }


                    if (delFollowCount != exFollowCount)
                    {
                        AuditLogSurvey(Messages.ADL_CustomerFollowupDocsUpdated.Replace("#CUSTOMER_NAME#", deliveryViewModel.CustomerName), centreId, userRole, loggedInuserId);
                    }

                    #region Delivery Check List Updated

                    existingChecList.QuestionResponse = deliveryCheckListViewModel.QuestionResponse;
                    existingChecList.ModifiedBy = Convert.ToInt32(loggedInuserId);
                    existingChecList.ModifiedDate = DateTime.Now;
                    _unitOfWork.DeliveryCheckListRepository.Update(existingChecList);

                    #endregion

                }
                else
                {
                    #region Delivery Check List Added

                    DeliveryCheckList deliveryCheckList = new DeliveryCheckList();
                    deliveryCheckList.Delivery = deliveryEntity;
                    deliveryCheckList.IsActive = true;
                    deliveryCheckList.CreatedBy = Convert.ToInt32(loggedInuserId);
                    deliveryCheckList.CreatedDate = DateTime.Now;
                    deliveryCheckList.QuestionResponse = deliveryCheckListViewModel.QuestionResponse;
                    _unitOfWork.DeliveryCheckListRepository.Add(deliveryCheckList);

                    #endregion

                    AuditLogSurvey(Messages.ADL_DeliveryCheckListAdded.Replace("#CUSTOMER_NAME#", deliveryViewModel.CustomerName), centreId, userRole, loggedInuserId);

                    if (deliveryCheckListFollowUpList.Any(x => x == true))
                    {
                        AuditLogSurvey(Messages.ADL_CustomerFollowupDocsAdded.Replace("#CUSTOMER_NAME#", deliveryViewModel.CustomerName), centreId, userRole, loggedInuserId);
                    }

                }

            }
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Private Methods

        #region To add customer survey json for  the delivery
        private void AddCustomerSurvey(SurveyPreparationViewModel surveyPreparationViewModel, long loggedInuserId, long centreId, string userRole)
        {
            List<SurveyPreparationJsonModel> surveyDeserializedList = JsonConvert.DeserializeObject<List<SurveyPreparationJsonModel>>(surveyPreparationViewModel.QuestionResponse);
            var question = _unitOfWork.QuestionTypeRepository.Find(p => p.Id == (Int32)Enums.QuestionType.CustomerSurvey);
            List<CustomerSurveyJsonModel> customerSurveyList = JsonConvert.DeserializeObject<List<CustomerSurveyJsonModel>>(question.QuestionJson);
            if (!string.IsNullOrEmpty(surveyDeserializedList[0].Fields[0].Answer))
            {
                customerSurveyList[1].IsVisible = false;
            }
            if (surveyDeserializedList[1].Fields[3].Answer == "Not available" || surveyDeserializedList[1].Fields[3].Answer == "Not started")
            {
                CustomerSurveyJsonModel surveyQuestion = new CustomerSurveyJsonModel();
                surveyQuestion.Name = "chargerHome";
                surveyQuestion.IsInfoSheet = true;
                surveyQuestion.Question = "Do you have access to a charger at your home location?";
                surveyQuestion.IsVisible = true;
                surveyQuestion.Answer = null;
                surveyQuestion.Type = "radio";
                surveyQuestion.Options = new List<string>() { "Yes", "No" };
                customerSurveyList.Insert(customerSurveyList.Count() - 7, surveyQuestion);

                surveyQuestion = new CustomerSurveyJsonModel();
                surveyQuestion.Name = "porscheHome";
                surveyQuestion.IsInfoSheet = true;
                surveyQuestion.Question = "Were you aware of the complementary Porsche Home Check offer to assess your home charging options?";
                surveyQuestion.IsVisible = false;
                surveyQuestion.Answer = null;
                surveyQuestion.Type = "radio";
                surveyQuestion.Options = new List<string>() { "Yes", "No" };
                customerSurveyList.Insert(customerSurveyList.Count() - 7, surveyQuestion);

                surveyQuestion = new CustomerSurveyJsonModel();
                surveyQuestion.Name = "recieveInfoHome";
                surveyQuestion.IsInfoSheet = true;
                surveyQuestion.Question = "Would you like to receive more information on the Porsche Home Check?";
                surveyQuestion.IsVisible = false;
                surveyQuestion.Answer = null;
                surveyQuestion.Type = "radio";
                surveyQuestion.Options = new List<string>() { "Yes", "No" };
                customerSurveyList.Insert(customerSurveyList.Count() - 7, surveyQuestion);
            }
            if (surveyDeserializedList[1].Fields[3].Answer == "In progress" || surveyDeserializedList[1].Fields[3].Answer == "Completed")
            {
                CustomerSurveyJsonModel surveyQuestion = new CustomerSurveyJsonModel
                {
                    Name = "satisfiedHomeCheck",
                    IsInfoSheet = true,
                    Question = "How satisfied are you with the Porsche Home Check and charger installation process?",
                    IsVisible = true,
                    Answer = null,
                    Type = "slider",
                    Min = "1",
                    Max = "5",
                    Step = "1",
                    MaxLabel = "5 - Very satisfied",
                    MinLabel = "1 - Not at all satisfied"
                };
                customerSurveyList.Insert(customerSurveyList.Count() - 7, surveyQuestion);
            }

            var customerSurveyJson = JsonConvert.SerializeObject(customerSurveyList);
            var customer = _unitOfWork.CustomerRepository.Find(p => p.Id == surveyPreparationViewModel.CustomerId);
            var delivery = _unitOfWork.DeliveryRepository.Find(p => p.Id == surveyPreparationViewModel.DeliveryId);

            CustomerSurvey customerSurvey = new CustomerSurvey();
            customerSurvey.Customer = customer;
            customerSurvey.Delivery = delivery;
            customerSurvey.QuestionResponse = customerSurveyJson;
            customerSurvey.IsResponseSubmitted = false;
            customerSurvey.IsActive = true;
            customerSurvey.CreatedBy = Convert.ToInt32(loggedInuserId);
            customerSurvey.CreatedDate = DateTime.Now;
            _unitOfWork.CustomerSurveyRepository.Add(customerSurvey);
        }

        #endregion

        #region To get calculated time based on customer survey and delivery preparation
        private async Task<string> GetCalculatedTime(SurveyPreparation surveyPrep, CustomerSurvey customerSurvey)
        {
            double intro = Convert.ToDouble(_configuration.GetSection(Constants.Intro).Value);
            double interior = Convert.ToDouble(_configuration.GetSection(Constants.Interior).Value);
            double pcmSetup = Convert.ToDouble(_configuration.GetSection(Constants.PCMSetup).Value);
            double connect = Convert.ToDouble(_configuration.GetSection(Constants.PorscheConnect).Value);
            double exterior = Convert.ToDouble(_configuration.GetSection(Constants.Exterior).Value);
            double charging = Convert.ToDouble(_configuration.GetSection(Constants.Charging).Value);
            double administration = Convert.ToDouble(_configuration.GetSection(Constants.Administration).Value);
            double wrapup = Convert.ToDouble(_configuration.GetSection(Constants.Wrapup).Value);
            double percentLess, customIdealTime, customerAllocatedTime;


            List<SurveyPreparationJsonModel> surveyPrepDeserialized;
            if (surveyPrep != null)
            {
                surveyPrepDeserialized = JsonConvert.DeserializeObject<List<SurveyPreparationJsonModel>>(surveyPrep?.QuestionResponse);
                var otherVehicle = surveyPrepDeserialized.Find(x => x.Info == "General Information").Fields.Find(p => p.Name == Constants.OtherPorscheVehicle).Answer;
                if (!string.IsNullOrEmpty(otherVehicle))
                {
                    interior = (double)(interior - 0.2 * interior);
                    pcmSetup = (double)(pcmSetup - 0.1 * pcmSetup);
                }
            }

            List<CustomerSurveyJsonModel> customerSurveyDerserialized;
            if (customerSurvey != null)
            {
                customerSurveyDerserialized = JsonConvert.DeserializeObject<List<CustomerSurveyJsonModel>>(customerSurvey.QuestionResponse);
                customerAllocatedTime = Convert.ToInt32(customerSurveyDerserialized.Find(x => x.Name == Constants.CustomerTime).Answer);
                var electricVehicle = customerSurveyDerserialized.Find(x => x.Name == Constants.ElectricVehicle);
                if (electricVehicle != null)
                {
                    if (Convert.ToInt32(electricVehicle.Answer) == 4)
                    {
                        charging = (double)(charging - 0.2 * charging);
                    }
                    if (Convert.ToInt32(electricVehicle.Answer) == 5)
                    {
                        charging = (double)(charging - 0.5 * charging);
                    }
                }
                var technicalFeature = customerSurveyDerserialized.Find(x => x.Name == Constants.TechnicalFeature);
                if (technicalFeature != null)
                {
                    if (Convert.ToInt32(technicalFeature.Answer) == 4)
                    {
                        interior = (double)(interior - 0.2 * interior);
                        pcmSetup = (double)(pcmSetup - 0.2 * pcmSetup);
                    }
                    if (Convert.ToInt32(technicalFeature.Answer) == 5)
                    {
                        interior = (double)(interior - 0.5 * interior);
                        pcmSetup = (double)(pcmSetup - 0.5 * pcmSetup);
                    }
                }
                var porscheConnect = customerSurveyDerserialized.Find(x => x.Name == Constants.Connect);
                if (porscheConnect != null)
                {
                    if (Convert.ToInt32(porscheConnect.Answer) == 4)
                    {
                        pcmSetup = (double)(pcmSetup - 0.2 * pcmSetup);
                        connect = (double)(connect - 0.2 * connect);
                    }
                    if (Convert.ToInt32(porscheConnect.Answer) == 5)
                    {
                        pcmSetup = (double)(pcmSetup - 0.5 * pcmSetup);
                        connect = (double)(connect - 0.5 * connect);
                    }
                }
                var areasToFocus = customerSurveyDerserialized.Find(x => x.Name == Constants.AreasToFocus);
                if (areasToFocus != null)
                {
                    var areas = areasToFocus.Answer.Split("|");
                    foreach (var area in areas)
                    {
                        var areaToChange = area.Split(' ')[0];
                        if (areaToChange.Equals(_configuration.GetSection(Constants.Interior).Key, StringComparison.OrdinalIgnoreCase))
                        {
                            interior = (double)(interior + 0.1 * interior);
                        }
                        if (areaToChange.Equals(_configuration.GetSection(Constants.Exterior).Key, StringComparison.OrdinalIgnoreCase))
                        {
                            exterior = (double)(exterior + 0.1 * exterior);
                        }
                        if (areaToChange.Equals(_configuration.GetSection(Constants.Charging).Key, StringComparison.OrdinalIgnoreCase))
                        {
                            charging = (double)(charging + 0.1 * charging);
                        }
                        if (areaToChange.Equals("Connect", StringComparison.OrdinalIgnoreCase))
                        {
                            connect = (double)(connect + 0.1 * connect);
                        }
                    }

                }
                charging = Math.Round(charging);
                interior = Math.Round(interior);
                pcmSetup = Math.Round(pcmSetup);
                connect = Math.Round(connect);
                exterior = Math.Round(exterior);
                customIdealTime = intro + interior + pcmSetup + connect + exterior + charging + administration + wrapup;
                if (customIdealTime > customerAllocatedTime)
                {
                    percentLess = Math.Round(((customIdealTime - customerAllocatedTime) * 100) / customIdealTime);
                    intro = (double)(intro - (percentLess / 100) * intro);
                    interior = (double)(interior - (percentLess / 100) * interior);
                    pcmSetup = (double)(pcmSetup - (percentLess / 100) * pcmSetup);
                    connect = (double)(connect - (percentLess / 100) * connect);
                    exterior = (double)(exterior - (percentLess / 100) * exterior);
                    charging = (double)(charging - (percentLess / 100) * charging);
                    administration = (double)(administration - (percentLess / 100) * administration);
                    wrapup = (double)(wrapup - (percentLess / 100) * wrapup);
                }
            }
            else
            {
                interior = Math.Round(interior);
                pcmSetup = Math.Round(pcmSetup);
            }
            var deliveryCheckList = await _unitOfWork.QuestionTypeRepository.FirstOrDefault(p => p.Id == (Int32)Enums.QuestionType.DeliveryCheckList);
            List<DeliveryCheckListJsonModel> deliveryCheckDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(deliveryCheckList.QuestionJson);
            if (deliveryCheckDeserialized != null)
            {
                try
                {
                    deliveryCheckDeserialized.Find(x => x.Name.Equals(_configuration.GetSection(Constants.Exterior).Key, StringComparison.OrdinalIgnoreCase)).AllocatedTime = String.Format("{0:0.00}", exterior).Replace(".", ":");
                    deliveryCheckDeserialized.Find(x => x.Name.Equals(_configuration.GetSection(Constants.Charging).Key, StringComparison.OrdinalIgnoreCase)).AllocatedTime = String.Format("{0:0.00}", charging).Replace(".", ":");
                    deliveryCheckDeserialized.Find(x => x.Name.Equals(_configuration.GetSection(Constants.Interior).Key, StringComparison.OrdinalIgnoreCase)).AllocatedTime = String.Format("{0:0.00}", interior).Replace(".", ":");
                    deliveryCheckDeserialized.Find(x => x.Name.Equals(_configuration.GetSection(Constants.PCMSetup).Key, StringComparison.OrdinalIgnoreCase)).AllocatedTime = String.Format("{0:0.00}", pcmSetup).Replace(".", ":");
                    deliveryCheckDeserialized.Find(x => x.Name.Equals("ConnectFeatures", StringComparison.OrdinalIgnoreCase)).AllocatedTime = String.Format("{0:0.00}", connect).Replace(".", ":");
                    deliveryCheckDeserialized.Find(x => x.Name.Equals("administration", StringComparison.OrdinalIgnoreCase)).AllocatedTime = String.Format("{0:0.00}", administration).Replace(".", ":");
                    deliveryCheckDeserialized.Find(x => x.Name.Equals("deliveryExperience", StringComparison.OrdinalIgnoreCase)).AllocatedTime = String.Format("{0:0.00}", wrapup).Replace(".", ":");
                }
                catch(Exception ex)
                {

                }
                
            }
            return JsonConvert.SerializeObject(deliveryCheckDeserialized);
        }

        #endregion

        #region Get Updated JSON
        private async Task<string> GetUpdatedJson(string questionJson, long checklistId)
        {
            Question question = null;
            var masterCheckList = await _unitOfWork.QuestionTypeRepository.FirstOrDefault(p => p.Id == (Int32)Enums.QuestionType.DeliveryCheckList);
            var deliveryCheckList = await _unitOfWork.DeliveryCheckListRepository.FirstOrDefault(p => p.Id == checklistId);
            List<DeliveryCheckListJsonModel> masterCheckDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(masterCheckList.QuestionJson);
            List<DeliveryCheckListJsonModel> deliveryCheckDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(questionJson);
            List<Document> masterDocList = new List<Document>();
            List<Document> deliveryDocList = new List<Document>();
            List<LinkViewModel> masterLinkList = new List<LinkViewModel>();
            List<LinkViewModel> deliveryLinkList = new List<LinkViewModel>();
            if (masterCheckDeserialized != null && deliveryCheckDeserialized != null)
            {
                foreach (var msection in masterCheckDeserialized)
                {
                    foreach (var field in msection.Fields)
                    {
                        var documentList = field.Questions.FindAll(p => p.Documents.Count > 0).Find(c => c.Documents.Count > 0)?.Documents;
                        var linkList = field.Questions.FindAll(p => p.Links.Count > 0).Find(c => c.Links.Count > 0)?.Links;
                        if (documentList != null && documentList.Count > 0)
                            masterDocList.AddRange(documentList);

                        if (linkList != null && linkList.Count > 0)
                            masterLinkList.AddRange(linkList);
                    }

                    foreach (var field in deliveryCheckDeserialized.Find(x => x.Name == msection.Name).Fields)
                    {
                        var documentList = field.Questions.FindAll(p => p.Documents.Count > 0).Find(c => c.Documents.Count > 0)?.Documents;
                        var linkList = field.Questions.FindAll(p => p.Links.Count > 0).Find(c => c.Links.Count > 0)?.Links;
                        if (documentList != null && documentList.Count > 0)
                            deliveryDocList.AddRange(documentList);

                        if (linkList != null && linkList.Count > 0)
                            deliveryLinkList.AddRange(linkList);
                    }

                    var firstNotSecondDoc = masterDocList.Where(x => !deliveryDocList.Any(z => z.Id == x.Id)).ToList();
                    var secondNotFirstDoc = deliveryDocList.Where(x => !masterDocList.Any(z => z.Id == x.Id)).ToList();

                    var firstNotSecondLink = masterLinkList.Where(x => !deliveryLinkList.Any(z => z.Id == x.Id)).ToList();
                    var secondNotFirstLink = deliveryLinkList.Where(x => !masterLinkList.Any(z => z.Id == x.Id)).ToList();

                    if (firstNotSecondDoc.Count() > 0 || firstNotSecondLink.Count() > 0)
                    {
                        foreach (var document in firstNotSecondDoc)
                        {
                            var fields = document.FileSubPath.Split('/');
                            if (fields.Length == 3)
                            {
                                question = deliveryCheckDeserialized.Find(x => x.Name == fields[^3]).Fields.Find(p => p.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                            }
                            else
                            {
                                question = deliveryCheckDeserialized.Find(x => x.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                            }
                            if (question != null && question.Documents.Find(x => x.Id == document.Id) == null)
                            {
                                question.Documents.Add(document);
                            }
                        }
                        foreach (var link in firstNotSecondLink)
                        {
                            var fields = link.Path.Split('/');
                            if (fields.Length == 3)
                            {
                                question = deliveryCheckDeserialized.Find(x => x.Name == fields[^3]).Fields.Find(p => p.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                            }
                            else
                            {
                                question = deliveryCheckDeserialized.Find(x => x.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                            }
                            if (question != null && question.Links.Find(x => x.Id == link.Id) == null)
                            {
                                question.Links.Add(link);
                            }
                        }

                    }
                    if (secondNotFirstDoc.Count() > 0 || secondNotFirstLink.Count() > 0)
                    {
                        foreach (var document in secondNotFirstDoc)
                        {
                            var fields = document.FileSubPath.Split('/');
                            if (fields.Length == 3)
                            {
                                question = deliveryCheckDeserialized.Find(x => x.Name == fields[^3]).Fields.Find(p => p.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                            }
                            else
                            {
                                question = deliveryCheckDeserialized.Find(x => x.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                            }
                            if (question != null)
                            {
                                question.Documents.RemoveAll(p => p.Id == document.Id);
                            }
                        }
                        foreach (var link in secondNotFirstLink)
                        {
                            var fields = link.Path.Split('/');
                            if (fields.Length == 3)
                            {
                                question = deliveryCheckDeserialized.Find(x => x.Name == fields[^3]).Fields.Find(p => p.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                            }
                            else
                            {
                                question = deliveryCheckDeserialized.Find(x => x.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                            }
                            if (question != null)
                            {
                                question.Links.RemoveAll(p => p.Id == link.Id);
                            }
                        }
                    }

                }

                deliveryCheckList.QuestionResponse = JsonConvert.SerializeObject(deliveryCheckDeserialized);
                _unitOfWork.DeliveryCheckListRepository.Update(deliveryCheckList);
                _unitOfWork.SaveChanges();
                return JsonConvert.SerializeObject(deliveryCheckDeserialized);
            }

            return "";
        }

        #endregion

        #region Add Audit Log

        private void AuditLogSurvey(string eventName, long centreId, string roleName, long userId)
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

        #region Download the file from S3 and send file
        public async Task<MemoryStream> GetDocumentFromS3(string key)
        {
            MemoryStream ms = null;

            GetObjectRequest getObjectRequest = new GetObjectRequest
            {
                BucketName = _configuration.GetSection(Constants.BucketName).Value,
                Key = key
            };

            using (var response = await _s3Client.GetObjectAsync(getObjectRequest))
            {
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    using (ms = new MemoryStream())
                    {
                        await response.ResponseStream.CopyToAsync(ms);
                    }
                }
            }

            if (ms is null || ms.ToArray().Length < 1)
                throw new FileNotFoundException(string.Format("The S3 Object '{0}' is not found", key));

            return ms;

        }

        #endregion

        #region Get Conetent from S3 Object
        public async Task<string> GetContentFromS3Object(string key)
        {
            string content = string.Empty;

            GetObjectRequest getObjectRequest = new GetObjectRequest
            {
                BucketName = _configuration.GetSection(Constants.BucketName).Value,
                Key = key
            };

            using (var response = await _s3Client.GetObjectAsync(getObjectRequest))
            {
                StreamReader reader = new StreamReader(response.ResponseStream);
                content = reader.ReadToEnd();
            }

            return content;
        }

        #endregion

        #endregion

        #endregion
    }
}
