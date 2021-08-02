using IAutoM8.Domain.Models.Credits;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.Team;
using IAutoM8.Domain.Models.Vendor;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace IAutoM8.Domain.Models.User
{
    public class User : IdentityUser<Guid>
    {
        public Guid? OwnerId { get; set; }
        public bool IsLocked { get; set; }
        public bool IsPayed { get; set; }

        #region Navigation Properties

        public virtual User Owner { get; set; }
        public virtual Business.Business Business { get; set; }
        public virtual Credits.Credits Credits { get; set; }
        public virtual UserProfile Profile { get; set; } = new UserProfile();

        #endregion

        #region Navigation Collections

        public virtual List<ProjectTaskComment> ProjectTaskComments { get; set; } = new List<ProjectTaskComment>();
        public virtual List<FormulaProject> UserCreatedFormulaProjects { get; set; } = new List<FormulaProject>();
        public virtual List<FormulaTask> UserCreatedFormulaTasks { get; set; } = new List<FormulaTask>();
        public virtual List<Project.Project> UserCreatedProjects { get; set; } = new List<Project.Project>();
        public virtual List<ProjectTask> UserCreatedTasks { get; set; } = new List<ProjectTask>();
        public virtual List<FormulaShare> AccessibleFormulas { get; set; } = new List<FormulaShare>();
        public virtual List<User> AssignUsers { get; set; } = new List<User>();
        public virtual IList<UserRole> Roles { get; set; } = new List<UserRole>();
        public virtual List<UserProject> UserProjects { get; set; } = new List<UserProject>();
        public virtual ICollection<TeamUser> TeamUsers { get; set; } = new List<TeamUser>();
        public virtual ICollection<Team.Team> Teams { get; set; } = new List<Team.Team>();
        public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
        public virtual ICollection<ProjectTaskUser> ProjectTaskUsers { get; set; } = new List<ProjectTaskUser>();
        public virtual ICollection<Skill.Skill> Skills { get; set; } = new List<Skill.Skill>();
        public virtual ICollection<ProjectTask> ProccessingTasks { get; set; } = new List<ProjectTask>();
        public virtual ICollection<ProjectTask> ReviewingTasks { get; set; } = new List<ProjectTask>();
        public virtual ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
        public virtual ICollection<CreditLog> ManagerCreditLogs { get; set; } = new List<CreditLog>();
        public virtual ICollection<TransferRequest> TransferRequests { get; set; } = new List<TransferRequest>();
        public virtual ICollection<CreditLog> VendorCreditLogs { get; set; } = new List<CreditLog>();
        public virtual List<Client.Client> Clients { get; set; } = new List<Client.Client>();
        public virtual List<NotificationSetting> NotificationSettings { get; set; } = new List<NotificationSetting>();
        public virtual List<FormulaTeam> FormulaTeams { get; set; }

        public virtual ICollection<FormulaTaskVendor> FormulaTaskVendors { get; set; } = new List<FormulaTaskVendor>();
        public virtual ICollection<ProjectTaskVendor> ProjectTaskVendors { get; set; } = new List<ProjectTaskVendor>();
        public virtual ICollection<Notification> SenderNotifications { get; set; } = new List<Notification>();
        public virtual ICollection<Notification> RecepientNotifications { get; set; } = new List<Notification>();

        #endregion
    }
}
