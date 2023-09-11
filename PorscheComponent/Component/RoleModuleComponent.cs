using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheDataAccess.Repositories;
using PorscheUtilities.HelperClass;
using PorscheUtilities.Models;

namespace PorscheComponent.Component
{
    public class RoleModuleComponent : IRoleModuleComponent
    {
        #region Private Variables
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        #region Constructor
        public RoleModuleComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Methods

        #region Get Roles

        public async Task<PagedList<RoleModuleViewModel>> GetRoleModules(PagingParameter pagingParameter)
        {
            var roleModuleList = await _unitOfWork.RoleModuleRepository.GetAllAsync();
            var moduleList = await _unitOfWork.ModuleRepository.GetAllAsync();

            var roleModules = (from roleModule in roleModuleList
                                join module in moduleList on roleModule.Modules?.Id equals module.Id
                                select new RoleModuleViewModel
                                {
                                    Id = roleModule.Id,
                                    RoleName = roleModule.RoleName,
                                    RoleDescription = roleModule.RoleDescription,
                                    Modules = new ModulesViewModel { Id = module?.Id ?? 0, ModuleName = module?.ModuleName ?? String.Empty, IsActive= module?.IsActive ?? false, CreatedBy = module.CreatedBy ?? null, CreatedDate = module.CreatedDate ?? null, ModifiedBy= module.ModifiedBy ?? null, ModifiedDate=module.ModifiedDate ?? null },
                                    IsCreate = roleModule.IsCreate,
                                    IsEdit = roleModule.IsEdit,
                                    IsView = roleModule.IsView,
                                    IsDelete = roleModule.IsDelete,
                                    IsActive = roleModule.IsActive,
                                    CreatedBy = roleModule.CreatedBy,
                                    CreatedDate = roleModule.CreatedDate,
                                    ModifiedBy = roleModule.ModifiedBy,
                                    ModifiedDate = roleModule.ModifiedDate
                                }
                                ).OrderByDescending(x => x.Id).ToList();


            if (!string.IsNullOrWhiteSpace(pagingParameter.Search))
            {
                SearchRecords(ref roleModules, pagingParameter.Search);
            }
            return PagedList<RoleModuleViewModel>.ToPagedList(roleModules, pagingParameter.PageNumber, pagingParameter.PageSize);
        }

        #endregion

        #region Get Role By ID
        public async Task<RoleModuleViewModel> GetRoleModuleById(int roleId)
        {
            var roleModuleList = await _unitOfWork.RoleModuleRepository.GetAllAsync();
            var moduleList = await _unitOfWork.ModuleRepository.GetAllAsync();

            if (roleModuleList.Any() && moduleList.Any())
            {
                return (from roleModule in roleModuleList
                        join module in moduleList on roleModule.Modules?.Id equals module.Id
                        where roleModule.Id == roleId
                        select new RoleModuleViewModel
                        {
                            Id = roleModule.Id,
                            RoleName = roleModule.RoleName,
                            RoleDescription = roleModule.RoleDescription,
                            Modules = new ModulesViewModel { Id = module?.Id ?? 0, ModuleName = module?.ModuleName ?? String.Empty, ModuleDescription= module?.ModuleDescription, IsActive = module?.IsActive ?? false, CreatedBy = module.CreatedBy ?? null, CreatedDate = module.CreatedDate ?? null, ModifiedBy = module.ModifiedBy ?? null, ModifiedDate = module.ModifiedDate ?? null },
                            IsCreate = roleModule.IsCreate,
                            IsEdit = roleModule.IsEdit,
                            IsView = roleModule.IsView,
                            IsDelete = roleModule.IsDelete,
                            IsActive = roleModule.IsActive,
                            CreatedBy = roleModule.CreatedBy,
                            CreatedDate = roleModule.CreatedDate,
                            ModifiedBy = roleModule.ModifiedBy,
                            ModifiedDate = roleModule.ModifiedDate
                        }
                              ).FirstOrDefault();
            }
                
            
            return null;

        }

        #endregion

        #region Get Role By Name
        public async Task<RoleModuleViewModel> GetRoleModuleByName(string romeName)
        {
            var roleModuleList = await _unitOfWork.RoleModuleRepository.GetAllAsync();
            var moduleList = await _unitOfWork.ModuleRepository.GetAllAsync();

            if (roleModuleList.Any())
            {
                return (from roleModule in roleModuleList
                        join module in moduleList on roleModule.Modules?.Id equals module.Id
                        where roleModule.RoleName.Trim() == romeName.Trim()
                        select new RoleModuleViewModel
                        {
                            Id = roleModule.Id,
                            RoleName = roleModule.RoleName,
                            RoleDescription = roleModule.RoleDescription,
                            Modules = new ModulesViewModel { Id = module?.Id ?? 0, ModuleName = module?.ModuleName ?? String.Empty, ModuleDescription = module?.ModuleDescription, IsActive = module?.IsActive ?? false, CreatedBy = module.CreatedBy ?? null, CreatedDate = module.CreatedDate ?? null, ModifiedBy = module.ModifiedBy ?? null, ModifiedDate = module.ModifiedDate ?? null },
                            IsCreate = roleModule.IsCreate,
                            IsEdit = roleModule.IsEdit,
                            IsView = roleModule.IsView,
                            IsDelete = roleModule.IsDelete,
                            IsActive = roleModule.IsActive,
                            CreatedBy = roleModule.CreatedBy,
                            CreatedDate = roleModule.CreatedDate,
                            ModifiedBy = roleModule.ModifiedBy,
                            ModifiedDate = roleModule.ModifiedDate
                        }
                              ).FirstOrDefault();
            }


            return null;

        }

