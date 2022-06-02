using LineBalancing.Constanta;
using System.Collections.Generic;

namespace LineBalancing.Helpers
{
    public static class StatusHelper
    {
        public static List<string> Statuses()
        {
            var statuses = new List<string>();
            statuses.Add(Status.NOT_RUNNING);
            statuses.Add(Status.IN_PROGRESS);
            statuses.Add(Status.COMPLETED);

            return statuses;
        }
    }
}