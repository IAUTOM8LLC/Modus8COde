using IAutoM8.Domain.Models.Abstract.Task;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Domain.Models.Team;
using IAutoM8.Domain.Models.Vendor;
using System;
using System.Collections.Generic;

namespace IAutoM8.Domain.Models.Formula.Task
{
    public class FormulaTask : BaseTask
    {
        public bool IsGlobal { get; set; }

        public int? PublicVaultFormulaTaskID { get; set; }

        public bool? DescNotificationFlag { get; set; }

        public string ReviewerTraining { get; set; }
        public bool IsTrainingLocked { get; set; }
        public int? CopyFormulaTaskID{ get; set; }

        public FormulaTask() { }

        public FormulaTask(FormulaTask task)
        {
            Title = task.Title;
            Description = task.Description;
            Duration = task.Duration;
            StartDelay = task.StartDelay;
            PosX = task.PosX;
            PosY = task.PosY;
            IsAutomated = task.IsAutomated;
            IsInterval = task.IsInterval;
            RecurrenceOptions = task.RecurrenceOptions != null
                ? new RecurrenceOptions
                {
                    Cron = task.RecurrenceOptions.Cron,
                    CronTab = task.RecurrenceOptions.CronTab,
                    EndRecurrenceDate = task.RecurrenceOptions.EndRecurrenceDate,
                    MaxOccurrences = task.RecurrenceOptions.MaxOccurrences,
                    RecurrenceType = task.RecurrenceOptions.RecurrenceType
                }
                : null;
            InternalFormulaProjectId = task.InternalFormulaProjectId;
        }


        #region Foreign Keys

        public Guid OwnerGuid { get; set; }
        public int? TeamId { get; set; }
        public int FormulaProjectId { get; set; }
        public int? AssignedTeamId { get; set; }
        public int? ReviewingTeamId { get; set; }
        public int? AssignedSkillId { get; set; }
        public int? ReviewingSkillId { get; set; }
        public int? OriginalFormulaTaskId { get; set; }
        public int? TaskConditionId { get; set; }
        public int? RecurrenceOptionsId { get; set; }
        public int? InternalFormulaProjectId { get; set; }

        #endregion

        #region Navigation Properties

        public virtual User.User Owner { get; set; }
        public virtual RecurrenceOptions RecurrenceOptions { get; set; }
        public virtual FormulaProject FormulaProject { get; set; }
        public virtual FormulaTaskCondition Condition { get; set; }
        public virtual ICollection<FormulaTaskConditionOption> AssignedConditionOptions { get; set; }
        public virtual Team.Team AssignedFormulaTeam { get; set; }
        public virtual FormulaTeam AssignedTeam { get; set; }
        public virtual FormulaTeam ReviewingTeam { get; set; }
        public virtual Skill.Skill AssignedSkill { get; set; }
        public virtual Skill.Skill ReviewingSkill { get; set; }
        public virtual FormulaTask OriginalFormulaTask { get; set; }
        public virtual FormulaProject InternalFormulaProject { get; set; }

        #endregion

        #region Navigation Collections

        public virtual List<FormulaTaskDependency> ParentTasks { get; set; } = new List<FormulaTaskDependency>();
        public virtual List<FormulaTaskDependency> ChildTasks { get; set; } = new List<FormulaTaskDependency>();
        public virtual ICollection<ResourceFormulaTask> ResourceFormulaTask { get; set; } = new List<ResourceFormulaTask>();
        public virtual ICollection<FormulaTaskVendor> FormulaTaskVendors { get; set; } = new List<FormulaTaskVendor>();
        public virtual ICollection<FormulaTask> ChildFormulaTasks { get; set; } = new List<FormulaTask>();
        public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
        #endregion
    }
}
