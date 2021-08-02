using System;
using System.Collections.Generic;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Resource;

namespace IAutoM8.Domain.Models.Project
{
    public class Project
    {
        public int Id { get; set; }
        public Guid OwnerGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsDeleted { get; set; }

        public int? ClientId { get; set; }
        public int? ParentProjectId { get; set; }

        #region Navigation Properties

        public virtual User.User Owner { get; set; }
        public Client.Client Client { get; set; }
        public virtual Project Parent { get; set; }
        #endregion

        #region Navigation Collections

        public virtual ICollection<Project> Children { get; set; } = new List<Project>();
        public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public virtual ICollection<User.UserProject> UserProjects { get; set; } = new List<User.UserProject>();
        public virtual ICollection<ResourceProject> ResourceProject { get; set; } = new List<ResourceProject>();

        #endregion
    }
}
