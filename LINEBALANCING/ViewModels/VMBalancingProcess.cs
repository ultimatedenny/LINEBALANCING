using LineBalancing.DTOs;
using LineBalancing.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace LineBalancing.ViewModels
{
    public class VMBalancingProcess
    {
        public DTOBalancingProcessItem DTOBalancingProcessItem { get; set; }
        public IEnumerable<DTOBalancingProcessItem> DTOBalancingProcessItems { get; set; }

        public BalancingProcessItem BalancingProcessItem { get; set; }

        // These properties are used as reponse after finish balancing process 
        public string Line { get; set; }
        public string Model { get; set; }
        public string CheckID { get; set; }

        // Dropdown list
        public SelectList SelectListTotalManpower { get; set; }
        public string SelectListOutputQuantity { get; set; }

        public string LineBalancingCheckID { get; set; }
    }
}