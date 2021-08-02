using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Domain.Models.Formula.Task
{
    public class FormulaTaskDisableStatus
    {
        public int ParentFormulaId { get; set; }
        public int ChildFormulaId { get; set; }
        public int InternalChildFormulaId { get; set; }
        public int InternalChildFormulaTaskId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool IsDisabled { get; set; }
    }
}
