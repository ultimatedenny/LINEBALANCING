using LineBalancing.Context;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;

namespace LineBalancing.Helpers
{
    public static class UserHelper
    {
        public static void CreateDefaultRole()
        {
            ApplicationContext context = new ApplicationContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var admin = new IdentityRole();
            admin.Name = Constanta.Role.ADMIN;
            roleManager.Create(admin);

            var user = new IdentityRole();
            user.Name = Constanta.Role.USER;
            roleManager.Create(user);
        }

        public static void CreateDefaultAdmin()
        {
            CreateAspNetAdmin();

            CreateWinAuthAdmin();
        }

        public static void CreateAspNetAdmin()
        {
            ApplicationContext context = new ApplicationContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Create 3 admins
            for (int i = 1; i <= 3; i++)
            {
                ApplicationUser applicationUser = new ApplicationUser();
                applicationUser.UserName = "admin" + i;
                applicationUser.IsActive = true.ToString();

                string defaultPassword = "12345678";

                // Store Name as Claim
                applicationUser.Claims.Add(new IdentityUserClaim() { ClaimType = ClaimTypes.Name, ClaimValue = "Shimano" });

                var createResult = userManager.Create(applicationUser, defaultPassword);
                if (createResult.Succeeded)
                    userManager.AddToRole(applicationUser.Id, Constanta.Role.ADMIN);
            }
        }

        public static void CreateWinAuthAdmin()
        {
            ApplicationContext context = new ApplicationContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Create 3 admins
            for (int i = 1; i <= 3; i++)
            {
                ApplicationUser applicationUser = new ApplicationUser();
                applicationUser.UserName = "winauthadmin" + i;
                applicationUser.IsActive = true.ToString();

                string defaultPassword = "12345678";

                // Store Name as Claim
                applicationUser.Claims.Add(new IdentityUserClaim() { ClaimType = ClaimTypes.Name, ClaimValue = "Shimano" });

                var createResult = userManager.Create(applicationUser, defaultPassword);
                if (createResult.Succeeded)
                    userManager.AddToRole(applicationUser.Id, Constanta.Role.ADMIN);
            }
        }
    }
}