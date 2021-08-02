namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class TaskConditionOptionDto
    {
        public int Id { get; set; }
        public int AssignedTaskId { get; set; }
        public bool IsSelected { get; set; }
        public string Option { get; set; }
        public int ConditionalTaskId { get; set; }
    }
}
