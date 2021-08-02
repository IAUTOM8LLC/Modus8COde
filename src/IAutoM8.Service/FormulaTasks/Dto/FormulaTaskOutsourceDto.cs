using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Service.FormulaTasks.Dto
{
    public class FormulaTaskOutsourceDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
        public decimal? Price { get; set; }
        public double? AvgWorking { get; set; }
        public double? AvgResponding { get; set; }
        public double? AvgRating { get; set; }
        public double? AvgMessaging { get; set; }
        public int TaskCompleted { get; set; }
        public string ProfileImage { get; set; }
        public string Role { get; set; }
        public string OwnerId { get; set; }
    }
}
