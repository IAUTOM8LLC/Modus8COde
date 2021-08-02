namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class TaskDependencyDto
    {
        public int ParentTaskId { get; set; }
        public int ChildTaskId { get; set; }
        public bool Required { get; set; }
    }
}
