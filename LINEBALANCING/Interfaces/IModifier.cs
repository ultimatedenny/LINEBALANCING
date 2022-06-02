using System;

namespace LineBalancing.Interfaces
{
    public interface IModifier
    {
        DateTime? CreatedTime { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedTime { get; set; }
        string UpdatedBy { get; set; }
    }
}
