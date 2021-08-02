using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Vendor.Dto
{
    public class VendorPerformanceDto
    {
        public int Id { get; set; }
        public string Team { get; set; }
        public string Skill { get; set; }
        public string Formula { get; set; }
        public string Task { get; set; }
        public int Reviews { get; set; }
        public int Rating { get; set; }
        public decimal Price { get; set; }
        public decimal DwellTime { get; set; }
        public decimal AvgDwellTime { get; set; }
        public decimal CompletionTime { get; set; }
        public decimal AvgCompletionTime { get; set; }
        public decimal TurnaroundTime { get; set; }
        public decimal AvgTurnaroundTime { get; set; }
    }
}
