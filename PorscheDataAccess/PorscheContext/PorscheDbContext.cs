using Microsoft.EntityFrameworkCore;
using PorscheDataAccess.DBModels;

namespace PorscheDataAccess.PorscheContext
{
    public class PorscheDbContext : DbContext
    {
        public PorscheDbContext()
        {
        }

        public PorscheDbContext(DbContextOptions<PorscheDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Delivery> Delivery { get; set; }
        public DbSet<CarModel> CarModel { get; set; }
        public DbSet<QuestionType> QuestionType { get; set; }
        public DbSet<SurveyPreparation> SurveyPreparation { get; set; }
        public DbSet<CustomerSurvey> CustomerSurvey { get; set; }
        public DbSet<Centre> Centre { get; set; }        
        public DbSet<PreDeliveryCheckList> PreDeliveryCheckList { get; set; }
        public DbSet<CentreHistory> CentreHistory { get; set; }        
        public DbSet<DeliveryHistory> DeliveryHistory { get; set; }
        public DbSet<DeliveryType> DeliveryType { get; set; }
        public DbSet<DeliveryCheckList> DeliveryCheckList { get; set; }
        public DbSet<FileDetail> FileDetail { get; set; }
        public DbSet<LinkDetail> LinkDetail { get; set; }        
        public DbSet<Modules> Modules { get; set; }
        public DbSet<RoleModule> RoleModule { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<UserTokenInfo> UserTokenInfo { get; set; }
        public DbSet<UserRoleModule> UserRoleModule { get; set; }        
        public DbSet<UserCentre> UserCentre { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }
    }
}
