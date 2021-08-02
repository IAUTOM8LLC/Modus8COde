namespace IAutoM8.Service.FormulaTasks.Dto
{
    public class FormulaTaskDependencyDto
    {
        public int ParentTaskId { get; set; }
        public int ChildTaskId { get; set; }
        public bool Required { get; set; }
    }
}
