namespace IAutoM8.Domain.Models.Formula
{
    public class FormulaProjectCategory
    {
        public int FormulaProjectId { get; set; }
        public int CategoryId { get; set; }

        public virtual FormulaProject FormulaProject { get; set; }
        public virtual Category Category { get; set; }
    }
}
