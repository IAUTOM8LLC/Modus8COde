using System;

namespace IAutoM8.Service.Notification.Models
{
    public class TaskSummaryDetail
    {
        public string Title { get; set; }
        public DateTime Time { get; set; }
        public override string ToString()
        {
            return Title + "  -  " + Time.ToUniversalTime();
        }
    }
}
