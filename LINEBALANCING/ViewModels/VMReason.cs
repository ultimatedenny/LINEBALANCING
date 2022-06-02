namespace LineBalancing.ViewModels
{
    public class VMReason
    {
        public int BalancingProcessItemId { get; set; }
        public string CheckId { get; set; }
        public string EditReason { get; set; }

        // This properties are used to parse value as return url
        public string Plant { get; set; }
        public string Department { get; set; }
        public string Line { get; set; }
        public string EmployeeNo { get; set; }
        public string Model { get; set; }
        public string ProcessCode { get; set; }
        public bool IsOneByOne { get; set; }
    }
}