using System.Collections.Generic;

namespace IAutoM8.Service.Formula.Dto
{
    public class SearchFormulaResultDto<T> where T : class
    {
        public int TotalCount { get; set; }
        public bool IsAdmin { get; set; }
        public IEnumerable<T> Formulas { get; set; }
    }
}
