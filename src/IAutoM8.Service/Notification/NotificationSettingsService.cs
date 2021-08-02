using AutoMapper;
using IAutoM8.Domain.Models;
using IAutoM8.Domain.Models.User;
using IAutoM8.Global;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Options;
using IAutoM8.Repository;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Dto;
using IAutoM8.Service.Notification.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.Service.Notification
{
    public class NotificationSettingsService: INotificationSettingsService
    {
        private readonly IRepo _repo;
        public NotificationSettingsService(IRepo repo)
        {
            _repo = repo;
        }

        public async Task<List<NotificationSettingDto>> GetBussinessSettingsAsync(Guid ownerGuid)
        {
            var settings = await _repo.Read<NotificationSetting>()
                .Where(n =>
                    n.BussinessId == ownerGuid
                    && n.RoleId == null
                    && n.UserId == null)
                .ToListAsync();

            if (settings.Count == 0)
                return GetDefaultSettings();
            return Mapper.Map<List<NotificationSettingDto>>(settings);
        }

        
        public async Task<List<NotificationSettingDto>> GetRoleSettingsAsync(Guid ownerGuid, Guid roleGuid)
        {
            var settings = await _repo.Read<NotificationSetting>()
                .Where(n =>
                    n.BussinessId == ownerGuid
                    && n.RoleId == roleGuid)
                .ToListAsync();

            if (settings.Count == 0)
                return (await GetBussinessSettingsAsync(ownerGuid)).ResetIds().ToList();

            return Mapper.Map<List<NotificationSettingDto>>(settings);
        }

        public async Task<List<NotificationSettingDto>> GetUserSettingsAsync(Guid ownerGuid, Guid userGuid)
        {
            var settings = await _repo.Read<NotificationSetting>()
                .Where(n =>
                    n.BussinessId == ownerGuid
                    && n.UserId == userGuid
                    && n.TaskId == null)
                .ToListAsync();

            if (settings.Count == 0)
            {
                var userRole = _repo.Read<UserRole>()
                    .First(u => u.UserId == userGuid);
                return (await GetRoleSettingsAsync(ownerGuid, userRole.RoleId)).ResetIds().ToList();
            }

            return Mapper.Map<List<NotificationSettingDto>>(settings);
        }

        public async Task<List<NotificationSettingDto>> GetTaskSettingsAsync(Guid ownerGuid, Guid userGuid, int taskId)
        {
            var settings = await _repo.Read<NotificationSetting>()
                .Where(n =>
                    n.BussinessId == ownerGuid
                    && n.UserId == userGuid
                    && n.TaskId == taskId)
                .ToListAsync();

            if (settings.Count == 0)
                return (await GetUserSettingsAsync(ownerGuid, userGuid))
                    .ResetIds()
                    .Where(x => NotificationGroup.TaskNotifications.Contains(x.Type))
                    .ToList();

            return Mapper.Map<List<NotificationSettingDto>>(settings);
        }

        public async Task UpdateBusinessSettingsAsync(Guid ownerGuid, List<NotificationSettingDto> notificationSettings)
        {
            using (var trx = _repo.Transaction())
            {
                if (notificationSettings.Any(s => s.Id == 0))
                {
                    var newSettings = GetDefaultSettings().Where(s => NotificationGroup.BussinessNotifications.Contains(s.Type));
                    foreach (var notificationSetting in newSettings)
                    {
                        var temp = notificationSettings.FirstOrDefault(s => s.Type == notificationSetting.Type) ?? new NotificationSettingDto
                            {
                                Type = notificationSetting.Type,
                                Enabled = true
                            };
                        var setting = Mapper.Map<NotificationSetting>(temp);
                        setting.BussinessId = ownerGuid;
                        await trx.AddAsync(setting);
                    }
                }
                else
                {
                    var settings = await trx.Track<NotificationSetting>()
                        .Where(s =>
                            s.BussinessId == ownerGuid
                            && s.RoleId == null
                            && s.UserId == null)
                        .ToListAsync();
                    UpdateSettings(settings, notificationSettings);
                }
                await trx.SaveAndCommitAsync();
            }
        }

        public async Task UpdateRoleSettingsAync(Guid ownerGuid, Guid roleGuid, List<NotificationSettingDto> notificationSettings)
        {
            using (var trx = _repo.Transaction())
            {
                if (notificationSettings.Any(s => s.Id == 0))
                {
                    var newSettings = GetDefaultSettings().Where(s => NotificationGroup.RoleNotifications.Contains(s.Type));
                    foreach (var notificationSetting in newSettings)
                    {
                        var temp = notificationSettings.FirstOrDefault(s => s.Type == notificationSetting.Type) ?? new NotificationSettingDto
                        {
                            Type = notificationSetting.Type,
                            Enabled = true
                        };
                        var setting = Mapper.Map<NotificationSetting>(temp);
                        setting.BussinessId = ownerGuid;
                        setting.RoleId = roleGuid;
                        await trx.AddAsync(setting);
                    }
                }
                else
                {
                    var settings = await trx.Track<NotificationSetting>()
                        .Where(s =>
                            s.BussinessId == ownerGuid
                            && s.RoleId == roleGuid)
                        .ToListAsync();
                    UpdateSettings(settings, notificationSettings);
                }
                await trx.SaveAndCommitAsync();
            }
        }

        public async Task UpdateRoleSettingsAync(Guid ownerGuid, RoleNotificationSettingsDto settings)
        {
            var roles = await _repo.Read<Role>().ToListAsync();
            await UpdateRoleSettingsAync(ownerGuid, roles.First(r => r.Name == UserRoles.Owner).Id, settings.Owner);
            await UpdateRoleSettingsAync(ownerGuid, roles.First(r => r.Name == UserRoles.Manager).Id, settings.Manager);
            await UpdateRoleSettingsAync(ownerGuid, roles.First(r => r.Name == UserRoles.Worker).Id, settings.Worker);
        }

        public async Task UpdateUserSettingsAsync(Guid ownerGuid, Guid userGuid, List<NotificationSettingDto> notificationSettings)
        {
            using (var trx = _repo.Transaction())
            {
                if (notificationSettings.Any(s => s.Id == 0))
                {
                    var newSettings = GetDefaultSettings().Where(s => NotificationGroup.UserNotifications.Contains(s.Type));
                    foreach (var notificationSetting in newSettings)
                    {
                        var temp = notificationSettings.FirstOrDefault(s => s.Type == notificationSetting.Type) ?? new NotificationSettingDto
                        {
                            Type = notificationSetting.Type,
                            Enabled = true
                        };
                        var setting = Mapper.Map<NotificationSetting>(temp);
                        setting.BussinessId = ownerGuid;
                        setting.UserId = userGuid;
                        await trx.AddAsync(setting);
                    }
                }
                else
                {
                    var settings = await trx.Track<NotificationSetting>()
                        .Where(s =>
                            s.BussinessId == ownerGuid
                            && s.UserId == userGuid
                            && s.TaskId == null)
                        .ToListAsync();
                    UpdateSettings(settings, notificationSettings);
                }
                await trx.SaveAndCommitAsync();
            }
        }

        public async Task UpdateTaskSettingsAsync(Guid ownerGuid, Guid userGuid, int taskId, List<NotificationSettingDto> notificationSettings)
        {
            using (var trx = _repo.Transaction())
            {
                if (notificationSettings.Any(s => s.Id == 0))
                {
                    var newSettings = GetDefaultSettings().Where(s => NotificationGroup.TaskNotifications.Contains(s.Type));
                    foreach (var notificationSetting in newSettings)
                    {
                        var temp = notificationSettings.FirstOrDefault(s => s.Type == notificationSetting.Type) ?? new NotificationSettingDto
                        {
                            Type = notificationSetting.Type,
                            Enabled = true
                        };
                        var setting = Mapper.Map<NotificationSetting>(temp);
                        setting.BussinessId = ownerGuid;
                        setting.UserId = userGuid;
                        setting.TaskId = taskId;
                        await trx.AddAsync(setting);
                    }
                }
                else
                {
                    var settings = await trx.Track<NotificationSetting>()
                        .Where(s =>
                            s.BussinessId == ownerGuid
                            && s.UserId == userGuid
                            && s.TaskId == taskId)
                        .ToListAsync();
                    UpdateSettings(settings, notificationSettings);
                }
                await trx.SaveAndCommitAsync();
            }
        }

        public async Task<bool> IsEnabledAsync(NotificationType type, Guid ownerGuid, Guid userGuid, int? taskId = null)
        {
            var result = await _repo.Read<NotificationSetting>()
                .Where(s =>
                    s.BussinessId == ownerGuid && s.Type == type &&
                    (s.TaskId == taskId && s.UserId == userGuid
                     || s.TaskId == null && s.UserId == userGuid
                     || s.RoleId != null && s.Role.UserRoles.Any(ur => ur.UserId == userGuid)
                     || s.RoleId == null && s.UserId == null && s.TaskId == null))
                .OrderByDescending(s => s.TaskId == taskId && s.UserId == userGuid)
                .ThenByDescending(s => s.TaskId == null && s.UserId == userGuid)
                .ThenByDescending(s => s.RoleId != null && s.Role.UserRoles.Any(ur => ur.UserId == userGuid))
                .ThenByDescending(s => s.RoleId == null && s.UserId == null && s.TaskId == null)
                .FirstOrDefaultAsync();

            if (result == null)
                return true;

            return result.Enabled;
        }

        public async Task<bool> IsEnabledAsync(NotificationType type, Guid ownerGuid, string email)
        {
            var result = await _repo.Read<NotificationSetting>()
                .Where(s =>
                    s.BussinessId == ownerGuid && s.Type == type &&
                    (s.UserId != null && s.User.Email == email //TODO: need to use NormalizedEmail???
                     || s.RoleId != null && s.Role.UserRoles.Any(ur => ur.User.Email == email)
                     || s.RoleId == null && s.UserId == null && s.TaskId == null))
                .OrderByDescending(s => s.UserId != null && s.User.Email == email)
                .ThenByDescending(s => s.RoleId != null && s.Role.UserRoles.Any(ur => ur.User.Email == email))
                .ThenByDescending(s => s.RoleId == null && s.UserId == null && s.TaskId == null)
                .FirstOrDefaultAsync();

            if (result == null)
                return true;

            return result.Enabled;
        }

        public List<NotificationSettingDto> GetDefaultSettings()
        {
            return Enum
                .GetValues(typeof(NotificationType))
                .Cast<NotificationType>()
                .Select(a => new NotificationSettingDto
                {
                    Type = a,
                    Enabled = true
                })
                .ToList();
        }

        public bool IsSettingsChanged(IList<NotificationSettingDto> originalSettings, IList<NotificationSettingDto> currentSettings)
        {
            return originalSettings.Any(
                s => currentSettings.Any(ms => ms.Type == s.Type && ms.Id == s.Id && s.Enabled != ms.Enabled));
        }

        public async Task<RoleNotificationSettingsDto> GetRoleSettings(Guid bussinesGuid)
        {
            var roles = await _repo.Read<Role>().ToListAsync();
            return new RoleNotificationSettingsDto
            {
                Owner = await GetRoleSettingsAsync(bussinesGuid, roles.First(r => r.Name == UserRoles.Owner).Id),
                Manager = await GetRoleSettingsAsync(bussinesGuid, roles.First(r => r.Name == UserRoles.Manager).Id),
                Worker = await GetRoleSettingsAsync(bussinesGuid, roles.First(r => r.Name == UserRoles.Worker).Id)
            };
        }

        #region Helpers
        private void UpdateSettings(List<NotificationSetting> settings, List<NotificationSettingDto> settingDtos)
        {
            foreach (var setting in settings)
            {
                var dto = settingDtos.FirstOrDefault(s => s.Type == setting.Type);
                if (dto != null)
                {
                    Mapper.Map(dto, setting);
                }
            }
        }
        #endregion
    }
}
