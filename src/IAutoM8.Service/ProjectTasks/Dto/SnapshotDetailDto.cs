using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
   public class SnapshotDetailDto
    {
        [Key]
        public Guid VENDOR_ID { get; set; }
        public string Vendor_FullName { get; set; }
        public int Invites { get; set; }
        public int Active { get; set; }
        public int AtRisk { get; set; }
        public int Overdue { get; set; }
        public decimal? QueueRevenue { get; set; }
        public int Lost { get; set; }
        public int TotalCompleted { get; set; }
        public decimal? TotalRevenue { get; set; }
    }
}
