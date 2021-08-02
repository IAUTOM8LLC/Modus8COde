using IAutoM8.Controllers.Abstract;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Dto;
using IAutoM8.Service.Notification.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Controllers.api
{
    [Authorize(Roles = OwnerOrManager)]
    public class NotificationSettingsController : BaseController
    {
        private readonly INotificationSettingsService _notificationSettingsService;
        private readonly ClaimsPrincipal _principal;

        public NotificationSettingsController(INotificationSettingsService notificationSettingsService, ClaimsPrincipal principal)
        {
            _notificationSettingsService = notificationSettingsService;
            _principal = principal;
        }

        [HttpGet("task/{taskId}")]
        public async Task<List<NotificationSettingDto>> GetTaskSettings([FromRoute] int taskId)
        {
            return await _notificationSettingsService.GetTaskSettingsAsync(_principal.GetOwnerId(), _principal.GetUserId(), taskId);
        }

        [HttpGet("user/{userId}")]
        public async Task<List<NotificationSettingDto>> GetUserSettings([FromRoute]Guid userId)
        {
            return await _notificationSettingsService.GetUserSettingsAsync(_principal.GetOwnerId(), userId);
        }

        [HttpGet("role/{roleId}")]
        public async Task<List<NotificationSettingDto>> GetRoleSettings([FromRoute]Guid roleId)
        {
            return await _notificationSettingsService.GetRoleSettingsAsync(_principal.GetOwnerId(), roleId);
        }

        [HttpGet("role")]
        public async Task<RoleNotificationSettingsDto> GetAllRolesSettings()
        {
            return await _notificationSettingsService.GetRoleSettings(_principal.GetOwnerId());
        }

        [HttpGet]
        public async Task<List<NotificationSettingDto>> GetSettings()
        {
            return await _notificationSettingsService.GetBussinessSettingsAsync(_principal.GetOwnerId());
        }

        [HttpPut("task/{taskId}")]
        public async Task<IActionResult> UpdateTaskSettings([FromRoute]int taskId, [FromBody] List<NotificationSettingDto> settings)
        {
            await _notificationSettingsService.UpdateTaskSettingsAsync(_principal.GetOwnerId(), _principal.GetUserId(), taskId, settings);
            return Ok();
        }

        [HttpPut("user/{userId}")]
        public async Task<IActionResult> UpdateUserSettings([FromRoute]Guid userId, [FromBody] List<NotificationSettingDto> settings)
        {
            await _notificationSettingsService.UpdateUserSettingsAsync(_principal.GetOwnerId(), userId, settings);
            return Ok();
        }

        [HttpPut("role/{roleId}")]
        public async Task<IActionResult> UpdateRoleSettings([FromRoute]Guid roleId, [FromBody] List<NotificationSettingDto> settings)
        {
            await _notificationSettingsService.UpdateRoleSettingsAync(_principal.GetOwnerId(), roleId, settings);
            return Ok();
        }

        [HttpPut("role")]
        public async Task<IActionResult> UpdateRoleSettings([FromBody] RoleNotificationSettingsDto settings)
        {
            await _notificationSettingsService.UpdateRoleSettingsAync(_principal.GetOwnerId(), settings);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] List<NotificationSettingDto> settings)
        {
            await _notificationSettingsService.UpdateBusinessSettingsAsync(_principal.GetOwnerId(), settings);
            return Ok();
        }
    }
}
