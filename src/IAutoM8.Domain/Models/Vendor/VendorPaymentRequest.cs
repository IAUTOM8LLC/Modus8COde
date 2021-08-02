using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Domain.Models
{
    public class VendorPaymentRequest
    {
        public int Id { get; set; }
        public Guid OwnerGuid { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
    }
}
