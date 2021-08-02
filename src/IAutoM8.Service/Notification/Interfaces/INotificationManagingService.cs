using IAutoM8.Service.Notification.Dto;
using System.Threading.Tasks;

namespace IAutoM8.Service.Notification.Interfaces
{
    public interface  INotificationManagingService
    {
        Task<NotificationDetailDto> GetUnreadMessages();
        Task<NotificationDetailDto> GetMessages(int page, int perPage, string filterSearch);
        Task ReadAllMessages();
        Task ReadMessage(int id);
        Task DeleteMessage(int id);
        Task<object> SendNudgeNotification(int projectTaskId);
        Task ReadAllComments(int taskId);
    }
}
