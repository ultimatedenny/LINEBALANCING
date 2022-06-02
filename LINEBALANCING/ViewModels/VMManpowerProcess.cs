using LineBalancing.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace LineBalancing.ViewModels
{
    public class VMManpowerProcess
    {
        public VMCurrentUser CurrentUser { get; set; }

        public ManpowerProcess ManpowerProcess { get; set; }
        public IEnumerable<ManpowerProcess> ManpowerProcesses { get; set; }
    }
}