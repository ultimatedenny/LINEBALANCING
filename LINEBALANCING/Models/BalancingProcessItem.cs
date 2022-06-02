using LineBalancing.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace LineBalancing.Models
{
    // This model is created as temporary table
    public class BalancingProcessItem : IModifier
    {
        public int Id { get; set; }

        public int BalancingProcessId { get; set; }

        [MaxLength(4)]
        public string Plant { get; set; }

        [MaxLength(3)]
        public string Department { get; set; }

        [MaxLength(10)]
        public string Line { get; set; }

        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string Model { get; set; }
        public double StandardCT { get; set; }
        public int TotalManPower { get; set; }

        [MaxLength(50)]
        public string ManpowerName { get; set; }

        [MaxLength(10)]
        public string EmployeeNo { get; set; }

        [MaxLength(50)]
        public string LeaderName { get; set; }

        public int Quantity { get; set; }
        public double ActualCT { get; set; }

        // This property is used to parse value from checking method
        public bool IsOneByOne { get; set; }

        public string Status { get; set; }
        public int EditTime { get; set; }

        [MaxLength(50)]
        public string EditReason { get; set; }
        public string CheckBy { get; set; }
        public string Remark { get; set; }

        public DateTime? CreatedTime { get; set; }

        [MaxLength(20)]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedTime { get; set; }

        [MaxLength(20)]
        public string UpdatedBy { get; set; }
        public double Sequence { get; set; }
    }
}