using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Payment.Dto
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal RequestedAmountWithTax { get; set; }
        public Guid VendorId { get; set; }
        public DateTime RequestTime { get; set; }
        public bool IsDone { get; set; }
        public string VendorName { get; set; }
        public bool IsChecked { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
    }

    public class PaymentRequestDto
    {
        public List<PaymentDto> Payments { get; set; }
        public int isAccept { get; set; }
    }

}
