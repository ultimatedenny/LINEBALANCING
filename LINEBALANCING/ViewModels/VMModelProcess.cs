using LineBalancing.Models;
using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMModelProcess
    {
        public VMCurrentUser CurrentUser { get; set; }

        public ModelProcess ModelProcess { get; set; }
        public IEnumerable<ModelProcess> ModelProcesses { get; set; }
    }
}