using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Formula.Dto
{
    public class SkillImportItemDto
    {
        public int SkillId { get; set; }
        public string Name { get; set; }
        public bool CanBeOutsourced { get; set; }
        public bool CanWorkerHasSkill { get; set; }
        public List<Guid> UserIds { get; set; }
    }
}
