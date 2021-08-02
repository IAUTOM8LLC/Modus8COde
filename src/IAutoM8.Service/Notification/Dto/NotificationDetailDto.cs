using IAutoM8.WebSockets.Stores.Models;

namespace IAutoM8.Service.Notification.Dto
{
    public class NotificationDetailDto
    {
        public int? UnreadCount { get; set; }
        public int TotalCount { get; set; }
        public NotificationModel[] Messages { get; set; }
    }
}
