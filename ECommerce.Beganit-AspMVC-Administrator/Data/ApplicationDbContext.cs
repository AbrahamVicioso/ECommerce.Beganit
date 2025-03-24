using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Beganit_AspMVC_Administrator.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);

        //    builder.Entity<IdentityUser>(obj =>
        //    {
        //        obj.ToTable(name: "Staff");
        //    });

        //    builder.Entity<IdentityRole>(obj =>
        //    {
        //        obj.ToTable(name: "AspNetStaffRoles");
        //    });

        //    builder.Entity<IdentityUserRole<string>>(obj =>
        //    {
        //        obj.ToTable(name: "AspNetStaffUserRoles");
        //    });

        //    builder.Entity<IdentityUserClaim<string>>(obj =>
        //    {
        //        obj.ToTable(name: "AspNetStaffUserClaims");
        //    });

        //    builder.Entity<IdentityUserLogin<string>>(obj =>
        //    {
        //        obj.ToTable(name: "AspNetStaffUserLogins");
        //    });

        //    builder.Entity<IdentityRoleClaim<string>>(obj =>
        //    {
        //        obj.ToTable(name: "AspNetStaffRoleClaims");
        //    });

        //    builder.Entity<IdentityUserToken<string>>(obj =>
        //    {
        //        obj.ToTable(name: "AspNetStaffUserTokens");
        //    });
        //}
    }
}
