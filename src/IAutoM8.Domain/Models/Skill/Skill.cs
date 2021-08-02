using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Team;
using IAutoM8.Global.Enums;
using System;
using System.Collections.Generic;

namespace IAutoM8.Domain.Models.Skill
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public Guid OwnerGuid { get; set; }
        public bool IsGlobal { get; set; }
        public int? PublicVaultSkillID { get; set; }
        public int? Status { get; set; }

        #region Navigation Properties

        public virtual User.User Owner { get; set; }

        #endregion

        #region Navigation Collections

        public ICollection<TeamSkill> TeamSkills { get; set; } = new List<TeamSkill>();
        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
        public ICollection<ProjectTask> AssignedTasks { get; set; } = new List<ProjectTask>();
        public ICollection<ProjectTask> ReviewingTasks { get; set; } = new List<ProjectTask>();
        public virtual List<FormulaTask> AssignedFormulaTasks { get; set; } = new List<FormulaTask>();
        public virtual List<FormulaTask> ReviewingFormulaTasks { get; set; } = new List<FormulaTask>();

        #endregion
    }
}
