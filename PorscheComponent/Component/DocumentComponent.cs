using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheDataAccess.Repositories;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;
using System.Text;

namespace PorscheComponent.Component
{
    public class DocumentComponent : IDocumentComponent
    {
        #region Private Variables

        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IAmazonS3 _s3Client;

        #endregion

        #region Constructor
        public DocumentComponent(IUnitOfWork unitOfWork, IConfiguration configuration, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _s3Client = s3Client;
        }
        #endregion

        #region Methods

        #region To get file path based on file id
        public async Task<string> GetFilePath(long fileId)
        {
            var file = await _unitOfWork.FileDetailRepository.FirstOrDefault(p => p.Id == fileId);
            if (file != null)
            {
                var section = file.Section;
                var subSection = file.SubSection;
                var question = file.Question;
                var fileName = file.FileName;
                StringBuilder builder = new StringBuilder();
                //builder.Append(_configuration.GetSection(Constants.SiteUrlApi).Value);
                builder.Append(_configuration.GetSection(Constants.CloudFrontURL).Value);
                builder.Append(Constants.Documents);
                builder.Append("/");
                //builder.Append(section);
                //builder.Append("/");
                //builder.Append(subSection);
                //builder.Append("/");
                //builder.Append(question);
                //builder.Append("/");
                builder.Append(fileName);
                return Convert.ToString(builder);
            }
            return "";
        }

        #endregion

