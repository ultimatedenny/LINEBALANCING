using LineBalancing.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineBalancing.Models
{
    public class LineProcess : IModifier
    {
        [Key]
        [Column(Order = 0)]
        [MaxLength(4)]
        public string Plant { get; set; }

        [Key]
        [Column(Order = 1)]
        [MaxLength(3)]
        public string Department { get; set; }

        [Key]
        [Column(Order = 2)]
        [MaxLength(10)]
        public string Line { get; set; }

        [Key]
        [Column(Order = 3)]
        public string ProcessCode { get; set; }

        [Key]
        [Column(Order = 4)]
        public string Sequence { get; set; }

        [MaxLength(100)]
        public string ProcessName { get; set; }

        public float StandardCT { get; set; }
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