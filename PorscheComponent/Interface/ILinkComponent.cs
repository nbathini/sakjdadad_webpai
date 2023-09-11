using PorscheUtilities.Models;

namespace PorscheComponent.Interface
{
    public interface ILinkComponent
    {
        Task<int> UploadLink(LinkViewModel linkViewModel, string roleName, long userId);
        Task<int> DeleteLink(long linkId, string roleName, long userId);
        Task<LinkViewModel> GetLinkDetail(long linkId);
    }
}
