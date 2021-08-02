using System;

namespace IAutoM8.Domain.Models.Formula
{
    public class FormulaShare
    {
        public int FormulaProjectId { get; set; }
        public Guid  UserId { get; set; }

        public virtual FormulaProject FormulaProject { get; set; }
        public virtual User.User AccessHolder { get; set; }
    }
}
