using IAutoM8.Domain.Models.Formula.Task;

namespace IAutoM8.Domain.Models.Resource
{
    public class ResourceFormulaTask : ResourceBase
    {
        public int FormulaTaskId { get; set; }
        
        public virtual FormulaTask FormulaTask { get; set; }
    }
}
