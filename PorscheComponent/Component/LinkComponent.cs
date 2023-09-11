using Newtonsoft.Json;
using PorscheComponent.Interface;
using PorscheDataAccess.DBModels;
using PorscheDataAccess.Repositories;
using PorscheUtilities.Models;
using PorscheUtilities.Utility;

namespace PorscheComponent.Component
{
    public class LinkComponent : ILinkComponent
    {
        #region Private Variables
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        #region Constructor
        public LinkComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #endregion

        #region Methods

        #region Upload link in db and update json
        public async Task<int> UploadLink(LinkViewModel linkViewModel, string roleName, long userId)
        {
            var deliveryCheckList = await _unitOfWork.QuestionTypeRepository.FirstOrDefault(p => p.Id == (Int32)Enums.QuestionType.DeliveryCheckList);
            List<DeliveryCheckListJsonModel> deliveryCheckDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(deliveryCheckList.QuestionJson);
            Question question = null;
            
            if (!string.IsNullOrEmpty(linkViewModel.Link))
            {
                LinkDetail link = new LinkDetail();
                var fields = linkViewModel.Path.Split('/');
                if (fields.Length == 3)
                {
                    link.Section = fields[^3];
                    link.SubSection = fields[^2];
                    link.Question = fields[^1];
                    question = deliveryCheckDeserialized.Find(x => x.Name == fields[^3]).Fields.Find(p => p.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                }
                if (fields.Length == 2)
                {
                    link.Section = fields[^2];
                    link.SubSection = fields[^1];
                    question = deliveryCheckDeserialized.Find(x => x.Name == fields[^2]).Questions.Find(c => c.Name == fields[^1]);
                }

                if (question != null)
                {
                    #region Add Link Details
                    link.LinkName = linkViewModel.LinkName!= null ? linkViewModel.LinkName: string.Empty;
                    link.LinkPath = linkViewModel.Link;
                    link.IsActive = true;
                    link.CreatedBy = Convert.ToInt32(userId);
                    link.CreatedDate = DateTime.Now;                    
                    _unitOfWork.LinkDetailRepository.Add(link);
                    _unitOfWork.SaveChanges();

                    linkViewModel.Id = link.Id;
                    question.Links.Add(linkViewModel);

                    #endregion

                    AuditLogLink(Messages.ADL_LinkAdded.Replace("#LinkURL#", linkViewModel.Link), 0, roleName, userId);

                    #region Update Delivery Check List

                    var deliveryChecklistJson = JsonConvert.SerializeObject(deliveryCheckDeserialized);
                    deliveryCheckList.QuestionJson = deliveryChecklistJson;
                    deliveryCheckList.ModifiedBy = Convert.ToInt32(userId);
                    deliveryCheckList.ModifiedDate = DateTime.Now;
                    _unitOfWork.QuestionTypeRepository.Update(deliveryCheckList);

                    #endregion
                }
            }

            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region To delete/inactivate link from db and update json
        public async Task<int> DeleteLink(long linkId, string roleName, long userId)
        {
            var link = await _unitOfWork.LinkDetailRepository.FirstOrDefault(p => p.Id == linkId);
            
            Question question = null;
            
            if (link != null)
            {
                var deliveryCheckList = await _unitOfWork.QuestionTypeRepository.FirstOrDefault(p => p.Id == (Int32)Enums.QuestionType.DeliveryCheckList);
                List<DeliveryCheckListJsonModel> deliveryCheckDeserialized = JsonConvert.DeserializeObject<List<DeliveryCheckListJsonModel>>(deliveryCheckList.QuestionJson);
                if (link.Section != null && link.SubSection != null && link.Question != null)
                {
                    question = deliveryCheckDeserialized.Find(x => x.Name.Equals(link.Section, StringComparison.OrdinalIgnoreCase)).Fields.Find(p => p.Name.Equals(link.SubSection, StringComparison.OrdinalIgnoreCase)).Questions.Find(c => c.Name.Equals(link.Question, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    question = deliveryCheckDeserialized.Find(x => x.Name.Equals(link.Section, StringComparison.OrdinalIgnoreCase)).Questions.Find(c => c.Name.Equals(link.SubSection, StringComparison.OrdinalIgnoreCase));
                }
                if (question != null)
                {
                    question.Links.RemoveAll(p => p.Id == linkId);

                    #region Update Delivery Check List
                    
                    var deliveryChecklistJson = JsonConvert.SerializeObject(deliveryCheckDeserialized);
                    deliveryCheckList.QuestionJson = deliveryChecklistJson;
                    deliveryCheckList.ModifiedBy = Convert.ToInt32(userId);
                    deliveryCheckList.ModifiedDate = DateTime.Now;
                    _unitOfWork.QuestionTypeRepository.Update(deliveryCheckList);

                    #endregion

                    #region Inactivate/Delete Link

                    link.IsActive = false;
                    link.ModifiedBy = Convert.ToInt32(userId);                    
                    link.ModifiedDate = DateTime.Now;
                    _unitOfWork.LinkDetailRepository.Update(link);

                    #endregion

                    AuditLogLink(Messages.ADL_LinkDeleted.Replace("#LinkURL#", link.LinkPath), 0, roleName, userId);


                }
            }
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region To get link details based on linkId
        public async Task<LinkViewModel> GetLinkDetail(long linkId)
        {
            LinkViewModel link = new LinkViewModel();
            var linkEntity = await _unitOfWork.LinkDetailRepository.FirstOrDefault(p => p.Id == linkId);
            if (linkEntity != null)
            {
                link.Id = linkEntity.Id;
                link.Path = linkEntity.LinkPath;
            }
            return link;
        }

        #endregion

        #region Add Audit Log Link

        private void AuditLogLink(string eventName, long centreId, string roleName, long userId)
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
