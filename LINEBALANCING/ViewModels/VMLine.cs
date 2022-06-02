using LineBalancing.Models;
using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMLine
    {
        public VMCurrentUser CurrentUser { get; set; }

        public bool HasRelationalData { get; set; }
        public Line Line { get; set; }
        public IEnumerable<Line> Lines { get; set; }
    }
}