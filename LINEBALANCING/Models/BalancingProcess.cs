using LineBalancing.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace LineBalancing.Models
{
    public class BalancingProcess : IModifier
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(4)]
        public string Plant { get; set; }

        [MaxLength(3)]
        public string Department { get; set; }

        [MaxLength(10)]
        public string Line { get; set; }

        [MaxLength(10)]
        public string EmployeeNo { get; set; }

        [MaxLength(50)]
        public string LeaderName { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public DateTime? CreatedTime { get; set; }

        [MaxLength(20)]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedTime { get; set; }

        [MaxLength(20)]
        public string UpdatedBy { get; set; }
    }
}