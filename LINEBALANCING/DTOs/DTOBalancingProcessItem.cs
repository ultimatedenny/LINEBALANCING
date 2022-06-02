using System;

namespace LineBalancing.DTOs
{
    // This class is used to bind data from table balancing process item or as result of joining table leader vs line & manpower vs process
    public class DTOBalancingProcessItem
    {
        public int Id { get; set; }
        public string CheckID { get; set; }

        public string Plant { get; set; }
        public string Department { get; set; }
        public string Line { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public int TotalManPower { get; set; }
        public string ManpowerName { get; set; }

        public string Model { get; set; }
        public double StandardCT { get; set; }
        public double Sequence { get; set; }
        public string EmployeeNo { get; set; }
        public string LeaderName { get; set; }
        
        public int Quantity { get; set; }
        public double ActualCT { get; set; }
        public string ActualCTView
        {
            get
            {
                return ActualCT.ToString("N2");
            }
        }
        public double CAPShift { get; set; }
        public double BalLost { get; set; }
        public double OMH { get; set; }

        // This property is used to parse value from checking method
        public bool IsOneByOne { get; set; }

        public string Status { get; set; }
        public string StatusView
        {
            get
            {
                // This property is used to set css class for status
                var statusView = "c-status__default";

                if (!string.IsNullOrEmpty(Status))
                {
                    if (Status.ToUpper() == "DONE")
                    {
                        statusView = "c-status__success";
                    }
                    else if (Status.ToUpper() == "IN PROGRESS")
                    {
                        statusView = "c-status__in-progress";
                    }
                }

                return statusView;
            }
        }

        public int EditTime { get; set; }
        public string EditReason { get; set; }
        public string CheckBy { get; set; }

        public DateTime CreatedTime { get; set; }
        public string CreatedUser { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string UpdatedBy { get; set; }
    }
}