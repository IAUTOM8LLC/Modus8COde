using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.FormulaTasks.Dto
{
    public class AddFormulaTaskDisableStatusDto
    {
        public int ParentFormulaId { get; set; }
        public int ChildFormulaId { get; set; }
        public int InternalChildFormulaId { get; set; }
        public int InternalChildFormulaTaskId { get; set; }
    }
}
