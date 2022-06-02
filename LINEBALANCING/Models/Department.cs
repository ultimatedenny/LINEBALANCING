using LineBalancing.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineBalancing.Models
{
    public class Department : IModifier
    {
        [Key]
        [MaxLength(4)]
        [Column(Order = 0)]
        public string Plant { get; set; }

        [Key]
        [Column(Order = 1)]
        [MaxLength(3)]
        public string DepartmentName { get; set; }

        [MaxLength(50)]
        public string DepartmentDescription { get; set; }

        public bool Active { get; set; }
        public string Status
        {
            get
            {
                return Active ? "Active" : "Non Active";
            }
        }

        public DateTime? CreatedTime { get; set; }

        [MaxLength(20)]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedTime { get; set; }

        [MaxLength(20)]
        public string UpdatedBy { get; set; }
    }
}