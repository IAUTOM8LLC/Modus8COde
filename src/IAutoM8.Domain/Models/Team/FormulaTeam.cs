using System;
using System.Collections.Generic;
using IAutoM8.Domain.Models.Formula.Task;

namespace IAutoM8.Domain.Models.Team
{
    public class FormulaTeam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid OwnerGuid { get; set; }

        #region Navigation Props
        public virtual User.User Owner { get; set; }
        #endregion

        #region Navigation Collections
        public virtual List<FormulaTask> AssignedTasks { get; set; } = new List<FormulaTask>();
        public virtual List<FormulaTask> ReviewingTasks { get; set; } = new List<FormulaTask>();
        #endregion
    }
}
