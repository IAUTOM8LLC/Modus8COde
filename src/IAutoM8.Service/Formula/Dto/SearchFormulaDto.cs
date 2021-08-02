using IAutoM8.Global.Enums;
using System.Collections.Generic;

namespace IAutoM8.Service.Formula.Dto
{
    public class SearchFormulaDto
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
        public bool IsCustom { get; set; }
        public IEnumerable<int> FilterCategorieIds { get; set; }
        public string FilterSearch { get; set; }
        public string SortField { get; set; }
        public string SortDirection { get; set; }
    }
}
