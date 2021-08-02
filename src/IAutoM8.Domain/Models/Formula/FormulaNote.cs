using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Domain.Models.Formula
{
    public class FormulaNote
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }
        public int? FormulaId { get; set; }
    }
}
