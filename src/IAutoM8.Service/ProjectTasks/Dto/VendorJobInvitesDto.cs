using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
   public class VendorJobInvitesDto
    {
        //[Key]
        //public Guid VENDOR_ID { get; set; }
        public int TaskId { get; set; }
        public string FormulaName { get; set; }
        public string TaskName { get; set; }
        public string SkillName { get; set; }
        public string TeamName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? SentOn { get; set; }
        public decimal? DurationHours { get; set; }
        public int? TimeLeftHours { get; set; }
        public int? ProjectTaskVendorId { get; set; }
    }
}
