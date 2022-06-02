using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Web.Configuration;

[assembly: OwinStartup(typeof(LineBalancing.App_Start.Startup))]

namespace LineBalancing.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var useWindowsAuthentication = WebConfigurationManager.AppSettings["UseWindowsAuthentication"];
            if (string.IsNullOrEmpty(useWindowsAuthentication) || useWindowsAuthentication.ToUpper() == "FALSE")
            {
                // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
                CookieAuthenticationOptions options = new CookieAuthenticationOptions
                {
                    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                    LoginPath = new PathString("/Account/Login")
                };
                app.UseCookieAuthentication(options);
            }
        }
    }
}
