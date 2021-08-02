using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Credits.Dto
{
    public class TransferRequestDto
    {
        public DateTime RequestTime { get; set; }
        public decimal RequestedAmountWithTax { get; set; }
    }
}
