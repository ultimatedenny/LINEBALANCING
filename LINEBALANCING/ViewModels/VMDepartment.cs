using LineBalancing.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace LineBalancing.ViewModels
{
    public class VMDepartment
    {
        public VMCurrentUser CurrentUser { get; set; }

        public bool HasRelationalData { get; set; }
        public Department Department { get; set; }
        public IEnumerable<Department> Departments { get; set; }
    }
}