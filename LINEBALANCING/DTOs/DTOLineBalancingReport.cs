namespace LineBalancing.DTOs
{
    public class DTOLineBalancingReport
    {
        public int Id { get; set; }

        public string CheckID { get; set; }
        public string Process { get; set; }
        public string ProcessIdView { get; set; }
        public string Periode { get; set; }
        public string CheckingPeriode { get; set; }
        public string Department { get; set; }
        public int TotalManPower { get; set; }
        public double StandardCT { get; set; }
        public string ManpowerName { get; set; }
        public string ManpowerNameAllProcess { get; set; }
        public string Plant { get; set; }
        public string Line { get; set; }
        public double QuantityCheck { get; set; }
        public double CAPShift { get; set; }
        public double CAPShiftAllProcess { get; set; }
        public double ActualCT { get; set; }
        public string MultipleActualCT { get; set; }
        public double FinalCT { get; set; }
        public double TotalMPPerProcess { get; set; }
        public string CountManpower { get; set; }
        public double TotalCT { get; set; }
        public string Leader { get; set; }
        public string CheckBy { get; set; }
        public string CheckDate { get; set; }
        public double HCT { get; set; }
        public string FinalRemark { get; set; }
        public int Sequence { get; set; }
        public double BalancingProcessId { get; set; }
        public bool IsOneByOne { get; set; }

        // These property are used to display data on submission history
        public string Status { get; set; }
        public string StatusView
        {
            get
            {
                // This property is used to set css class for status
                var statusView = "c-status__default";

                if (!string.IsNullOrEmpty(Status))
                {
                    if (Status.ToUpper() == "COMPLETED")
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
        public string Model { get; set; }
        public double BalLost { get; set; }
        public double OMH { get; set; }
        public string Remark { get; set; }
        public string EditTimes { get; set; }
        public string EditReason { get; set; }
    }
}