using System.Collections.Generic;

namespace IAutoM8.Service.Notification.Models
{
    public class WorkerSummary
    {
        public List<TaskSummaryDetail> Upcommings { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> InProgressings { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> Proccessings { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> NeedReviews { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> Overdues { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> DeclineReviews { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> Completed { get; set; } = new List<TaskSummaryDetail>();
    }
}
