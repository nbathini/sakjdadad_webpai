using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheDataAccess.Repositories;
using PorscheUtilities.HelperClass;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PorscheComponent.Component
{
    public class CarModelComponent : ICarModelComponent
    {
        #region Private Variables
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IAmazonS3 _s3Client;

        #endregion

        #region Constructor
        public CarModelComponent(IUnitOfWork unitOfWork, IConfiguration configuration, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _s3Client = s3Client;
        }

        #endregion

        #region Methods

        #region Get Car Models
        public async Task<PagedList<CarViewModel>> GetCarModels(PagingParameter pagingParameter)
        {
            var carModels = await _unitOfWork.CarModelRepository.GetAllAsync();

            var carModelList = (from carmodel in carModels //where carmodel.IsActive == true
                                select new CarViewModel
                                {
                                    Id = carmodel.Id,
                                    Name = carmodel.Name,
                                    ImagePath = carmodel.ImagePath,
                                    Description = carmodel.Description,
                                    IsActive = carmodel.IsActive,
                                    CreatedBy = carmodel.CreatedBy,
                                    CreatedDate = carmodel.CreatedDate,
                                    ModifiedBy = carmodel.ModifiedBy,
                                    ModifiedDate = carmodel.ModifiedDate
                                }
                                ).OrderByDescending(x => x.Id).ToList();
            

            if (!string.IsNullOrWhiteSpace(pagingParameter.Search))
            {
                SearchRecords(ref carModelList, pagingParameter.Search);
            }
            return PagedList<CarViewModel>.ToPagedList(carModelList, pagingParameter.PageNumber, pagingParameter.PageSize);
        }

        #endregion

        #region Get Car Model By Car Model ID
        public async Task<CarViewModel> GetCarModelById(long carModelId)
        {
            CarViewModel carViewModel = null;
            var carModel = await _unitOfWork.CarModelRepository.FirstOrDefault(p => p.Id == carModelId);
            if (carModel != null)
            {
                carViewModel = new CarViewModel();
                carViewModel.Id = carModel.Id;
                carViewModel.Name = carModel.Name;
                carViewModel.ImagePath = carModel.ImagePath;
                carViewModel.Description = carModel.Description;
            }
            return carViewModel;
        }

        #endregion

        #region Get Car Model By Name
        public async Task<CarViewModel> GetCarModelByName(string carModelName)
        {
            CarViewModel carViewModel = null;
            var carModel = await _unitOfWork.CarModelRepository.FirstOrDefault(p => p.Name.Trim().ToLower() == carModelName.Trim().ToLower());
            if (carModel != null)
            {
                carViewModel = new CarViewModel();
                carViewModel.Id = carModel.Id;
                carViewModel.Name = carModel.Name;
                carViewModel.ImagePath = carModel.ImagePath;
                carViewModel.Description = carModel.Description;
            }
            return carViewModel;
        }

        #endregion

        #region Check Car Model Name Existed For Other Car Model
        public async Task<CarViewModel> GetCarModelByIdName(long carModelId, string carModelName)
        {
            CarViewModel carViewModel = null;

            var carModel = await _unitOfWork.CarModelRepository.FirstOrDefault(p => p.Id != (Int32)carModelId && p.Name.Trim().ToLower() == carModelName.Trim().ToLower());
            if (carModel != null)
            {
                carViewModel = new CarViewModel();
                carViewModel.Id = carModel.Id;
                carViewModel.Name = carModel.Name;
                carViewModel.ImagePath = carModel.ImagePath;
                carViewModel.Description = carModel.Description;
            }
            return carViewModel;
        }

        #endregion

        #region Add/Edit Car Model
        public async Task<int> AddEditCarModel(CarViewModel carViewModel, string roleName, long? userId)
        {
            CarModel carModelEntity = null;
            if (carViewModel.Id > 0)
            {
                carModelEntity = _unitOfWork.CarModelRepository.Find(p => p.Id == carViewModel.Id);
                if (carModelEntity != null)
                {
                    carModelEntity.Name = carViewModel.Name;
                    carModelEntity.ImagePath = carViewModel.ImagePath;
                    carModelEntity.Description = carViewModel.Description;
                    carModelEntity.ModifiedBy = Convert.ToInt32(userId);
                    carModelEntity.ModifiedDate = DateTime.Now;
                    _unitOfWork.CarModelRepository.Update(carModelEntity);

                    AuditLogCarModel(Messages.ADL_CarModelUpdate.Replace("#CarModel#", carViewModel.Name), 0, roleName, userId.Value);
                }
            }
            else
            {
                carModelEntity = new CarModel();
                carModelEntity.Name = carViewModel.Name;
                carModelEntity.ImagePath = carViewModel.ImagePath;
                carModelEntity.Description = carViewModel.Description;
                carModelEntity.IsActive = true;
                carModelEntity.CreatedBy = Convert.ToInt32(userId);
                carModelEntity.CreatedDate = DateTime.Now;
                _unitOfWork.CarModelRepository.Add(carModelEntity);

                AuditLogCarModel(Messages.ADL_CarModelCreate.Replace("#CarModel#", carViewModel.Name), 0, roleName, userId.Value);
            }

            
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Delete Car Model
        public async Task<int> DeleteCarModel(int carModelId, bool isActive, string roleName, long userId)
        {
            var carModelEntity = _unitOfWork.CarModelRepository.Find(p => p.Id == carModelId);

            if (carModelEntity != null)
            {
                carModelEntity.IsActive = isActive;
                carModelEntity.ModifiedBy = Convert.ToInt32(userId);
                carModelEntity.ModifiedDate = DateTime.Now;
                _unitOfWork.CarModelRepository.Update(carModelEntity);

                if (isActive)
                {
                    AuditLogCarModel(Messages.ADL_CarModelActive.Replace("#CarModel#", carModelEntity.Name), 0, roleName, userId);
                }
                else
                {
                    AuditLogCarModel(Messages.ADL_CarModelInactive.Replace("#CarModel#", carModelEntity.Name), 0, roleName, userId);
                }                    

                return await _unitOfWork.SaveChangesAsync();

            }
            return 0;
        }

        #endregion

        #region Car Models Search Functionality 
        private void SearchRecords(ref List<CarViewModel> carModelList, string searchString)
        {
            if (!carModelList.Any() || string.IsNullOrWhiteSpace(searchString))
                return;

            carModelList = carModelList.Where(o => o.Name.ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                               || o.ImagePath.ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                               || o.Description.ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())).ToList();
        }

        #endregion

        #region Upload Images to AWS S3 Bucket
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

        #region Add Audit Log For Car Model

        private void AuditLogCarModel(string eventName, long centreId, string roleName, long userId)
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
