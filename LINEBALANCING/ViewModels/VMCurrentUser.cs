using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMCurrentUser
    {
        public string Username { get; set; }
        public IList<string> Roles { get; set; }
        public bool IsWindowsAuthentication { get; set; }
        public bool IsAdmin { get; set; }
    }
}