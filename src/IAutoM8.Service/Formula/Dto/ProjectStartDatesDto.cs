using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Formula.Dto
{
    public class ProjectStartDatesDto
    {
        public DateTime? ProjectStartDateTime { get; set; }
        public Dictionary<int, DateTime?> RootStartDateTime { get; set; } = new Dictionary<int, DateTime?>();
    }
}
