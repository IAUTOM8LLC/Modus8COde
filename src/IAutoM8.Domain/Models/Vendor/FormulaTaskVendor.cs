using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Domain.Models.Vendor
{
    public class FormulaTaskVendor
    {
        public int Id { get; set; }
        public int FormulaTaskId { get; set; }
        public Guid VendorGuid { get; set; }

        public decimal Price { get; set; }
        public FormulaRequestStatus Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }
        public Guid? ChildCompanyWorkerID { get; set; }

        #region Navigation props
        public virtual User.User Vendor { get; set; }
        public virtual FormulaTask FormulaTask { get; set; }
        #endregion
    }
}
