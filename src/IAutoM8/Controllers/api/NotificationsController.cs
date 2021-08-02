using IAutoM8.Controllers.Abstract;
using IAutoM8.Service.Notification.Dto;
using IAutoM8.Service.Notification.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IAutoM8.Controllers.api
{
    [Authorize]
    public class NotificationsController : BaseController
    {
        private readonly INotificationManagingService _notificationManagingService;

        public NotificationsController(INotificationManagingService notificationManagingService)
        {
            _notificationManagingService = notificationManagingService;
        }

        [HttpGet("get-all-unread")]
        public async Task<NotificationDetailDto> GetUnreadMessages()
        {
            return await _notificationManagingService.GetUnreadMessages();
        }

        [HttpGet]
        public async Task<NotificationDetailDto> GetMessages([FromQuery]int page, [FromQuery]int perPage, [FromQuery]string filterSearch)
        {
            return await _notificationManagingService.GetMessages(page, perPage, filterSearch);
        }

        [HttpPost]
        public async Task ReadMessage([FromBody]int id)
        {
            await _notificationManagingService.ReadMessage(id);
        }

        [HttpPost("read-all")]
        public async Task ReadAllMessages()
        {
            await _notificationManagingService.ReadAllMessages();
        }

        [HttpDelete("{id:int}")]
        public async Task DeleteMessage(int id)
        {
            await _notificationManagingService.DeleteMessage(id);
        }

        [HttpGet("send-nudge/{projectTaskId:int}")]
        public async Task<object> SendNudgeNotification(int projectTaskId)
        {
            return await _notificationManagingService.SendNudgeNotification(projectTaskId);
        }

        [HttpPut("read-comments/{taskId:int}")]
        public async Task ReadComments(int taskId)
        {
            await _notificationManagingService.ReadAllComments(taskId);
        }

    }
}
