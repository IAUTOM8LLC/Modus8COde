using IAutoM8.Global.Enums;
using System;
using System.Collections.Generic;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Domain.Models.Project.Task;

namespace IAutoM8.Domain.Models.Formula
{
    public class FormulaProject
    {
        public FormulaProject() { }

        public FormulaProject(FormulaProject formula, Guid userGuid)
        {
            Name = formula.Name;
            Description = formula.Description;
            DateCreated = DateTime.UtcNow;
            IsDeleted = false;
            ShareType = FormulaShareType.NotShared;
            OwnerGuid = userGuid;
            OriginalFormulaProject = formula;
        }

        public int Id { get; set; }
        public Guid OwnerGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FormulaOverview { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
        public FormulaShareType ShareType { get; set; }
        public bool IsResharingAllowed { get; set; }
        public int? OriginalFormulaProjectId { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsStarred { get; set; }
        public int? Status { get; set; }
        public int? PublicVaultFormulaProjectID { get; set; }
        public int? CopyFormulaProjectID { get; set; }

        #region Navigation Properties

        public virtual User.User Owner { get; set; }
        public FormulaProject OriginalFormulaProject { get; set; }

        #endregion

        #region Navigation Collections

        public virtual ICollection<FormulaTask> FormulaTasks { get; set; } = new List<FormulaTask>();
        public virtual ICollection<FormulaShare> FormulaShares { get; set; } = new List<FormulaShare>();
        public virtual ICollection<ResourceFormula> ResourceFormula { get; set; } = new List<ResourceFormula>();
        public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
        public virtual ICollection<FormulaProject> ChildFormulaProjects { get; set; } = new List<FormulaProject>();
        /// <summary>
        /// Collection of FormulaProject that reference this project as a formula group
        /// </summary>
        public virtual ICollection<FormulaTask> InternalFormulaTasks { get; set; } = new List<FormulaTask>();
        public virtual ICollection<FormulaProjectCategory> FormulaProjectCategories { get; set; } = new List<FormulaProjectCategory>();

        #endregion
    }
}
