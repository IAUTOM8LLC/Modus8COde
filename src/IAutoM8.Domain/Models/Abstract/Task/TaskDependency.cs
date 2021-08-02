namespace IAutoM8.Domain.Models.Abstract.Task
{
    public abstract class TaskDependency<T>
    {
        public int ParentTaskId { get; set; }
        public int ChildTaskId { get; set; }
        public bool Required { get; set; }
        public virtual T ParentTask { get; set; }
        public virtual T ChildTask { get; set; }
    }
}
