using System.Collections.Generic;

namespace IAutoM8.Service.Formula.Dto
{
    public class AddFormulaDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FormulaOverview { get; set; }
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
