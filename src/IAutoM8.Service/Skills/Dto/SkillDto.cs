using IAutoM8.Domain.Models.Formula;
using IAutoM8.Global.Enums;
using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Skills.Dto
{
    public class SkillDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public List<UserSkillDto> Users { get; set; }
        public bool HasAssignedTasks { get; set; }
        public bool IsWorkerSkill { get; set; }
        public bool IsGlobal { get; set; }
        public int? Status { get; set; }
        public string TeamName { get; set; }
        public List<FormulaProject> RevFormulas { get; set; }
        public List<FormulaProject> DevFormulas { get; set; }
    }
}
