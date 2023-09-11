using Microsoft.AspNetCore.Http;
using PorscheUtilities.Models;

namespace PorscheComponent.Interface
{
    public interface IDocumentComponent
    {
        Task<int> UploadDocuments(List<Document> documentList, long loggedInuserId, long centreId, string userRole);
        Task<string> GetFilePath(long fileId);
        Task<int> DeleteDocument(long fileId, long loggedInuserId, long centreId, string userRole);
        Task<string> GetCheckListJson();
        Task<Document> GetDocumentDetail(long fileId);
        Task<int> SendDocuments(SendDocumentViewModel sendDocumentViewModel, long loggedInuserId, long centreId, string userRole);
        Task<bool> UploadFileToS3(IFormFile file, string bucketName, string subFolderInBucket, string key);
        Task<int> ManageCheckList(List<DeliveryCheckListJsonModel> deliveryCheckListJsonModelList, long loggedInuserId, string roleName);
    }
}
