using Microsoft.AspNetCore.Http;
using PorscheUtilities.HelperClass;
using PorscheUtilities.Models;

namespace PorscheComponent.Interface
{
    public interface ICarModelComponent
    {
        Task<PagedList<CarViewModel>> GetCarModels(PagingParameter pagingParameter);
        Task<CarViewModel> GetCarModelById(long carModelId);
        Task<CarViewModel> GetCarModelByName(string carModelName);
        Task<CarViewModel> GetCarModelByIdName(long carModelId, string carModelName);
        Task<int> DeleteCarModel(int carModelId, bool isActive, string roleName, long userId);
        Task<int> AddEditCarModel(CarViewModel carViewModel, string roleName, long? userId);
        Task<bool> UploadFileToS3(IFormFile file, string bucketName, string subFolderInBucket, string key);

    }
}
