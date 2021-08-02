using System;
using System.Collections.Generic;
using System.Text;
using IAutoM8.Domain.Models;
using IAutoM8.Global.Enums;
using IAutoM8.Service.Resources.Dto;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class ProjectTaskVendorDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public ProjectRequestStatus Answer { get; set; }
        public decimal Price { get; set; }
        public int? Duration { get; set; }
        public int NotificationId { get; set; }
        public List<ResourceDto> Resources { get; set; }
        public bool IsRecurrent { get; set; }
        public bool IsAutomated { get; set; }
        public bool IsConditional { get; set; }
        public FormulaTaskRecurrenceType? RecurrenceType { get; set; }
        public RecurrenceOptions RecurrenceOptions { get; set; }
    }
}
