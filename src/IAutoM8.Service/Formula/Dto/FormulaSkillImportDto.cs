using IAutoM8.Service.FormulaTasks.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Formula.Dto
{
    public class FormulaSkillImportDto
    {
        public int SkillId { get; set; }
        public int? ReviewingSkillId { get; set; }
        public int FormulaTaskId { get; set; }
        public string Title { get; set; }
        public string Team { get; set; }
        public string Skill { get; set; }
        public bool IsDisabled { get; set; }
        public bool CanBeOutsourced { get; set; }
        public bool CanWorkerHasSkill { get; set; }
        public bool CanReviewerHasSkill { get; set; }
        public List<Guid> UserIds { get; set; }
        public List<Guid> ReviewingUserIds { get; set; }
        public List<Guid> OutsourceUserIds { get; set; }
        public List<FormulaTaskOutsourceDto> CertifiedVendors { get; set; }
    }
}
