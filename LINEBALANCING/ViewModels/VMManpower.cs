using LineBalancing.Models;
using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMManpower
    {
        public VMCurrentUser CurrentUser { get; set; }

        public bool HasRelationalData { get; set; }
        public ManPower Manpower { get; set; }
        public IEnumerable<ManPower> Manpowers { get; set; }
    }
}