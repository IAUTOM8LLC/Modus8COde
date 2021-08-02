using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Domain.Models.Project.Task
{
    public class ProjectTaskChecklist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ProjectTaskId { get; set; }
        public bool TodoIsChecked { get; set; }
        public bool ReviewerIsChecked { get; set; }
        public TodoType? Type { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
