using IAutoM8.Global.Enums;

namespace IAutoM8.Service.Notification.Dto
{
    public class NotificationSettingDto
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public bool Enabled { get; set; }
    }
}
