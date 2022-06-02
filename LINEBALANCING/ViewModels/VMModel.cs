using LineBalancing.Models;
using System.Collections.Generic;

namespace LineBalancing.ViewModels
{
    public class VMModel
    {
        public VMCurrentUser CurrentUser { get; set; }

        public bool HasRelationalData { get; set; }
        public Model Model { get; set; }
        public IEnumerable<Model> Models { get; set; }
    }
}