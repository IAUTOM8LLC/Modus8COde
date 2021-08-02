using System;
using System.Collections.Generic;
using IAutoM8.Service.ProjectTasks.Dto;

namespace IAutoM8.Service.Formula.Dto
{
    public class ImportTasksDto
    {
        public int FormulaId { get; set; }
        public ProjectStartDatesDto ProjectStartDates { get; set; }
        public IEnumerable<SkillMapDto> SkillMappings { get; set; }
    }
}
