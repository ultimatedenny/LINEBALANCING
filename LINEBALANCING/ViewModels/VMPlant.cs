using LineBalancing.Models;
using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMPlant
    {
        public VMCurrentUser CurrentUser { get; set; }

        public bool HasRelationalData { get; set; }
        public Plant Plant { get; set; }
        public IEnumerable<Plant> Plants { get; set; }
    }
}