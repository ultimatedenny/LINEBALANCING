using LineBalancing.Models;
using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMLeader
    {
        public VMCurrentUser CurrentUser { get; set; }

        public bool HasRelationalData { get; set; }
        public Leader Leader { get; set; }
        public IEnumerable<Leader> Leaders { get; set; }
    }
}