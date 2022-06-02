using LineBalancing.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineBalancing.Models
{
    public class MonthlySchedule : IModifier
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
        [MaxLength(20)]
        public string Line { get; set; }

        [Key]
        [Column(Order = 3)]
        public string Model { get; set; }

        [Key]
        [Column(Order = 4)]
        public string ProcessName { get; set; }

        [Key]
        [Column(Order = 5)]
        public DateTime DateFrom { get; set; }
        public string ShortDateFrom
        {
            get
            {
                return DateFrom.ToString("dd-MM-yyyy");
            }
        }

        [Key]
        [Column(Order = 6)]
        public DateTime DateTo { get; set; }
        public string ShortDateTo
        {
            get
            {
                return DateTo.ToString("dd-MM-yyyy");
            }
        }

        [MaxLength(10)]
        public string Remark { get; set; }

        public DateTime? CreatedTime { get; set; }

        [MaxLength(20)]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedTime { get; set; }

        [MaxLength(20)]
        public string UpdatedBy { get; set; }
    }
}