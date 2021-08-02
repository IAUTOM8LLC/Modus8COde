using System;

namespace IAutoM8.Service.Company.Dto
{
    public class CompanyUserPriceDto
    {
        public Guid CompanyId { get; set; }
        public Guid CompanyWorkerId { get; set; }
        public string CompanyWorkerFullName { get; set; }
        public string Formula { get; set; }
        public int FormulaId { get; set; }
        public int FormulaTaskId { get; set; }
        public string Task { get; set; }
        public decimal CompanyPrice { get; set; }
        public decimal CompanyWorkerPrice { get; set; }
        public int Reviews { get; set; }
        public int Rating { get; set; }
        public int NoOfWorkers { get; set; }

    }
}
