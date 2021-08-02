using System;
using IAutoM8.Global.Enums;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class TaskHistoryItemDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public int? ProjectTaskConditionOptionId { get; set; }
        public DateTime HistoryTime { get; set; }
        public string Status { get; set; }
        public string Condition { get; set; }
        public string ConditionOption { get; set; }
        public string TaskName { get; set; }
        public string ProjectName { get; set; }
    }
}