        #endregion

        #region Check Role Name is already existed or not
        public async Task<RoleModuleViewModel> GetRoleModuleByIdName(int roleId, string romeName)
        {
            var roleModuleList = await _unitOfWork.RoleModuleRepository.GetAllAsync();


            if (roleModuleList.Any())
            {
                return (from roleModule in roleModuleList
                        where roleModule.Id != roleId && roleModule.RoleName.Trim() == romeName.Trim()
                        select new RoleModuleViewModel
                        {
                            Id = roleModule.Id,
                            RoleName = roleModule.RoleName,
                            RoleDescription = roleModule.RoleDescription,
                            IsCreate = roleModule.IsCreate,
                            IsEdit = roleModule.IsEdit,
                            IsView = roleModule.IsView,
                            IsDelete = roleModule.IsDelete,
                            IsActive = roleModule.IsActive,
                            CreatedBy = roleModule.CreatedBy,
                            CreatedDate = roleModule.CreatedDate,
                            ModifiedBy = roleModule.ModifiedBy,
                            ModifiedDate = roleModule.ModifiedDate
                        }
                              ).FirstOrDefault();
            }


            return null;

        }

        #endregion

        #region Add/Edit Role
        public async Task<int> AddEditRoleModule(RoleModuleViewModel roleModuleViewModel, int loginUserId)
        {
            RoleModule roleModuleEntity = null;

            if (roleModuleViewModel.Id > 0)
            {
                roleModuleEntity = _unitOfWork.RoleModuleRepository.Find(p => p.Id == roleModuleViewModel.Id);
                
                if (roleModuleEntity != null)
                {

                    roleModuleEntity.RoleName = roleModuleViewModel.RoleName;                    
                    roleModuleEntity.RoleDescription = roleModuleViewModel.RoleDescription;
                    roleModuleEntity.Modules = _unitOfWork.ModuleRepository.Find(p => p.Id == roleModuleViewModel.Modules.Id);
                    roleModuleEntity.IsCreate = roleModuleViewModel.IsCreate;
                    roleModuleEntity.IsEdit = roleModuleViewModel.IsEdit;
                    roleModuleEntity.IsView = roleModuleViewModel.IsView;
                    roleModuleEntity.IsDelete = roleModuleViewModel.IsDelete;
                    roleModuleEntity.IsActive = roleModuleViewModel.IsActive;                    
                    roleModuleEntity.ModifiedBy = loginUserId;
                    roleModuleEntity.ModifiedDate = DateTime.Now;


                    _unitOfWork.RoleModuleRepository.Update(roleModuleEntity);
                }
            }
            else
            {
                roleModuleEntity = new RoleModule();
                roleModuleEntity.RoleName = roleModuleViewModel.RoleName;
                roleModuleEntity.RoleDescription = roleModuleViewModel.RoleDescription;
                roleModuleEntity.Modules = _unitOfWork.ModuleRepository.Find(p => p.Id == roleModuleViewModel.Modules.Id);
                roleModuleEntity.IsCreate = roleModuleViewModel.IsCreate;
                roleModuleEntity.IsEdit = roleModuleViewModel.IsEdit;
                roleModuleEntity.IsView = roleModuleViewModel.IsView;
                roleModuleEntity.IsDelete = roleModuleViewModel.IsDelete;
                roleModuleEntity.IsActive = roleModuleViewModel.IsActive;
                roleModuleEntity.CreatedBy = loginUserId;
                roleModuleEntity.CreatedDate = DateTime.Now;

                _unitOfWork.RoleModuleRepository.Add(roleModuleEntity);
            }


            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Delete Role
        public async Task<int> DeleteRoleModule(int roleId, bool isActive, int loginUserId)
        {
            var roleModuleEntity = _unitOfWork.RoleModuleRepository.Find(p => p.Id == roleId);

            if (roleModuleEntity != null)
            {
                roleModuleEntity.IsActive = isActive;
                roleModuleEntity.ModifiedBy = loginUserId;
                roleModuleEntity.ModifiedDate = DateTime.Now;

                _unitOfWork.RoleModuleRepository.Update(roleModuleEntity);
                return await _unitOfWork.SaveChangesAsync();

            }
            return 0;
        }

        #endregion

        #region Search Functionality For Role

        private void SearchRecords(ref List<RoleModuleViewModel> roleModuleList, string searchString)
        {
            if (!roleModuleList.Any() || string.IsNullOrWhiteSpace(searchString))
                return;

            roleModuleList = roleModuleList.Where(o => o.RoleName.ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                                       || o.RoleDescription.ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                                       || o.Modules.ModuleName.ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                                       || ((o.IsActive) ? "Active" : "Inactive").ToLowerInvariant().Contains(searchString.Trim().ToLowerInvariant())
                                                        ).ToList();
        }

        #endregion

        #region Get User Role By User ID and Role ID

        public async Task<bool> GetUserRoleModuleByUserIdRoleId(int userId, int roleId)
        {
            var userRoleModuleList = await _unitOfWork.UserRoleModuleRepository.GetAllAsync();

            var userList = (await _unitOfWork.UserInfoRepository.GetAllAsync()).ToList();
            return userRoleModuleList.Any(p => p.RoleModuleId == roleId && p.UserInfoId == userId);
            
        }

        #endregion

        #endregion
    }
}
