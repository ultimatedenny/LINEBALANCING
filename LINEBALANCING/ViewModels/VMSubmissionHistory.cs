using LineBalancing.DTOs;
using System.Collections.Generic;
using System.Web.Mvc;

namespace LineBalancing.ViewModels
{
    public class VMSubmissionHistory
    {
        public VMCurrentUser CurrentUser { get; set; }

        public IEnumerable<DTOLineBalancingReport> DTOLineBalancingReports { get; set; }

        // This property is used to leader line
        public int LeaderLineId { get; set; }

        // Dropdown list
        public SelectList SelectListFrom { get; set; }
        public SelectList SelectListPeriode { get; set; }
        public SelectList SelectListLeader { get; set; }
        public SelectList SelectListPlant { get; set; }
        public SelectList SelectListLine { get; set; }
        public SelectList SelectListModel { get; set; }
        public SelectList SelectListStatus { get; set; }

        public DTOBalancing DTOBalancing { get; set; }
    }

    public class VMSubmissionHistoryFilter
    {
        public string From { get; set; }
        public string Periode { get; set; }
        public string Leader { get; set; }
        public string Plant { get; set; }
        public string Department { get; set; }
        public string Line { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
    }
}