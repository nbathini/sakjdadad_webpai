using PorscheUtilities.Models;

namespace PorscheComponent.Interface
{
    public interface ISurveyComponent
    {
        Task<int> AddEditDeliveryPreparation(SurveyPreparationViewModel surveyPreparationViewModel, long loggedInuserId, long centreId, string userRole);
        Task<SurveyPreparationViewModel> GetDeliveryPreparation(int deliveryId);
        Task<InfoSheetViewModel> GetInfoSheetData(int deliveryId, int customerId);
        Task<CustomerSurveyViewModel> GetCustomerSurveyJson(int deliveryId, int customerId);
        Task<int> SubmitSurveyResponse(CustomerSurveyViewModel customerSurveyViewModel, long loggedInuserId, long centreId, string userRole);
        Task<PreDeliveryCheckListModel> GetPreDeliveryCheckList(int deliveryId);
        Task<int> AddEditPreDeliveryCheckList(PreDeliveryCheckListModel preDeliveryChecklistModel, long loggedInuserId, long centreId, string userRole);
        Task<DeliveryCheckListViewModel> GetDeliveryCheckList(int deliveryId);
        Task<int> AddEditDeliveryCheckList(DeliveryCheckListViewModel deliveryCheckListViewModel, long loggedInuserId, long centreId, string userRole);
        Task<int> UpdateCustomerSurveyStatus(long deliveryId, long loggedInuserId, long centreId, string userRole);
        Task<MemoryStream> GetDocumentFromS3(string key);
        Task<string> GetContentFromS3Object(string key);
    }
}
