using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class SkillMapDto
    {
        public int SkillId { get; set; }
        public int? ReviewingSkillId { get; set; }
        public int FormulaTaskId { get; set; }
        public IEnumerable<Guid> UserIds { get; set; }
        public IEnumerable<Guid> ReviewingUserIds { get; set; }
        public IEnumerable<Guid> OutsourceUserIds { get; set; }
        public bool IsOutsorced { get; set; }
        public bool IsDisabled { get; set; }
    }
}
