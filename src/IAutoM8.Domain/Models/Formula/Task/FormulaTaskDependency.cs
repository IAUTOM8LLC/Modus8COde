using IAutoM8.Domain.Models.Abstract.Task;

namespace IAutoM8.Domain.Models.Formula.Task
{
    public class FormulaTaskDependency: TaskDependency<FormulaTask>
    {
        public FormulaTaskDependency() { }

        public FormulaTaskDependency(FormulaTaskDependency formulaTaskDependency)
        {
            Required = formulaTaskDependency.Required;
        }
    }
}
