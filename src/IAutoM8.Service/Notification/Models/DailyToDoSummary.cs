using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Notification.Models
{
    public class DailyToDoSummary
    { 
        public IEnumerable<TaskSummaryDetail> UpcomingTasks { get; set; }
        public IEnumerable<TaskSummaryDetail> ToDoTasks { get; set; }
        public IEnumerable<TaskSummaryDetail> NeedsReviewTasks { get; set; }
    }
}
