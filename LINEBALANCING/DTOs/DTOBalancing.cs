namespace LineBalancing.DTOs
{
    public class DTOBalancing
    {
        public string BalancingProcessId { get; set; }

        public string Plant { get; set; }
        public string Department { get; set; }
        public string Line { get; set; }
        public string EmployeeNo { get; set; }
        public string LeaderName { get; set; }
        public string Model { get; set; }
        public bool IsOneByOne { get; set; }

        // Parse only for submission history
        public string CheckId { get; set; }
    }
}