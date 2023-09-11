using PorscheUtilities.HelperClass;
using PorscheUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PorscheComponent.Interface
{
    public interface IAuditLogComponent
    {
        Task<int> ManageAuditLog(AuditLogViewModel auditLog, long? userId);
        Task<PagedList<AuditLogViewModel>> GetAuditLog(PagingParameter pagingParameter);
        Task<List<AuditLogViewModel>> GetAllAuditLog();
        Task<int> AddAuditLog(string moduleName, string eventName, long centreId, string roleName, long userId);
    }
}
