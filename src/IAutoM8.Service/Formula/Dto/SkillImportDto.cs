using IAutoM8.Service.FormulaTasks.Dto;
using IAutoM8.Service.Users.Dto;
using System.Collections.Generic;

namespace IAutoM8.Service.Formula.Dto
{
    public class SkillImportDto
    {
        public List<FormulaSkillImportDto> FormulaTaskItems { get; set; }
        public List<SkillImportItemDto> SkillItems { get; set; }
        public List<SkillUserItemDto> AllUsers {get;set; }
        public List<SkillUserItemDto> ManagerUsers { get; set; }
        public List<FormulaTaskOutsourceDto> OutsourcedUsers { get; set; }
    }
}
