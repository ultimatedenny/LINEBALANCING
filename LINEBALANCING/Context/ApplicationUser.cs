using Microsoft.AspNet.Identity.EntityFramework;

namespace LineBalancing.Context
{
    public class ApplicationUser : IdentityUser
    {
        public string EmployeeNo { get; set; }
        public string LeaderName { get; set; }
        public string IsActive { get; set; }
    }
}