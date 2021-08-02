using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Domain.Models.Vendor
{
    public class FormulaTaskStatistic
    {
        public int Id { get; set; }
        public int FormulaTaskId { get; set; }
        public int ProjectTaskId { get; set; }
        public Guid VendorGuid { get; set; }
        public StatisticType Type { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Completed { get; set; }
        public short? Value { get; set; }
        public int? FormulaTaskStatisticId { get; set; }
    }
}
