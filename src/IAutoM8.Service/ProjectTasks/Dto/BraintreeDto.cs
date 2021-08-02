using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class BraintreeDto
    {
        public decimal Amount { get; set; }
        public string Nonce { get; set; }
    }
}
