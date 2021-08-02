using System;
using IAutoM8.Global.Enums;

namespace IAutoM8.Domain.Models.Project.Task
{
    public class TaskHistory
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int? ProjectTaskConditionOptionId { get; set; }
        public DateTime HistoryTime { get; set; }
        public ActivityType Type { get; set; }
        public Guid? UserGuid { get; set; }

        #region Navigation Properties

        public virtual User.User User { get; set; }
        public virtual ProjectTask Task { get; set; }
        public virtual ProjectTaskConditionOption ProjectTaskConditionOption { get; set; }

        #endregion
    }
}
