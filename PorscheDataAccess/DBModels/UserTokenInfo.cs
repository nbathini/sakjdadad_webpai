using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PorscheDataAccess.DBModels
{
    public class UserTokenInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public UserInfo UserInfo { get; set; }
        public string? Token { get; set; }              
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool IsActive { get; set; }
    }
}
