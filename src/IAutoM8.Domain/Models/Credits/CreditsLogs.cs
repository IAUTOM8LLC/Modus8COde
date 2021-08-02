using System;
using System.Collections.Generic;
using System.Text;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Global.Enums;

namespace IAutoM8.Domain.Models.Credits
{
    public class CreditLog
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountWithTax { get; set; }
        public DateTime HistoryTime { get; set; }
        public CreditsLogType Type { get; set; }
        public int? ProjectTaskId { get; set; }
        public Guid ManagerId { get; set; }
        public Guid? VendorId { get; set; }

        public virtual User.User Manager { get; set; }
        public virtual ProjectTask ProjectTask { get; set; }
        public virtual User.User Vendor { get; set; }
    }
}
