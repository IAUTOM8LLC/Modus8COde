using IAutoM8.Service.CommonDto;
using System;
using System.Collections.Generic;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class UpdateTaskDto
    {
        public int Id { get; set; }
        public int? TeamId { get; set; }
        public int ProjectId { get; set; }
        public List<Guid> AssignedUserIds { get; set; }
        public List<Guid> ReviewingUserIds { get; set; }

        public string FormulaName { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public bool IsAutomated { get; set; }
        public bool IsRecurrent { get; set; }
        public bool IsConditional { get; set; }
        public bool IsInterval { get; set; }

        public int? Duration { get; set; }
        public int StartDelay { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool UseDateRangeAsDuration { get; set; }

        public int PosX { get; set; }
        public int PosY { get; set; }

        public AddTaskChecklistDto AddTodoCheckList { get; set; }
        public AddTaskChecklistDto AddReviewerCheckList { get; set; }

        public List<TaskChecklistDto> TaskCheckLists { get; set; }
        public List<UpdateTaskChecklistDto> EditCheckLists { get; set; }

        public TaskConditionDto Condition { get; set; }
        public RecurrenceOptionsDto RecurrenceOptions { get; set; }
    }
}
