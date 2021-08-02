using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Domain.Models.Credits
{
    public class TransferRequest
    {
        public int Id { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal RequestedAmountWithTax { get; set; }
        public Guid VendorId { get; set; }
        public DateTime RequestTime { get; set; }
        public bool IsDone { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }

        public virtual User.User Vendor { get; set; }
    }
}
