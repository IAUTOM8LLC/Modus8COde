using IAutoM8.Global.Enums;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class ConditionOptionDto
    {
        public int ConditionOptionId { get; set; }
        public TaskStatusType Status { get; set; }
        public byte? Rating { get; set; }
    }
}
