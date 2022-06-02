using LineBalancing.DTOs;
using LineBalancing.Models;
using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMBalancing
    {
        public VMCurrentUser CurrentUser { get; set; }

        public IEnumerable<DTOLineBalancingReport> OutstandingJobs { get; set; }
        public int OutstandingJobCount { get; set; }
        public string OutstandingJob { get; set; }

        // This properties are used to save leader line
        public LeaderLine LeaderLine { get; set; }
        public int LeaderLineId { get; set; }

        // This properties are used to leader line (for login leader)
        public string PlantCode { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeNo { get; set; }
        public string LeaderName { get; set; }

        // This property is used to parse value after create leader line
        public DTOBalancing DTOBalancing { get; set; }
    }
}