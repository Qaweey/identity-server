using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared.Enums;
using System.Data;

namespace settl.identityserver.EntityFrameworkCore.AppDbContext
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public DbSet<Users> TblUsers { get; set; }
        public DbSet<tbl_OTP> TblOTP { get; set; }
        public DbSet<UserOnboarding> TblUserOnboarding { get; set; }
        public DbSet<SecurityAnswer> TblSecurityAnswer { get; set; }
        public DbSet<UserTypes> TblUserTypes { get; set; }
        public DbSet<SecurityQuestion> TblSecurityQuestions { get; set; }
        public DbSet<SmileIdVerification> SmileIdVerifications { get; set; }

        public DbSet<tbl_admin> TblAdmin { get; set; }
        public DbSet<tbl_admin_role> TblAdminRoles { get; set; }
        public DbSet<TblRegisterNewDeviceLog> TblRegisterNewDevices { get; set; }

        public IDbConnection Connection => Database.GetDbConnection();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Users>(entity =>
            {
                entity.Property(p => p.Id).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Constants.DB_CONNECTION);
            }
        }
    }
}