using System;
using System.Collections.Generic;
using IAutoM8.Service.Resources.Dto;

namespace IAutoM8.Service.FormulaTasks.Dto
{
    public class FormulaTaskDto: UpdateFormulaTaskDto
    {
        public int? ParentTaskId { get; set; }
        public int? InternalFormulaProjectId { get; set; }
        public int? AssignedSkillId { get; set; }        
        public int? ReviewingSkillId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public List<int> ParentTasks { get; set; }
        public List<int> ChildTasks { get; set; }
        public List<ResourceDto> Resources { get; set; }
        public List<FormulaTaskChecklistDto> FormulaTaskChecklists { get; set; }
        public bool? ShowTrainingTab { get; set; }
    }
}
