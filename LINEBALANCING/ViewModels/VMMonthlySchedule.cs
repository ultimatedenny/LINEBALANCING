using LineBalancing.Models;
using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMMonthlySchedule
    {
        public VMCurrentUser CurrentUser { get; set; }

        public IEnumerable<MonthlySchedule> MonthlySchedules { get; set; }
    }
}