using LineBalancing.Helpers;
using System.Web;
using System.Web.Mvc;

namespace LineBalancing.Authorization
{
    public class CustomAuthorization : AuthorizeAttribute
    {
        public string AccessLevel { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            var currentUserRoles = AuthenticationHelper.CurrentUserRoles();
            if (currentUserRoles == null || currentUserRoles.Count == 0)
                return false;

            return currentUserRoles.Contains(AccessLevel);
        }
    }
}