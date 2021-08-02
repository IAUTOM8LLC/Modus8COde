
using IAutoM8.Global.Enums;

namespace IAutoM8.Service.Formula.Dto
{
    public class FormulaShareStatusDto
    {
        public FormulaShareType ShareType { get; set; }
        public bool IsResharingAllowed { get; set; }
    }
}
