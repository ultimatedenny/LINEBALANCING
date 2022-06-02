using LineBalancing.Models;
using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMLineProcess
    {
        public VMCurrentUser CurrentUser { get; set; }

        public LineProcess LineProcess { get; set; }
        public IEnumerable<LineProcess> LineProcesses { get; set; }
    }
}