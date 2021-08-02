using System.Collections.Generic;

namespace IAutoM8.Domain.Models.Formula
{
    public class Category
    {
        public int Id { get; set; }//Commented
        public string Name { get; set; }

        public virtual ICollection<FormulaProjectCategory> FormulaProjectCategories { get; set; } = new List<FormulaProjectCategory>();
    }
}
