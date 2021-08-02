using IAutoM8.Domain.Models.Abstract.Task;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global.Enums;
using System;
using System.Collections.Generic;

namespace IAutoM8.Domain.Models.Project.Task
{
    public class ProjectTask : BaseTask
    {
        public TaskStatusType Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool? DescNotificationFlag { get; set; }
        public string ReviewerTraining { get; set; }
        public bool IsTrainingLocked { get; set; }
        public bool? IsDisabled { get; set; }

        #region Foreign Keys

        public Guid OwnerGuid { get; set; }
        public int? TeamId { get; set; }
        public int ProjectId { get; set; }
        public int? AssignedTeamId { get; set; }
        public int? ReviewingTeamId { get; set; }
        public int? AssignedSkillId { get; set; }
        public int? ReviewingSkillId { get; set; }
        public int? TaskConditionId { get; set; }
        public int? RecurrenceOptionsId { get; set; }
        public int? TreeDetailId { get; set; }
        public Guid? ProccessingUserGuid { get; set; }
        public Guid? ReviewingUserGuid { get; set; }

        public int? FormulaId { get; set; }
        public int? ParentTaskId { get; set; }
        public int? FormulaTaskId { get; set; }

        #endregion

        #region Navigation Properties

        public virtual Project Project { get; set; }
        public virtual User.User Owner { get; set; }
        public virtual ProjectTaskCondition Condition { get; set; }
        public virtual RecurrenceOptions RecurrenceOptions { get; set; }
        public virtual Team.Team AssignedProjectTeam { get; set; }
        public virtual Team.Team AssignedTeam { get; set; }
        public virtual Team.Team ReviewingTeam { get; set; }
        public virtual Skill.Skill AssignedSkill { get; set; }
        public virtual Skill.Skill ReviewingSkill { get; set; }
        public virtual User.User ProccessingUser { get; set; }
        public virtual User.User ReviewingUser { get; set; }
        public virtual ProjectTask ParentTask { get; set; }
        public virtual FormulaProject FormulaProject { get; set; }
        public virtual FormulaTask FormulaTask { get; set; }

        #endregion

        #region Navigation Collections
        public virtual List<ProjectTaskComment> ProjectTaskComments { get; set; } = new List<ProjectTaskComment>();
        public virtual List<CreditLog> CreditLogs { get; set; } = new List<CreditLog>();
        public virtual List<ProjectTaskDependency> ParentTasks { get; set; } = new List<ProjectTaskDependency>();
        public virtual List<ProjectTaskDependency> ChildTasks { get; set; } = new List<ProjectTaskDependency>();
        public virtual ICollection<ProjectTaskUser> ProjectTaskUsers { get; set; } = new List<ProjectTaskUser>();
        public virtual ICollection<ProjectTaskConditionOption> AssignedConditionOptions { get; set; } = new List<ProjectTaskConditionOption>();
        public virtual List<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
        public virtual List<TaskJob> TaskJobs { get; set; } = new List<TaskJob>();
        public virtual ICollection<ResourceProjectTask> ResourceProjectTask { get; set; } = new List<ResourceProjectTask>();
        public virtual List<NotificationSetting> NotificationSettings { get; set; } = new List<NotificationSetting>();
        public virtual ICollection<ProjectTask> FormulaProjectTask { get; set; } = new List<ProjectTask>();
        public virtual ICollection<ProjectTaskVendor> ProjectTaskVendors { get; set; } = new List<ProjectTaskVendor>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        #endregion
    }
}


