using IAutoM8.Domain.Models.Formula;

namespace IAutoM8.Domain.Models.Resource
{
    public class ResourceFormula : ResourceBase
    {
        public int FormulaId { get; set; }
        public virtual FormulaProject Formula { get; set; }
    }
}
