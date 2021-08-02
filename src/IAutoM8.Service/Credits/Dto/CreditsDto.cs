using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.CreditsService.Dto
{
    public class CreditsDto
    {
        public decimal ReservedCredits { get; set; }
        public DateTime? LastUpdate { get; set; }
        public decimal AvailableCredits { get; set; }
        public int PrepaidTasksCount { get; set; }
        public float Percentage { get; set; }
        public float Fee { get; set; }
    }
}
