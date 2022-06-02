using LineBalancing.Constanta;
using LineBalancing.Context;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;
using System.Web.Security;

namespace LineBalancing.Authorization
{
    public class CustomRoleProvider : RoleProvider
    {
        private ApplicationContext db = new ApplicationContext();
        private UserStore<ApplicationUser> userStore;
        private UserManager<ApplicationUser> userManager;

        public override string ApplicationName { get; set; }

        public CustomRoleProvider()
        {
            userStore = new UserStore<ApplicationUser>(db);
            userManager = new UserManager<ApplicationUser>(userStore);
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            string[] currentRoles = null;

            var selectedUser = userManager.FindByName(username);
            if (selectedUser != null)
            {
                currentRoles = userManager.GetRoles(selectedUser.Id).ToArray();
            }

            return currentRoles;
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            bool isUserInRole = false;

            var selectedUser = userManager.FindByName(username);
            if (selectedUser != null)
            {
                var currentRoles = userManager.GetRoles(selectedUser.Id);

                isUserInRole = currentRoles.Contains(roleName);
            }

            return isUserInRole;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}