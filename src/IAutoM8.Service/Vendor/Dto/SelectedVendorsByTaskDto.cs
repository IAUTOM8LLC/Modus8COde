using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Vendor.Dto
{
    public class SelectedVendorsByTaskDto
    {
        public int FormulaTaskId { get; set; }
        public List<Guid> OutsourcerIds { get; set; }
    }
}
