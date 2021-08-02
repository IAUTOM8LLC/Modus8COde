using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Domain.Models.Formula.Task
{
    public class FormulaTaskChecklist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? FormulaTaskId { get; set; }
        public TodoType? Type { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int? BussinessEntityID { get; set; }
    }
}
