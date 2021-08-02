namespace IAutoM8.Service.Notification.Models
{
    public class DailySummaryModel
    {
        public string FullName { get; set; }
        public WorkerSummary WorkerSummary { get; set; }
        public ManagerSummary ManagerSummary { get; set; }
    }
}
