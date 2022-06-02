using LineBalancing.Models;
using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMLeaderLine
    {
        public VMCurrentUser CurrentUser { get; set; }

        public LeaderLine LeaderLine { get; set; }
        public IEnumerable<LeaderLine> LeaderLines { get; set; }
    }
}