using LineBalancing.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace LineBalancing.Models
{
    public class Plant : IModifier
    {
        [Key]
        [MaxLength(4)]
        public string PlantCode { get; set; }

        [MaxLength(50)]
        public string PlantDescription { get; set; }

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