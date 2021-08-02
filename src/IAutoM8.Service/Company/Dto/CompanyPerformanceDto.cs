using System;

namespace IAutoM8.Service.Company.Dto
{
    public class CompanyPerformanceDto
    {
        public Guid Id { get; set; }
        public string Team { get; set; }
        public string Skill { get; set; }
        public string Formula { get; set; }
        public int FormulaId { get; set; }
        public int FormulaTaskId { get; set; }
        public string Task { get; set; }
        public decimal Price { get; set; }
        public int Reviews { get; set; }
        public int Rating { get; set; }
        public int NoOfWorkers { get; set; }
        public decimal DwellTime { get; set; }
        public decimal AvgDwellTime { get; set; }
        public decimal CompletionTime { get; set; }
        public decimal AvgCompletionTime { get; set; }
        public decimal TurnaroundTime { get; set; }
        public decimal AvgTurnaroundTime { get; set; }
    }
}
