using IAutoM8.Service.CommonDto;
using System.Collections.Generic;

namespace IAutoM8.Service.FormulaTasks.Dto
{
    public class UpdateFormulaTaskDto
    {
        public int Id { get; set; }
        public int? TeamId { get; set; }
        public int FormulaProjectId { get; set; }
        public int? AssignedTeamId { get; set; }
        public int? AssignedSkillId { get; set; }
        public int? ReviewingSkillId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public bool IsAutomated { get; set; }
        public bool IsConditional { get; set; }
        public bool IsRecurrent { get; set; }
        public bool IsInterval { get; set; }
        public bool IsTrainingLocked { get; set; }

        public int? Duration { get; set; }
        public int StartDelay { get; set; }

        public int PosX { get; set; }
        public int PosY { get; set; }

        public AddTaskChecklistDto AddTodoCheckList { get; set; }
        public AddTaskChecklistDto AddReviewerCheckList { get; set; }

        public FormulaTaskConditionDto Condition { get; set; }
        public RecurrenceOptionsDto RecurrenceOptions { get; set; }

        public string ReviewerTraining { get; set; }
    }
}
