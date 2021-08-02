using System;
using System.Collections.Generic;
using System.Text;
using IAutoM8.Domain.Models.User;

namespace IAutoM8.Domain.Models.Credits
{
    public class Credits
    {
        public User.User User { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalCredits { get; set; }
        public DateTime LastUpdate { get; set; }
        public string BraintreeCustomerId { get; set; }
    }
}
