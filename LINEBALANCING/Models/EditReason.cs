using System;
using System.ComponentModel.DataAnnotations;

namespace LineBalancing.Models
{
    public class EditReason
    {
        [Key]
        public int Id { get; set; }

        public int BalancingProcessItemId { get; set; }
        public string CheckID { get; set; }

        [MaxLength(4)]
        public string Plant { get; set; }

        [MaxLength(3)]
        public string Department { get; set; }

        [MaxLength(10)]
        public string Line { get; set; }

        public string ProcessName { get; set; }
        public string Model { get; set; }
        public double StandardCT { get; set; }
        public int TotalManPower { get; set; }

        [MaxLength(50)]
        public string ManpowerName { get; set; }

        [MaxLength(50)]
        public string LeaderName { get; set; }

        public int Quantity { get; set; }
        public double ActualCT { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedDate { get; set; }
        public int EditTime { get; set; }
    }
}