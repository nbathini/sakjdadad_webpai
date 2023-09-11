using PorscheUtilities.Models;

namespace PorscheComponent.Interface
{
    public interface IDeliveryComponent
    {
        Task<List<DeliveryViewModel>> GetDelivery(long centreId, DateTime startDate, DateTime endDate);
        Task<int> AddDelivery(long centreId, DeliveryViewModel deliveryViewModel, string userRole, long loggedInuserId);
        Task<int> EditDelivery(long centreId, DeliveryViewModel deliveryViewModel, long? userId, string userRole, string userRoleId);
        Task<List<CarViewModel>> GetCarModels();
        Task<DeliveryViewModel> GetDeliveryById(long centreId, long deliveryId);
        Task<List<UserInfoViewModel>> GetConsultantList(long centreId);
        Task<List<UserInfoViewModel>> GetPorscheProList(long centreId);
        Task<List<UserInfoViewModel>> GetAdvisorList(long centreId);
        Task<int> DeleteDelivery(long centreId, int deliveryId, long userId, string userRole, string userRoleId);
        Task<List<DeliveryTypeViewModel>> GetDeliveryType();
        Task<string> ImageUrlToBase64(string imageUrl);
    }
}