        #region Upload documents in db and updated json
        public async Task<int> UploadDocuments(List<Document> documentList, long loggedInuserId, long centreId, string userRole)
        {
            var deliveryCheckList = await _unitOfWork.QuestionTypeRepository.FirstOrDefault(p => p.Id == (Int32)Enums.QuestionType.DeliveryCheckList);
            List<DeliveryCheckListJsonModel> deliveryCheckDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(deliveryCheckList.QuestionJson);
            Question question = null;
            foreach (var document in documentList)
            {
                var fields = document.FileSubPath.Split('/');
                FileDetail file = new FileDetail();
                if (fields.Length == 3)
                {
                    file.Section = fields[^3];
                    file.SubSection = fields[^2];
                    file.Question = fields[^1];

                    question = deliveryCheckDeserialized.Find(x => x.Name == fields[^3]).Fields.Find(p => p.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                }
                if (fields.Length == 2)
                {
                    file.Section = fields[^2];
                    file.SubSection = fields[^1];
                    question = deliveryCheckDeserialized.Find(x => x.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                }

                if (question != null)
                {

                    #region Add Document Upload Details 

                    file.FileName = document.FileName;
                    file.FileSize = document.FileSize != null ? document.FileSize.Value : 0;
                    file.FilePath = document.FileFullPath;
                    file.FileType = document.FileType;
                    file.IsActive = true;
                    file.CreatedBy = Convert.ToInt32(loggedInuserId);
                    file.CreatedDate = DateTime.Now;                    
                    _unitOfWork.FileDetailRepository.Add(file);
                    _unitOfWork.SaveChanges();

                    document.Id = file.Id;
                    question.Documents.Add(document);

                    #endregion

                    #region Update Delivery Check List

                    var deliveryChecklistJson = JsonConvert.SerializeObject(deliveryCheckDeserialized);
                    deliveryCheckList.QuestionJson = deliveryChecklistJson;
                    deliveryCheckList.ModifiedBy = Convert.ToInt32(loggedInuserId);
                    deliveryCheckList.ModifiedDate = DateTime.Now;
                    _unitOfWork.QuestionTypeRepository.Update(deliveryCheckList);

                    AuditLogDocumentConfig(Messages.ADL_DocumentAdded.Replace("#DocName#", document.FileName), 0, userRole, loggedInuserId);

                    #endregion
                }
            }
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Delete document from db and json
        public async Task<int> DeleteDocument(long fileId, long loggedInuserId, long centreId, string userRole)
        {
            var file = await _unitOfWork.FileDetailRepository.FirstOrDefault(p => p.Id == fileId);
            Question question = null;
            if (file != null)
            {
                var filePath = file.FilePath;
                
                //if (File.Exists(filePath))
                //{
                //    File.Delete(filePath);
                    
                    var deliveryCheckList = await _unitOfWork.QuestionTypeRepository.FirstOrDefault(p => p.Id == (Int32)Enums.QuestionType.DeliveryCheckList);
                    
                    List<DeliveryCheckListJsonModel> deliveryCheckDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(deliveryCheckList.QuestionJson);
                    
                    if (file.Section != null && file.SubSection != null && file.Question != null)
                    {
                        question = deliveryCheckDeserialized.Find(x => x.Name.Equals(file.Section, StringComparison.OrdinalIgnoreCase)).Fields.Find(p => p.Name.Equals(file.SubSection, StringComparison.OrdinalIgnoreCase)).Questions.Find(c => c.Name.Equals(file.Question, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        question = deliveryCheckDeserialized.Find(x => x.Name.Equals(file.Section, StringComparison.OrdinalIgnoreCase)).Questions.Find(c => c.Name.Equals(file.SubSection, StringComparison.OrdinalIgnoreCase));
                    }
                    
                    if (question != null)
                    {
                        question.Documents.RemoveAll(p => p.Id == fileId);

                        #region Update Delivery Check List
                        
                        var deliveryChecklistJson = JsonConvert.SerializeObject(deliveryCheckDeserialized);
                        deliveryCheckList.QuestionJson = deliveryChecklistJson;
                        deliveryCheckList.ModifiedBy = Convert.ToInt32(loggedInuserId);
                        deliveryCheckList.ModifiedDate = DateTime.Now;
                        _unitOfWork.QuestionTypeRepository.Update(deliveryCheckList);
                        
                        #endregion

                        #region Inactivate/Delete File Details

                        file.IsActive = false;
                        file.ModifiedBy = Convert.ToInt32(loggedInuserId);
                        file.ModifiedDate = DateTime.Now;
                        _unitOfWork.FileDetailRepository.Update(file);

                    #endregion

                    AuditLogDocumentConfig(Messages.ADL_DocumentDeleted.Replace("#DocName#", file.FileName), 0, userRole, loggedInuserId);
                }
                //}
            }
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Get master delivery checklist json
        public async Task<string> GetCheckListJson()
        {
            return (await _unitOfWork.QuestionTypeRepository.FirstOrDefault(p => p.Id == (Int32)Enums.QuestionType.DeliveryCheckList)).QuestionJson;
        }

        #endregion

        #region Get document details based on id
        public async Task<Document> GetDocumentDetail(long fileId)
        {
            Document document = new Document();
            var documentEntity = await _unitOfWork.FileDetailRepository.FirstOrDefault(p => p.Id == fileId);
            if (documentEntity != null)
            {
                document.Id = documentEntity.Id;
                document.FileName = documentEntity.FileName;
                document.FileFullPath = documentEntity.FilePath;
                document.FileSize = documentEntity.FileSize;
                document.FileType = documentEntity.FileType;

            }
            return document;
        }

        #endregion

        #region Update json for documents send out
        public async Task<int> SendDocuments(SendDocumentViewModel sendDocumentViewModel, long loggedInuserId, long centreId, string userRole)
        {
            List<DeliveryCheckListJsonModel> deliveryCheckDeserialized = null;
            var existingCheckList = await _unitOfWork.DeliveryCheckListRepository.FirstOrDefault(p => p.Id == sendDocumentViewModel.CheckListId);
            var deliveryCheckList = await _unitOfWork.QuestionTypeRepository.FirstOrDefault(p => p.Id == (Int32)Enums.QuestionType.DeliveryCheckList);
            if (existingCheckList != null)
            {
                deliveryCheckDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(existingCheckList.QuestionResponse);
            }
            else
            {
                deliveryCheckDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(deliveryCheckList.QuestionJson);
            }
            if (sendDocumentViewModel.FileList != null && sendDocumentViewModel.FileList.Count > 0)
            {
                foreach (var document in sendDocumentViewModel.FileList)
                {
                    Document doc = null;
                    var file = await _unitOfWork.FileDetailRepository.FirstOrDefault(p => p.Id == document);
                    if (file.Section != null && file.SubSection != null && file.Question != null)
                    {
                        doc = deliveryCheckDeserialized.Find(x => x.Name.Equals(file.Section, StringComparison.OrdinalIgnoreCase)).Fields.Find(p => p.Name.Equals(file.SubSection, StringComparison.OrdinalIgnoreCase)).Questions.Find(c => c.Name.Equals(file.Question, StringComparison.OrdinalIgnoreCase)).Documents.Find(y => y.Id == document);
                    }
                    else
                    {
                        doc = deliveryCheckDeserialized.Find(x => x.Name.Equals(file.Section, StringComparison.OrdinalIgnoreCase)).Questions.Find(c => c.Name.Equals(file.SubSection, StringComparison.OrdinalIgnoreCase)).Documents.Find(y => y.Id == document);
                    }
                    if (doc != null)
                    {
                        doc.IsSent = true;
                        doc.SentOn = DateTime.Now;
                    }
                }
            }
            var deliveryChecklistJson = JsonConvert.SerializeObject(deliveryCheckDeserialized);

            if (existingCheckList != null)
            {
                #region Update Delivery Check List

                existingCheckList.QuestionResponse = deliveryChecklistJson;
                existingCheckList.ModifiedBy = Convert.ToInt32(loggedInuserId);
                existingCheckList.ModifiedDate = DateTime.Now;
                _unitOfWork.DeliveryCheckListRepository.Update(existingCheckList);

                #endregion
            }
            else
            {
                #region Add Delivery Check List

                DeliveryCheckList deliveryCheck = new DeliveryCheckList();
                deliveryCheck.Delivery = await _unitOfWork.DeliveryRepository.FirstOrDefault(p => p.Id == sendDocumentViewModel.DeliveryId);
                deliveryCheck.QuestionResponse = deliveryChecklistJson;
                deliveryCheck.IsActive = true;
                deliveryCheck.CreatedBy = Convert.ToInt32(loggedInuserId);
                deliveryCheck.CreatedDate = DateTime.Now;
                _unitOfWork.DeliveryCheckListRepository.Add(deliveryCheck);

                #endregion
            }

            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Upload Documents to AWS S3 Bucket
        public async Task<bool> UploadFileToS3(IFormFile file, string bucketName, string subFolderInBucket, string key)
        {
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

            if (!bucketExists) return false;

            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = string.IsNullOrEmpty(subFolderInBucket) ? key : $"{subFolderInBucket?.TrimEnd('/')}/{key}",
                InputStream = file.OpenReadStream(),
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AWSKMS,
                ServerSideEncryptionKeyManagementServiceKeyId = _configuration.GetSection(Constants.AWSKMSKeyId).Value
            };

            request.Metadata.Add("Content-Type", file.ContentType);

            await _s3Client.PutObjectAsync(request);

            return true;

        }

        #endregion

        #region Manage Check List
        public async Task<int> ManageCheckList(List<DeliveryCheckListJsonModel> deliveryCheckListJsonModelList, long loggedInuserId, string roleName)
        {
            var deliveryCheckList = await _unitOfWork.QuestionTypeRepository.FirstOrDefault(p => p.Id == (Int32)Enums.QuestionType.DeliveryCheckList);

            var deliveryChecklistJson = JsonConvert.SerializeObject(deliveryCheckListJsonModelList);
            deliveryCheckList.QuestionJson = deliveryChecklistJson;
            deliveryCheckList.ModifiedBy = Convert.ToInt32(loggedInuserId);
            deliveryCheckList.ModifiedDate = DateTime.Now;
            _unitOfWork.QuestionTypeRepository.Update(deliveryCheckList);

            AuditLogDocumentConfig(Messages.ADL_DocumentConfigUpdate, 0, roleName, loggedInuserId);

            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Add Audit Log Document Config

        private void AuditLogDocumentConfig(string eventName, long centreId, string roleName, long userId)
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
