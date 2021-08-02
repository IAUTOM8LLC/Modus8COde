using System;
using System.Collections.Generic;
using System.Text;
using IAutoM8.Global.Enums;
using IAutoM8.Service.Resources.Dto;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class FormulaTaskVendorDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public FormulaRequestStatus Answer { get; set; }
        public decimal? Price { get; set; }
        public decimal? CompanyWorkerPrice { get; set; }
        public int? Duration { get; set; }
        public int NotificationId { get; set; }
        public List<ResourceDto> Resources { get; set; }
        public string Role { get; set; }
    }
}
