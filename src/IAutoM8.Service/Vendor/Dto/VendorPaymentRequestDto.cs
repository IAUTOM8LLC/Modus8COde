using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Vendor.Dto
{
    public class VendorPaymentRequestDto
    {
        public int Id { get; set; }
        public Guid OwnerGuid { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
    }
}
