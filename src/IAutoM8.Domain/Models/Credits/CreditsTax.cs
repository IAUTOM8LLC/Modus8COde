using System;
using System.Collections.Generic;
using System.Text;
using IAutoM8.Global.Enums;

namespace IAutoM8.Domain.Models.Credits
{
    public class CreditsTax
    {
        public int Id { get; set; }
        public float Percentage { get; set; }
        public float Fee { get; set; }
        public CreditsTaxType Type { get; set; }
    }
}
