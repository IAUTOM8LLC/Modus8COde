using System.Collections.Generic;

namespace IAutoM8.Service.Notification.Models
{
    public class DailyFullTemplateModel
    {
        public string FullName { get; set; }
        public string SiteUrl { get; set; }
        public List<DailySummaryModel> Summary { get; set; }
    }
}
