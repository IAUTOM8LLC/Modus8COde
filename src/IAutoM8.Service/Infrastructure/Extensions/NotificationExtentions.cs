using IAutoM8.Service.Notification.Dto;
using System.Collections.Generic;

namespace IAutoM8.Service.Infrastructure.Extensions
{
    public static class NotificationExtentions
    {
        public static IList<NotificationSettingDto> ResetIds(this IList<NotificationSettingDto> settings)
        {
            foreach (var setting in settings)
            {
                setting.Id = 0;
            }
            return settings;
        }
    }
}
