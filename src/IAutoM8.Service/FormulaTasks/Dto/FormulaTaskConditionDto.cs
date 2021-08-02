using System.Collections.Generic;

namespace IAutoM8.Service.FormulaTasks.Dto
{
    public class FormulaTaskConditionDto
    {
        public string Condition { get; set; }
        public List<FormulaTaskConditionOptionDto> Options { get; set; }
    }
}
