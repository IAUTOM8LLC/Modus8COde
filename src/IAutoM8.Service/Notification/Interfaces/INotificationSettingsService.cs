using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Global.Enums;
using IAutoM8.Service.Notification.Dto;

namespace IAutoM8.Service.Notification.Interfaces
{
    public interface INotificationSettingsService
    {
        Task<List<NotificationSettingDto>> GetBussinessSettingsAsync(Guid ownerGuid);
        Task<List<NotificationSettingDto>> GetUserSettingsAsync(Guid ownerGuid, Guid userGuid);
        Task<List<NotificationSettingDto>> GetRoleSettingsAsync(Guid ownerGuid, Guid roleGuid);
        Task<List<NotificationSettingDto>> GetTaskSettingsAsync(Guid ownerGuid, Guid userGuid, int taskId);
        Task UpdateBusinessSettingsAsync(Guid ownerGuid, List<NotificationSettingDto> notificationSettings);
        Task UpdateRoleSettingsAync(Guid ownerGuid, Guid roleGuid, List<NotificationSettingDto> notificationSettings);
        Task UpdateRoleSettingsAync(Guid ownerGuid, RoleNotificationSettingsDto settings);
        Task UpdateUserSettingsAsync(Guid ownerGuid, Guid userGuid, List<NotificationSettingDto> notificationSettings);
        Task UpdateTaskSettingsAsync(Guid ownerGuid, Guid userGuid, int taskId, List<NotificationSettingDto> notificationSettings);
        Task<bool> IsEnabledAsync(NotificationType type, Guid ownerGuid, Guid userGuid, int? taskId = null);
        Task<bool> IsEnabledAsync(NotificationType type, Guid ownerGuid, string email);
        List<NotificationSettingDto> GetDefaultSettings();
        bool IsSettingsChanged(IList<NotificationSettingDto> originalSettings, IList<NotificationSettingDto> currentSettings);
        Task<RoleNotificationSettingsDto> GetRoleSettings(Guid bussinesGuid);
    }
}
