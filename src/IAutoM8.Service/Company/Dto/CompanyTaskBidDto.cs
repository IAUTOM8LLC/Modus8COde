using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Company.Dto
{
    public class CompanyTaskBidDto
    {
        public Guid CompanyId { get; set; }
        public string FullName { get; set; }
        public int BidId { get; set; }
        public int TaskId { get; set; }
        public string FormulaName { get; set; }
        public string TaskName { get; set; }
        public string SkillName { get; set; }
        public string TeamName { get; set; }
        public DateTime Created { get; set; }
    }
}
