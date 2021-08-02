using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.FormulaTasks.Dto
{
    public class DisabledFormulaTaskDto
    {
        public int? ParentFormulaId { get; set; }
        public int? ChildFormulaId { get; set; }
        public int? ParentFormulaTaskId { get; set; }
    }
}
