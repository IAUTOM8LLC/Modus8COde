using System.Collections.Generic;

namespace IAutoM8.Service.Notification.Models
{
    public class ManagerSummary
    {
        public List<TaskSummaryDetail> ManagerUpcommings { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> ManagerInProccessings { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> ManagerOverdues { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> ManagerNeedReviews { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> ManagerReviewings { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> ManagerDeclineReviews { get; set; } = new List<TaskSummaryDetail>();
        public List<TaskSummaryDetail> ManagerAcceptReviews { get; set; } = new List<TaskSummaryDetail>();
    }
}
