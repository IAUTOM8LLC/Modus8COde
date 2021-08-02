using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.WebSockets.Stores.Models
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public bool IsRead { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public string SenderName { get; set; }
        public DateTime CreateDate { get; set; }
        public NotificationType NotificationType { get; set; }
    }
}
