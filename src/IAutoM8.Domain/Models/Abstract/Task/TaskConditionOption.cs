namespace IAutoM8.Domain.Models.Abstract.Task
{
    public abstract class TaskConditionOption<TCondition, TTask>
    {
        public int Id { get; set; }
        public TCondition Condition { get; set; }
        public int TaskConditionId { get; set; }
        public int? AssignedTaskId { get; set; }
        public string Option { get; set; }
        public bool IsSelected { get; set; }
        public virtual TTask AssignedTask { get; set; }
    }
}
