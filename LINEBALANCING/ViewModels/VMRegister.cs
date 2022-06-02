using LineBalancing.DTOs;
using LineBalancing.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace LineBalancing.ViewModels
{
    public class VMRegister
    {
        public VMCurrentUser CurrentUser { get; set; }

        public IEnumerable<DTORegisteredUser> RegisteredUsers { get; set; }
        public DTORegisteredUser RegisteredUser { get; set; }

        // Dropdown list
        public SelectList SelectListLeader { get; set; }
        public SelectList SelectListStatus { get; set; }
    }
}