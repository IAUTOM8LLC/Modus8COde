using IAutoM8.Repository;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Dto;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.WebSockets.Stores.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using NotificationEntity = IAutoM8.Domain.Models.Notification;
using IAutoM8.Domain.Models.Project.Task;
using System;
using IAutoM8.Service.CommonService.Interfaces;
using Braintree.Exceptions;

namespace IAutoM8.Service.Notification
{
    public class NotificationManagingService : INotificationManagingService
    {
        private readonly ClaimsPrincipal _principal;
        private readonly IRepo _repo;
        private readonly IDateTimeService _dateTimeService;

        public NotificationManagingService(ClaimsPrincipal principal, IRepo repo, IDateTimeService dateTimeService)
        {
            _principal = principal;
            _repo = repo;
            _dateTimeService = dateTimeService;
        }

        public async Task DeleteMessage(int id)
        {
            var notification = await _repo.Track<NotificationEntity>().FirstAsync(w => w.Id == id);
            if (notification == null || notification.RecipientGuid != _principal.GetUserId())
                return;
            _repo.Remove(notification);
            await _repo.SaveChangesAsync();
        }

        public async Task<object> SendNudgeNotification(int projectTaskId)
        {
            Guid SenderGuid = _principal.GetUserId();

            var notificationEnt = _repo.Read<NotificationEntity>().Where(w => w.TaskId == projectTaskId && w.SenderGuid == SenderGuid
            && w.NotificationType == Global.Enums.NotificationType.SendNudgeNotification).OrderByDescending(x => x.Id).ToList();

            var hourCheck = notificationEnt.Count() == 0 ? false : true;

            if (notificationEnt.Count() < 3)
            {
                double hours = 2;

                if (hourCheck)
                {
                    hours = (_dateTimeService.NowUtc - notificationEnt.FirstOrDefault().CreateDate).TotalHours;
                }

                if (hours > 1)
                {
                    var projectTask = await _repo.Read<ProjectTask>().FirstAsync(w => w.Id == projectTaskId);

                    if (projectTask == null)
                        return null;

                    NotificationEntity notification = new NotificationEntity();
                    notification.CreateDate = _dateTimeService.NowUtc;
                    notification.IsRead = false;
                    notification.Message = "Need more discussion on task " + projectTask.Title;
                    notification.NotificationType = Global.Enums.NotificationType.SendNudgeNotification;
                    notification.RecipientGuid = projectTask.OwnerGuid;
                    notification.SenderGuid = SenderGuid;
                    notification.TaskId = projectTaskId;
                    _repo.Add(notification);

                    await _repo.SaveChangesAsync();

                    return new
                    {
                        id = 3,
                        message = "Nudge notification sent to Task owner."
                    };
                }
                else
                {
                    return new
                    {
                        id = 2,
                        message = "Can not Nudge before one hour of previous Nudge notification."
                    };
                }

            }

            return new
            {
                id = 1,
                message = "You can only Nudge 3 times. You have the option to Cancel this Task."
            };

        }

        public async Task<NotificationDetailDto> GetMessages(int page, int perPage, string filterSearch)
        {
            var userId = _principal.GetUserId();
            var query = _repo.Read<NotificationEntity>()
                .Include(i => i.Sender).ThenInclude(i => i.Profile)
                .Where(w => w.RecipientGuid == userId);
            if (!string.IsNullOrEmpty(filterSearch))
                query = query.Where(w => w.Message.Contains(filterSearch));
            var notifications = await query
                .OrderBy(o => o.IsRead)
                .ThenByDescending(o => o.Id)
                .Skip((page - 1) * perPage).Take(perPage)
                .Select(s => new NotificationModel
                {
                    Id = s.Id,
                    IsRead = s.IsRead,
                    Message = s.Message,
                    Url = s.Url,
                    SenderName = s.SenderGuid.HasValue ? s.Sender.Profile.FullName : "Modus8",
                    CreateDate = s.CreateDate,
                    NotificationType = s.NotificationType
                })
                .ToArrayAsync();
            return new NotificationDetailDto
            {
                TotalCount = await query.CountAsync(),
                Messages = notifications
            };
        }

        public async Task<NotificationDetailDto> GetUnreadMessages()
        {
            var userId = _principal.GetUserId();
            var notifications = await _repo.Read<NotificationEntity>()
                .Include(i => i.Sender).ThenInclude(i => i.Profile)
                .Where(w => w.RecipientGuid == userId && !w.IsRead)
                .OrderByDescending(o => o.Id)
                .Select(s => new NotificationModel
                {
                    Id = s.Id,
                    IsRead = s.IsRead,
                    Message = s.Message,
                    Url = s.Url,
                    SenderName = s.SenderGuid.HasValue ? s.Sender.Profile.FullName : "Modus8",
                    CreateDate = s.CreateDate,
                    NotificationType = s.NotificationType
                })
                .ToArrayAsync();
            return new NotificationDetailDto
            {
                TotalCount = await _repo.Read<NotificationEntity>().CountAsync(w => w.RecipientGuid == userId),
                UnreadCount = notifications.Count(),
                Messages = notifications
            };
        }

        public async Task ReadAllMessages()
        {
            var notifications = await _repo.Track<NotificationEntity>().Where(w => w.RecipientGuid == _principal.GetUserId() && !w.IsRead)
                .ToListAsync();
            notifications.ForEach(notification => notification.IsRead = true);
            await _repo.SaveChangesAsync();
        }

        public async Task ReadMessage(int id)
        {
            var notification = await _repo.Track<NotificationEntity>().FirstAsync(w => w.Id == id);
            if (notification == null || notification.RecipientGuid != _principal.GetUserId())
                return;
            notification.IsRead = true;
            await _repo.SaveChangesAsync();
        }

        public async Task ReadAllComments(int taskId)
        {
            var notifications = await _repo.Track<NotificationEntity>()
                .Where(w => w.TaskId == taskId
                    && w.NotificationType == Global.Enums.NotificationType.TaskCommented
                    && w.RecipientGuid == _principal.GetUserId()
                    && !w.IsRead)
                .ToListAsync();

            notifications.ForEach(notification => notification.IsRead = true);
            await _repo.SaveChangesAsync();
        }
    }
}
