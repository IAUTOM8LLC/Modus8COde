using IAutoM8.Global.Enums;

namespace IAutoM8.Domain.Models.Project.Task
{
    public class TaskJob
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string HangfireJobId { get; set; }
        public TaskJobType Type { get; set; }

        #region Navigation Properties

        public virtual ProjectTask Task { get; set; }

        #endregion
    }
}
