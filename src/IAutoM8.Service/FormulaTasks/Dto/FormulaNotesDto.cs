using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.FormulaTasks.Dto
{
    public class FormulaNotesDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }
        public int? FormulaId { get; set; }
    }
}
