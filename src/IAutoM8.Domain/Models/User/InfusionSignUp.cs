using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Domain.Models.User
{
    public class InfusionSignUp
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public int OrderId { get; set; }
        public string Url { get; set; }
        public int? ContactId { get; set; }
    }
}
