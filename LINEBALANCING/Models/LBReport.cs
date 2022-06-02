using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineBalancing.Models
{
    public class LBReport
    {
        [MaxLength(10)]
        public string Periode { get; set; }

        [MaxLength(10)]
        public string CheckPeriode { get; set; }

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
        public string Process { get; set; }

        [Key]
        [Column(Order = 5)]
        public string CheckID { get; set; }

        [MaxLength(50)]
        public string LeaderName { get; set; }

        public int TotalManPower { get; set; }

        [MaxLength(50)]
        [Column(Order = 6)]
        public string ManpowerName { get; set; }

        public int QuantityCheck { get; set; }
        public double StandardCT { get; set; }
        public double ActualCT { get; set; }
        public double CAPShift { get; set; }
        public double BalLost { get; set; }
        public double OMH { get; set; }
        public string FinalRemark { get; set; }

        [MaxLength(20)]
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
                        statusView = "c-status__success";
                }

                return statusView;
            }
        }

        [MaxLength(10)]
        public string EditTime { get; set; }

        [MaxLength(100)]
        public string EditReason { get; set; }

        [MaxLength(10)]
        public string Remark { get; set; }

        [MaxLength(50)]
        public string CheckBy { get; set; }

        public DateTime? CheckDate { get; set; }
    }
}