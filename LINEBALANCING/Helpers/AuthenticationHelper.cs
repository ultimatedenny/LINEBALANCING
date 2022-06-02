using LineBalancing.ViewModels;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;

namespace LineBalancing.Helpers
{
    public static class AuthenticationHelper
    {
        public static bool IsUseWindowsAuthentication()
        {
            bool isUseWindowsAuthentication = false;

            var useWindowsAuthentication = WebConfigurationManager.AppSettings["UseWindowsAuthentication"];
            if (!string.IsNullOrEmpty(useWindowsAuthentication) && useWindowsAuthentication.ToUpper() == "TRUE")
            {
                isUseWindowsAuthentication = true;
            }

            return isUseWindowsAuthentication;
        }

        public static VMCurrentUser CurrentUser(string currentUserName = "")
        {
            VMCurrentUser vmCurrentUser = null;

            try
            {
                HttpContext context = HttpContext.Current;
                var currentUser = (VMCurrentUser)context.Session["Login"];
                if (currentUser != null)
                    vmCurrentUser = currentUser;
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return vmCurrentUser;
        }

        public static IList<string> CurrentUserRoles(string currentUserName = "")
        {
            IList<string> currentRoles = null;

            try
            {
                HttpContext context = HttpContext.Current;
                var currentUser = (VMCurrentUser)context.Session["Login"];
                if (currentUser != null)
                    currentRoles = currentUser.Roles;
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return currentRoles;
        }
    }
}