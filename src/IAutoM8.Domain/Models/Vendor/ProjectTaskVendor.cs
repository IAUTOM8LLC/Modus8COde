using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Domain.Models.Vendor
{
    public class ProjectTaskVendor
    {
        public int Id { get; set; }
        public int ProjectTaskId { get; set; }
        public Guid VendorGuid { get; set; }

        public decimal Price { get; set; }
        public ProjectRequestStatus Status { get; set; }

        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }

        #region Navigation props
        public virtual User.User Vendor { get; set; }
        public virtual ProjectTask ProjectTask { get; set; }
        #endregion
    }
}
