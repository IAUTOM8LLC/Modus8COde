using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class ProjectTaskOutsourceDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public DateTime? Date { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public double? AvgWorking { get; set; }
        public double? AvgResponding { get; set; }
        public double? AvgRating { get; set; }
        public double? AvgMessaging { get; set; }
    }
}
