using System.Collections.Generic;

namespace IAutoM8.Service.Notification.Dto
{
    public class RoleNotificationSettingsDto
    {
        public List<NotificationSettingDto> Owner { get; set; }
        public List<NotificationSettingDto> Manager { get; set; }
        public List<NotificationSettingDto> Worker { get; set; }
    }
}
