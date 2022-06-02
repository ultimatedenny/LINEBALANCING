using LineBalancing.DTOs;
using System.Collections.Generic;
using System.Web.Mvc;

namespace LineBalancing.ViewModels
{
    public class VMSummaryData
    {
        public VMCurrentUser CurrentUser { get; set; }

        public IEnumerable<DTOLineBalancingReport> DTOLineBalancingReports { get; set; }

        public int LineBalancingReportId { get; set; }

        // Dropdown list
        public SelectList SelectListFrom { get; set; }
        public SelectList SelectListPeriode { get; set; }
        public SelectList SelectListLeader { get; set; }
        public SelectList SelectListPlant { get; set; }
        public SelectList SelectListDepartment { get; set; }
        public SelectList SelectListLine { get; set; }
        public SelectList SelectListModel { get; set; }
        public SelectList SelectListStatus { get; set; }
        public SelectList SelectListProcess { get; set; }
    }

    public class VMSummaryDataFilter
    {
        // These properties are used to filter detail summary data by category
        public string LineBalancingReportId { get; set; }
        public string Category { get; set; }

        public string From { get; set; }
        public string Periode { get; set; }
        public string Leader { get; set; }
        public string Plant { get; set; }
        public string Department { get; set; }
        public string Line { get; set; }
        public string Process { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
    }
}