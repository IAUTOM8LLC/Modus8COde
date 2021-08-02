using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.User;
using IAutoM8.Global;
using IAutoM8.Global.Enums;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure;
using IAutoM8.Service.Notification.Dto;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.Notification.Models;
using IAutoM8.Service.Users.Dto;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using IAutoM8.Domain.Models.Team;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Service.SendGrid.Interfaces;
using IAutoM8.Global.Options;
using Microsoft.Extensions.Options;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.WebSockets.Stores.Interfaces;
using System.Security.Claims;
using IAutoM8.Service.Infrastructure.Extensions;
using NotificationEntity = IAutoM8.Domain.Models.Notification;
using IAutoM8.Service.Teams.Dto;
using System.Globalization;

namespace IAutoM8.Service.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationSocketStore _notificationSocketStore;
        private readonly ClaimsPrincipal _principal;
        private readonly IRepo _repo;
        private readonly IEmailService _emailService;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationSettingsService _notificationSettingsService;
        private readonly ITemplateService _templateService;
        private readonly ISendGridService _sendGridService;
        private readonly UserManager<User> _userManager;
        private readonly EmailTemplates _emailTemplates;
        private readonly TransferRequestRecipientsOptions _transferRequestOptions;
        private readonly AccountConfirmationSetting _accountConfirmationSetting;
        private readonly OutsourceConfirmationSetting _outsourceConfirmationSetting;
        private readonly TransferRequestSettings _transferRequestSettings;

        public NotificationService(INotificationSocketStore notificationSocketStore,
            ClaimsPrincipal principal,
            IRepo repo,
            IEmailService emailService,
            IDateTimeService dateTimeService,
            INotificationSettingsService notificationSettingsService,
            ITemplateService templateService,
            ISendGridService sendGridService,
            IOptions<TransferRequestRecipientsOptions> transferRequestOptions,
            UserManager<User> userManager,
            IOptions<EmailTemplates> emailTemplates,
            IOptions<AccountConfirmationSetting> accountConfirmationSetting,
            IOptions<TransferRequestSettings> transferRequestSettings,
            IOptions<OutsourceConfirmationSetting> outsourceConfirmationSetting)
        {
            _notificationSocketStore = notificationSocketStore;
            _principal = principal;
            _repo = repo;
            _emailService = emailService;
            _dateTimeService = dateTimeService;
            _notificationSettingsService = notificationSettingsService;
            _templateService = templateService;
            _userManager = userManager;
            _sendGridService = sendGridService;
            _emailTemplates = emailTemplates.Value;
            _accountConfirmationSetting = accountConfirmationSetting.Value;
            _outsourceConfirmationSetting = outsourceConfirmationSetting.Value;
            _transferRequestOptions = transferRequestOptions.Value;
            _transferRequestSettings = transferRequestSettings.Value;
        }

        public async Task SendAssignToProjectAsync(Guid ownerGuid, string projectName, string userEmail)
        {
            if (await _notificationSettingsService.IsEnabledAsync(NotificationType.AssignToProject, ownerGuid, userEmail))
            {
                await _sendGridService.SendMessage(userEmail, _emailTemplates.EmailNotification, "New Project Assigned To User",
                    new Dictionary<string, string> { { "{{NotificationText}}", $"[New Project] {projectName}" } });
            }
        }

        public async Task SendAssignToTaskAsync(ITransactionScope trx, int taskId)
        {
            var projectTaskUsers = trx.Read<ProjectTaskUser>()
                .Include(i => i.User)
                .Include(i => i.ProjectTask)
                    .ThenInclude(i => i.Project)
                .Where(w => w.ProjectTaskId == taskId && w.ProjectTaskUserType == ProjectTaskUserType.Assigned);
            if (projectTaskUsers.Any())
            {
                var taskInfo = projectTaskUsers.First().ProjectTask;
                var userIds = projectTaskUsers.Select(s => s.UserId).ToArray();
                var url = string.Format(_outsourceConfirmationSetting.TaskInfo, taskInfo.ProjectId, taskInfo.Id);
                var oldRecipients = await trx.Read<NotificationEntity>()
                    .Where(w => w.TaskId == taskId && userIds.Contains(w.RecipientGuid) && !w.IsRead)
                    .Select(s => s.RecipientGuid).ToListAsync();
                var userItems = projectTaskUsers.Select(s => new UserItemDto
                {
                    Id = s.UserId,
                    OwnerId = s.User.OwnerId,
                    Email = s.User.Email
                });
                var newRecepients = projectTaskUsers.Where(w => !oldRecipients.Contains(w.UserId))
                    .Select(s => s.UserId).ToList();
                await AddNotifications(newRecepients, trx, taskId, url, $"You have been assigned to <b>{taskInfo.Title}</b>", NotificationType.AssignToTask);

                var userEmails = await UserEmails(taskId, userItems, NotificationType.AssignToTask);
                if (userEmails.Count == 0)
                    return;
                await _sendGridService.SendMessage(userEmails, _emailTemplates.EmailNotificationExtended, "New Task Assigned To User",
                    new Dictionary<string, string> {
                        { "{{NotificationText}}", $"[New Task] {taskInfo.Title} of project {taskInfo.Project.Name}" },
                        { "{{TaskUrl}}", url }
                    });
            }
        }

        private async Task AddNotifications(List<Guid> newRecepients, ITransactionScope trx, int? taskId, string url, string message, NotificationType notificationType)
        {
            if (newRecepients.Any())
            {
                var userId = _principal.GetOptionalUserId();
                var name = _principal.GetFullName() ?? "Modus8";
                var relativeUrl = url.Replace(_accountConfirmationSetting.SiteUrl, "/");
                foreach (var recepient in newRecepients)
                {
                    var newNotification = new NotificationEntity
                    {
                        IsRead = false,
                        Message = message,
                        SenderGuid = userId,
                        RecipientGuid = recepient,
                        TaskId = taskId,
                        CreateDate = DateTime.UtcNow,
                        Url = relativeUrl,
                        NotificationType = notificationType
                    };
                    await trx.AddAsync(newNotification);
                    await trx.SaveChangesAsync();
                    await _notificationSocketStore.TaskNewNotification(recepient, new WebSockets.Stores.Models.NotificationModel
                    {
                        Id = newNotification.Id,
                        Message = newNotification.Message,
                        Url = newNotification.Url,
                        SenderName = name,
                        IsRead = false,
                        CreateDate = newNotification.CreateDate,
                        NotificationType = notificationType
                    });
                }
            }
        }

        private async Task<IList<UserItemDto>> GetAllTaksUsers(ITransactionScope trx, int taskId,
            bool skipWorkers = false, bool skipManagers = false, bool skipVendors = false)
        {
            var query = trx.Read<ProjectTaskUser>()
                   .Include(i => i.User).Include(i => i.ProjectTask).ThenInclude(i => i.Project)
                   .Where(w => w.ProjectTaskId == taskId);
            if (skipWorkers)
            {
                query = query.Where(w => w.ProjectTaskUserType != ProjectTaskUserType.Assigned);
            }
            if (skipManagers)
            {
                query = query.Where(w => w.ProjectTaskUserType != ProjectTaskUserType.Reviewing);
            }
            var projectTaskUsers = await query
                .Select(s => new UserItemDto
                {
                    Id = s.UserId,
                    OwnerId = s.User.OwnerId,
                    Email = s.User.Email,

                    TaskId = s.ProjectTaskId,
                    ProjectId = s.ProjectTask.ProjectId,
                    ProjectName = s.ProjectTask.Project.Name,
                    TaskName = s.ProjectTask.Title
                }).ToListAsync();

            if (!skipVendors)
            {
                var acceptedVendorRequest = await trx.Read<ProjectTaskVendor>()
                    .Include(i => i.ProjectTask).ThenInclude(i => i.Project)
                    .Where(t => t.Status == ProjectRequestStatus.Accepted && t.ProjectTaskId == taskId)
                    .Select(t => new UserItemDto
                    {
                        Email = t.Vendor.Email,
                        Id = t.Vendor.Id,

                        IsVendor = true,
                        TaskId = t.ProjectTaskId,
                        ProjectId = t.ProjectTask.ProjectId,
                        ProjectName = t.ProjectTask.Project.Name,
                        TaskName = t.ProjectTask.Title
                    }).FirstOrDefaultAsync();
                if (acceptedVendorRequest != null)
                {
                    projectTaskUsers.Add(acceptedVendorRequest);
                }
            }
            return projectTaskUsers;
        }

        public async Task SendDeadlineAsync(ITransactionScope trx, int taskId)
        {
            var projectTaskUsers = await GetAllTaksUsers(trx, taskId);

            var userEmails = await UserEmails(taskId, projectTaskUsers, NotificationType.TaskDeadline);
            if (userEmails.Count == 0)
                return;
            var taskInfo = projectTaskUsers[0];
            await _sendGridService.SendMessage(userEmails, _emailTemplates.EmailNotificationExtended, "Task - Deadline Approaching",
                new Dictionary<string, string> { { "{{NotificationText}}",
                        $"[Task Almost Due] {taskInfo.TaskName} of project {taskInfo.ProjectName}" },
                    { "{{TaskUrl}}",string.Format(_outsourceConfirmationSetting.TaskInfo, taskInfo.ProjectId, taskInfo.TaskId) } });
        }

        public async Task SendTransferRequestAsync(ITransactionScope trx, int transferRequestId)
        {
            var transferRequest = await trx.Read<TransferRequest>()
                .Include(t => t.Vendor)
                    .ThenInclude(t => t.Profile)
                .FirstOrDefaultAsync(w => w.Id == transferRequestId);

            if (transferRequest != null)
            {
                await _sendGridService.SendMessage(_transferRequestOptions.Emails.Split(',').ToList(), _emailTemplates.TransferRequest, $"{transferRequest.Vendor.Profile.FullName} transfer request",
                new Dictionary<string, string> { { "{{VendorFullName}}", transferRequest.Vendor.Profile.FullName },
                    {"{{AmountWithTax}}", transferRequest.RequestedAmountWithTax.ToString() },
                    {"{{TransferApproveUrl}}", string.Format(_transferRequestSettings.AcceptTransferRequest, transferRequestId) } });
            }
        }

        public async Task SendInProgressTaskAsync(ITransactionScope trx, int taskId)
        {
            var projectTaskUsers = await GetAllTaksUsers(trx, taskId);
            var userEmails = await UserEmails(taskId, projectTaskUsers, NotificationType.TaskInProgress);
            if (userEmails.Count == 0)
                return;
            var taskInfo = projectTaskUsers[0];
            await _sendGridService.SendMessage(userEmails, _emailTemplates.EmailNotificationExtended, "Task - Start Now",
                new Dictionary<string, string> { { "{{NotificationText}}",
                        $"[Start Task] {taskInfo.TaskName} of project {taskInfo.ProjectName}" },
                    { "{{TaskUrl}}",string.Format(_outsourceConfirmationSetting.TaskInfo, taskInfo.ProjectId, taskInfo.TaskId) } });
        }

        public async Task SendDeclineReviewTaskAsync(ITransactionScope trx, int taskId)
        {
            var tasks = await GetAllTaksUsers(trx, taskId, skipManagers: true);
            var userEmails = await UserEmails(taskId, tasks, NotificationType.TaskDeclineReview);
            if (userEmails.Count == 0)
                return;
            var taskInfo = tasks[0];
            await _sendGridService.SendMessage(userEmails, _emailTemplates.EmailNotificationExtended, "Task - Changes Needed",
                new Dictionary<string, string> { { "{{NotificationText}}", $"[Changes Needed] {taskInfo.TaskName} of project {taskInfo.ProjectName}" },
                    { "{{TaskUrl}}",string.Format(_outsourceConfirmationSetting.TaskInfo, taskInfo.ProjectId, taskInfo.TaskId) } });
        }

        public async Task SendApproveReviewTaskAsync(ITransactionScope trx, int taskId)
        {
            var userId = _principal.GetOptionalUserId();
            var name = _principal.GetFullName() ?? "Moduss";
            var taskDetail = await trx.Read<ProjectTask>()
                .Include(i => i.ProjectTaskVendors).Where(w => w.Id == taskId).Select(s => new
                {
                    s.Title,
                    UserGuid = s.ProccessingUserGuid.Value,
                    Price = s.ProjectTaskVendors.Where(w => w.Status == ProjectRequestStatus.Accepted).Select(v => v.Price).ToList()
                }).FirstOrDefaultAsync();
            var newNotification = new NotificationEntity
            {
                IsRead = false,
                Message = taskDetail.Price.Count == 0
                    ? $"<b>{taskDetail.Title}</b> has been approved and completed"
                    : $"You’ve just earned <b>${taskDetail.Price[0]}</b> because <b>{taskDetail.Title}</b> has been approved and completed",
                SenderGuid = userId,
                RecipientGuid = taskDetail.UserGuid,
                TaskId = taskId,
                CreateDate = DateTime.UtcNow,
                NotificationType = NotificationType.ProjectTaskCompleted
            };
            await trx.AddAsync(newNotification);
            await trx.SaveChangesAsync();
            await _notificationSocketStore.TaskNewNotification(taskDetail.UserGuid, new WebSockets.Stores.Models.NotificationModel
            {
                Id = newNotification.Id,
                Message = newNotification.Message,
                SenderName = name,
                IsRead = false,
                CreateDate = newNotification.CreateDate,
                NotificationType = NotificationType.ProjectTaskCompleted
            });
        }

        public async Task SendNeedReviewTaskAsync(ITransactionScope trx, int taskId)
        {
            var tasks = await GetAllTaksUsers(trx, taskId, skipWorkers: true, skipVendors: true);
            var taskInfo = tasks[0];
            await AddNotifications(tasks.Select(s => s.Id).ToList(), trx, taskId,
                string.Format(_outsourceConfirmationSetting.ProjectPage, taskInfo.ProjectId),
                $"Review <b>{taskInfo.TaskName}</b> now", NotificationType.TaskNeedReview);
            var userEmails = await UserEmails(taskId, tasks, NotificationType.TaskNeedReview);
            if (userEmails.Count == 0)
                return;
            await _sendGridService.SendMessage(userEmails, _emailTemplates.EmailNotificationExtended, "Task - Review Needed",
                new Dictionary<string, string> { { "{{NotificationText}}", $"[Review Needed] {taskInfo.TaskName} of project {taskInfo.ProjectName}" },
                    { "{{TaskUrl}}",string.Format(_outsourceConfirmationSetting.TaskInfo, taskInfo.ProjectId, taskInfo.TaskId) } });
        }

        public async Task SendOverdueTaskAsync(ITransactionScope trx, int taskId)
        {
            var projectTaskUsers = await GetAllTaksUsers(trx, taskId);

            var userEmails = await UserEmails(taskId, projectTaskUsers, NotificationType.TaskOverdue);
            if (userEmails.Count == 0)
                return;
            var taskInfo = projectTaskUsers[0];
            await _sendGridService.SendMessage(userEmails, _emailTemplates.EmailNotificationExtended, "Task - Overdue",
                new Dictionary<string, string> { { "{{NotificationText}}",
                        $"[Task Overdue] {taskInfo.TaskName} of project {taskInfo.ProjectName}" },
                    { "{{TaskUrl}}",string.Format(_outsourceConfirmationSetting.TaskInfo, taskInfo.ProjectId, taskInfo.TaskId) } });
        }

        public async Task SendSummaryAsync(Guid id)
        {
            var tempUser = await _repo.Read<User>().FirstAsync(u => u.Id == id);
            if (!await _notificationSettingsService.IsEnabledAsync(NotificationType.DailySummary, tempUser.OwnerId ?? id, id))
                return;

            var today = _dateTimeService.NowUtc;
            var yesterday = today.AddDays(-1);
            var assignedUsers = await _repo.Read<TaskHistory>()
                .Include(i => i.Task)
                    .ThenInclude(i => i.Project)
                .Include(i => i.Task)
                    .ThenInclude(i => i.ProjectTaskUsers)
                .Where(w => w.HistoryTime >= yesterday
                            && w.HistoryTime < today
                            && w.Task.Project.OwnerGuid == id
                            )
                .SelectMany(sm => sm.Task.ProjectTaskUsers
                    .Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Assigned)
                    .Select(s => new SummaryDto
                    {
                        Time = sm.HistoryTime,
                        Title = sm.Task.Title,
                        UserId = s.UserId,
                        Type = sm.Type,
                        ExecuterId = sm.UserGuid
                    }))
                .ToListAsync();

            var reviewedUsers = await _repo.Read<TaskHistory>()
                .Include(i => i.Task)
                    .ThenInclude(i => i.Project)
                .Include(i => i.Task)
                    .ThenInclude(i => i.ProjectTaskUsers)
                .Where(w => w.HistoryTime >= yesterday && w.HistoryTime < today && w.Task.Project.OwnerGuid == id)
                .SelectMany(sm => sm.Task.ProjectTaskUsers.Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing)
                    .Select(s => new SummaryDto
                    {
                        Time = sm.HistoryTime,
                        Title = sm.Task.Title,
                        UserId = s.UserId,
                        Type = sm.Type,
                        ExecuterId = sm.UserGuid
                    }))
                .ToListAsync();

            var bussinessUsers = await _repo.Read<User>()
                .Include(i => i.Profile)
                .Include(i => i.Roles)
                    .ThenInclude(i => i.Role)
                .Where(w => w.OwnerId == id || w.Id == id)
                .Select(s => new { s.Id, s.Profile.FullName, s.Email, Role = s.Roles.Select(r => r.Role.Name).First() }).ToListAsync();
            var owner = bussinessUsers.First(w => w.Id == id);

            StringBuilder tables = new StringBuilder();
            var summaries = new List<DailySummaryModel>();
            foreach (var user in bussinessUsers)
            {
                var summary = new DailySummaryModel
                {
                    FullName = user.FullName,
                    WorkerSummary = GetWorkerSummary(assignedUsers, user.Id)
                };
                summaries.Add(summary);
                if (user.Role == UserRoles.Manager || user.Role == UserRoles.Owner)
                {
                    summary.ManagerSummary = GetManageSummary(reviewedUsers, user.Id);
                }

                var userSummary = await _templateService.BuildEmailAsync("DailyEveningSummary", summary);
                var header = await _templateService.BuildEmailAsync("DailySummaryHeader", summary.FullName);
                tables.Append(header);
                tables.Append(userSummary);

                await _sendGridService.SendMessage(user.Email, _emailTemplates.DailyEveningSummary, user.FullName + " summary",
                    new Dictionary<string, string> {
                        { "{{UserName}}", summary.FullName},
                        { "{{Table}}", userSummary } });
            }

            await _sendGridService.SendMessage(owner.Email,
                await _templateService.BuildEmailAsync("DayliFullTemplate", new DailyFullTemplateModel
                {
                    FullName = owner.FullName,
                    Summary = summaries,
                    SiteUrl = _accountConfirmationSetting.SiteUrl
                }), "Company progress for today");
        }

        public async Task SendDailyToDoSummary(Guid id)
        {
            var owner = await _repo.Read<User>()
                .Include(t => t.Profile)
                .FirstOrDefaultAsync(t => t.Id == id);

            var users = await _repo.Read<User>()
                .Include(t => t.Profile)
                .Where(t => t.OwnerId == id)
                .ToListAsync();

            await SendDailyToDoSummary(owner);

            foreach (var user in users)
            {
                await SendDailyToDoSummary(user);
            }
        }

        private async Task SendDailyToDoSummary(User user)
        {
            var todayTasks = await GetTasksForToday(user);

            if (!todayTasks.Any())
            {
                return;
            }

            await _sendGridService.SendMessage(user.Email, _emailTemplates.DailyMorningSummary, "Your tasks for today",
                    new Dictionary<string, string> {
                        { "{{UserName}}", user.Profile.FullName },
                        { "{{TaskList}}", string.Concat(todayTasks.Select(t => $"<li>{t.Title}</li>")) }
                    });
        }

        private async Task<List<ProjectTask>> GetTasksForToday(User user)
        {
            var projectIds = await GetOwnProjectsIds(user);
            var accessQuery = _repo
                .Read<Project>()
                .Where(f =>
                projectIds.Contains(f.Id));

            var tasksQuery = _repo.Read<ProjectTask>()
                .Include(c => c.ChildTasks)
                .Include(c => c.ParentTasks)
                .Include(c => c.RecurrenceOptions)
                .Include(c => c.Condition)
                    .ThenInclude(c => c.Options)
                .Include(c => c.AssignedConditionOptions)
                    .ThenInclude(co => co.Condition)
                    .ThenInclude(c => c.Task)
                .Include(c => c.Condition)
                    .ThenInclude(t => t.Options)
                .Where(c => projectIds.Contains(c.ProjectId) && c.Status != TaskStatusType.Completed && c.FormulaId == null);

            if (await _userManager.IsInRoleAsync(user, UserRoles.Worker))
            {
                tasksQuery = tasksQuery
                    .Join(_repo.Read<TeamUser>()
                        .Include(i => i.Team).ThenInclude(i => i.AssignedTasks).Where(w => w.UserId == user.Id)
                        .SelectMany(sm => sm.Team.AssignedTasks.Select(s => s.Id)),
                    inner => inner.Id, outer => outer,
                    (task, userProject) => task);
            }

            tasksQuery = tasksQuery.Where(t => t.Status != TaskStatusType.New
                                               || t.StartDate < _dateTimeService.TodayUtc.AddDays(1));

            return await tasksQuery.ToListAsync();
        }

        private async Task<IEnumerable<int>> GetOwnProjectsIds(User user)
        {
            IQueryable<Project> query = _repo.Read<Project>();

            if (await _userManager.IsInRoleAsync(user, UserRoles.Owner))
            {
                query = query.Where(x => x.OwnerGuid == user.Id);
            }
            else if (await _userManager.IsInRoleAsync(user, UserRoles.Manager))
            {
                //TODO need to refactor this for now EF Core doesnt support IQueryable Union #246
                var projectIds = _repo.Read<UserProject>()
                .Where(w => w.UserId == user.Id).Select(s => s.ProjectId)
                .Union(_repo.Read<TeamUser>()
                    .Include(i => i.Team)
                    .ThenInclude(i => i.AssignedTasks)
                    .Where(w => w.UserId == user.Id)
                    .SelectMany(sm => sm.Team.AssignedTasks.Select(s => s.ProjectId))
                    .GroupBy(g => g).Select(s => s.Key))
                .Union(_repo.Read<TeamUser>()
                    .Include(i => i.Team)
                    .ThenInclude(i => i.ReviewingTasks)
                    .Where(w => w.UserId == user.Id)
                    .SelectMany(sm => sm.Team.ReviewingTasks.Select(s => s.ProjectId))
                    .GroupBy(g => g).Select(s => s.Key)
                ).ToList();
                query = query.Where(w => projectIds.Contains(w.Id));
            }
            else
            {
                query = query.Join(_repo.Read<TeamUser>()
                        .Include(i => i.Team)
                        .ThenInclude(i => i.AssignedTasks)
                        .Where(w => w.UserId == user.Id)
                        .SelectMany(sm => sm.Team.AssignedTasks.Select(s => s.ProjectId))
                        .GroupBy(g => g),
                    outer => outer.Id,
                    inner => inner.Key,
                    (project, userProject) => project
                );
            }

            return await query.Select(t => t.Id).ToListAsync();
        }



        private WorkerSummary GetWorkerSummary(List<SummaryDto> summaries, Guid userId)
        {
            var result = new WorkerSummary();
            foreach (var entity in summaries)
            {
                if (entity.UserId == userId)
                {
                    var task = new TaskSummaryDetail { Title = entity.Title, Time = entity.Time };
                    switch (entity.Type)
                    {
                        case ActivityType.New:
                            result.Upcommings.Add(task);
                            break;
                        case ActivityType.InProgress:
                            result.InProgressings.Add(task);
                            break;
                        case ActivityType.Processing:
                            if (entity.ExecuterId == userId)
                                result.Proccessings.Add(task);
                            break;
                        case ActivityType.NeedsReview:
                            result.NeedReviews.Add(task);
                            break;
                        case ActivityType.DeclineReview:
                            result.DeclineReviews.Add(task);
                            break;
                        case ActivityType.AcceptReview:
                        case ActivityType.Completed:
                            result.Completed.Add(task);
                            break;
                        case ActivityType.Overdue:
                            result.Overdues.Add(task);
                            break;
                    }
                }
            }
            return result;
        }

        private ManagerSummary GetManageSummary(List<SummaryDto> summaries, Guid userId)
        {
            var result = new ManagerSummary();
            foreach (var entity in summaries)
            {
                if (entity.UserId == userId)
                {
                    var task = new TaskSummaryDetail { Title = entity.Title, Time = entity.Time };
                    switch (entity.Type)
                    {
                        case ActivityType.New:
                            result.ManagerUpcommings.Add(task);
                            break;
                        case ActivityType.InProgress:
                            result.ManagerInProccessings.Add(task);
                            break;
                        case ActivityType.Reviewing:
                            if (entity.ExecuterId == userId)
                                result.ManagerReviewings.Add(task);
                            break;
                        case ActivityType.NeedsReview:
                            result.ManagerNeedReviews.Add(task);
                            break;
                        case ActivityType.DeclineReview:
                            result.ManagerDeclineReviews.Add(task);
                            break;
                        case ActivityType.AcceptReview:
                            result.ManagerAcceptReviews.Add(task);
                            break;
                        case ActivityType.Overdue:
                            result.ManagerOverdues.Add(task);
                            break;
                    }
                }
            }
            return result;
        }

        private async Task<List<string>> UserEmails(int taskId, IEnumerable<UserItemDto> users, NotificationType type)
        {
            var userEmails = new List<string>();
            foreach (var user in users)
            {
                if (await _notificationSettingsService.IsEnabledAsync(type, user.OwnerId ?? user.Id, user.Id, taskId))
                {
                    userEmails.Add(user.Email);
                }
            }
            return userEmails;
        }

        public async Task SendTaskCommentAsync(int taskId)
        {
            var users = new List<UserItemDto>();
            using (var trx = _repo.Transaction())
            {
                var comentatorId = _principal.GetUserId();
                users.AddRange(await GetAllTaksUsers(trx, taskId));
                var owner = await trx.Read<ProjectTask>()
                 .Include(i => i.Owner)
                 .Where(w => w.Id == taskId)
                 .Select(s => new UserItemDto
                 {
                     Id = s.OwnerGuid,
                     OwnerId = s.Owner.OwnerId,
                     Email = s.Owner.Email,
                     TaskId = s.Id,
                     TaskName = s.Title,
                     ProjectId = s.ProjectId,
                     ProjectName = s.Project.Name
                 })
                 .FirstOrDefaultAsync();
                users.Add(owner);
                users = users.Where(w => w.Id != comentatorId).ToList();
                await AddNotifications(users.GroupBy(g => g.Id).Select(s => s.Key).ToList(), trx, taskId,
                    string.Format(_outsourceConfirmationSetting.TaskComment, users[0].ProjectId, users[0].TaskId),
                    $"A comment has been added to <b>{users[0].TaskName}</b>", NotificationType.TaskCommented);
                await trx.SaveAndCommitAsync();
            }

            var comment = await _repo.Read<ProjectTaskComment>()
                .Include(t => t.User)
                .ThenInclude(t => t.Profile)
                .Where(w => w.ProjectTaskId == taskId)
                .OrderByDescending(o => o.DateCreated)
                .Select(s => new { s.User.Profile.FullName, s.Text }).FirstOrDefaultAsync();

            var userComparer = new UserItemDtoComparer();
            var userEmails = await UserEmails(taskId, users, NotificationType.TaskCommented);

            if (userEmails.Count == 0)
                return;

            var taskInfo = users[0];
            await _sendGridService.SendMessage(userEmails, _emailTemplates.EmailNotificationExtended, "Task - Commenting",
                new Dictionary<string, string> { { "{{NotificationText}}", $"{comment.FullName} commented in task {taskInfo.TaskName} \"{comment.Text}\" " },
                    { "{{TaskUrl}}", string.Format(_outsourceConfirmationSetting.TaskComment, taskInfo.ProjectId, taskInfo.TaskId) } });
        }
        public async Task SendProjectTaskOutsourcesAcceptAsync(ITransactionScope trx, int taskId)
        {
            var users = new List<UserItemDto>();
            var vendorName = _principal.GetFullName();
            users.AddRange(await GetAllTaksUsers(trx, taskId, skipVendors: true, skipWorkers: true));
            var owner = await trx.Read<ProjectTask>()
             .Include(i => i.Owner)
             .Where(w => w.Id == taskId)
             .Select(s => new UserItemDto
             {
                 Id = s.OwnerGuid,
                 OwnerId = s.Owner.OwnerId,
                 Email = s.Owner.Email,
                 TaskId = s.Id,
                 TaskName = s.Title,
                 ProjectId = s.ProjectId,
                 ProjectName = s.Project.Name
             })
             .FirstOrDefaultAsync();
            users.Add(owner);
            await AddNotifications(users.GroupBy(g => g.Id).Select(s => s.Key).ToList(), trx, taskId,
                string.Format(_outsourceConfirmationSetting.TaskInfo, users[0].ProjectId, users[0].TaskId),
                $"<b>{owner.TaskName}</b> has been accepted by {vendorName}", NotificationType.ProjectTaskAccepted);
        }

        public async Task SendFormulaTaskOutsourcesAsync(ITransactionScope trx, int requestId)
        {
            var data = await trx.Read<FormulaTaskVendor>()
                    .Include(i => i.Vendor)
                    .Include(i => i.FormulaTask)
                    .Where(w => w.Id == requestId)
                    .Select(s => new { s.Vendor.Email, s.Vendor.Id, s.FormulaTask.Title })
                    .FirstOrDefaultAsync();

            await AddNotifications(new List<Guid> { data.Id }, trx, null,
                string.Format(_outsourceConfirmationSetting.Formula, requestId),
                $"You have been asked to bid on <b>{data.Title}</b> now", NotificationType.FormulaTaskOutsource);

            await _sendGridService.SendMessage(data.Email, _emailTemplates.VendorInvitationToBid, $"Invitation To Bid - {data.Title}",
                new Dictionary<string, string> { { "{{TaskName}}", data.Title },
                    {"{{NotificationUrl}}", string.Format(_outsourceConfirmationSetting.Formula, requestId) } });
        }

        public async Task SendProjectTaskOutsourcesAsync(ITransactionScope trx, int requestId)
        {
            var data = await trx.Read<ProjectTaskVendor>()
                    .Include(i => i.Vendor)
                    .Where(w => w.Id == requestId)
                    .Select(s => new { s.Vendor.Email, s.Vendor.Id, s.ProjectTask.Title })
                    .FirstOrDefaultAsync();

            await AddNotifications(new List<Guid> { data.Id }, trx, null,
                string.Format(_outsourceConfirmationSetting.Project, requestId),
                $"<b>{data.Title}</b> has been sent to you for acceptance", NotificationType.ProjectTaskOutsource);

            await _sendGridService.SendMessage(data.Email, _emailTemplates.VendorInvitationToWork, $"Click to win the work! {data.Title}",
                new Dictionary<string, string> { { "{{TaskName}}", data.Title },
                    {"{{NotificationUrl}}", string.Format(_outsourceConfirmationSetting.Project, requestId) } });
        }

        public async Task SendProjectTaskOutsourcesUnavailableAsync(ITransactionScope trx, int requestId)
        {
            var data = await trx.Read<ProjectTaskVendor>()
                    .Include(i => i.Vendor)
                    .Where(w => w.Id == requestId)
                    .Select(s => new
                    {
                        s.Vendor.Email,
                        s.Vendor.Id,
                        s.ProjectTaskId,
                        s.ProjectTask.Title,
                        s.ProjectTask.OwnerGuid,
                        s.ProjectTask.Owner.Profile.FullName
                    })
                    .FirstOrDefaultAsync();

            //var name = "Modus8";
            var relativeUrl = string.Format(_outsourceConfirmationSetting.Project, requestId)
                .Replace(_accountConfirmationSetting.SiteUrl, "/");

            if (data != null)
            {
                var newNotification = new NotificationEntity
                {
                    IsRead = false,
                    Message = $"<b>{data.Title}</b> is no longer available",
                    SenderGuid = data.OwnerGuid,
                    RecipientGuid = data.Id,
                    TaskId = data.ProjectTaskId,
                    CreateDate = DateTime.UtcNow,
                    Url = relativeUrl,
                    NotificationType = NotificationType.ProjectTaskUnavailable
                };
                await trx.AddAsync(newNotification);
                await trx.SaveChangesAsync();

                await _notificationSocketStore.TaskNewNotification(data.Id, new WebSockets.Stores.Models.NotificationModel
                {
                    Id = newNotification.Id,
                    Message = newNotification.Message,
                    Url = newNotification.Url,
                    SenderName = data.FullName,
                    IsRead = false,
                    CreateDate = newNotification.CreateDate,
                    NotificationType = NotificationType.ProjectTaskUnavailable
                });
            }

                //await AddNotifications(new List<Guid> { data.Id }, trx, null,
                //string.Format(_outsourceConfirmationSetting.Project, requestId),
                //$"<b>{data.Title}</b> is no longer available", NotificationType.ProjectTaskUnavailable);
        }

        public async Task SendAcceptOutsourcePaymentAsync(ITransactionScope trx, CreditLog creditLog)
        {
            await AddNotifications(new List<Guid> { creditLog.VendorId.Value }, trx, null,
                null,
                $"Your payment of ${creditLog.Amount} was just sent to you ", NotificationType.AcceptOutsourceRequest);
        }

        public async Task SendStartProjectTaskOutsourcesAsync(ITransactionScope trx, int taskId)
        {
            var data = await trx.Read<ProjectTask>()
                    .Include(i => i.ProccessingUser)
                    .ThenInclude(i => i.Profile)
                    .Include(i => i.ProjectTaskUsers)
                    .Where(w => w.Id == taskId)
                    .Select(s => new
                    {
                        s.ProccessingUser.Email,
                        s.ProccessingUser.Profile.FullName,
                        s.Title,
                        s.ProjectId,
                        s.Project.Name,
                        s.ProjectTaskUsers,
                        s.OwnerGuid
                    })
                    .FirstOrDefaultAsync();
            var managers = data.ProjectTaskUsers
                       .Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing).Select(s => s.UserId).ToList();
            if (!managers.Contains(data.OwnerGuid))
                managers.Add(data.OwnerGuid);
            await AddNotifications(managers, trx,
                    taskId, string.Format(_outsourceConfirmationSetting.TaskInfo, data.ProjectId, taskId),
                $"<b>{ data.Title}</b> has been started by <b>{data.FullName}</b>", NotificationType.ProjectTaskStarted);
            ;
            await _sendGridService.SendMessage(data.Email, _emailTemplates.VendorStartToWork, $" [Start Now] {data.Title}/{data.Name}",
                new Dictionary<string, string> { { "{{TaskName}}", data.Title }, { "{{ProjectName}}", data.Name },
                    {"{{StartWorkUrl}}", string.Format(_outsourceConfirmationSetting.StartWork, data.ProjectId, taskId) } });
        }

        public async Task SendStartProjectTaskAsync(ITransactionScope trx, int taskId)
        {
            var data = await trx.Read<ProjectTask>()
                    .Include(i => i.ProccessingUser)
                    .ThenInclude(i => i.Profile)
                    .Include(i => i.ProjectTaskUsers)
                    .Where(w => w.Id == taskId)
                    .Select(s => new
                    {
                        s.ProjectId,
                        s.Title,
                        s.OwnerGuid,
                        s.ProjectTaskUsers
                    })
                    .FirstOrDefaultAsync();
            var userId = _principal.GetUserId();
            var managers = data.ProjectTaskUsers
                       .Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing && t.UserId != userId).Select(s => s.UserId).ToList();
            if (!managers.Contains(data.OwnerGuid) && data.OwnerGuid != userId)
                managers.Add(data.OwnerGuid);
            await AddNotifications(managers, trx,
                    taskId, string.Format(_outsourceConfirmationSetting.TaskInfo, data.ProjectId, taskId),
                $"<b>{data.Title}</b> has been started by <b>{_principal.GetFullName()}</b>", NotificationType.ProjectTaskStarted);
        }

        public async Task SendInviteOutsourceAsync(ProjectTask task)
        {
            using (var trx = _repo.Transaction())
            {
                var managers = task.ProjectTaskUsers
                           .Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing).Select(s => s.User.Id).ToList();
                if (!managers.Contains(task.OwnerGuid))
                    managers.Add(task.OwnerGuid);
                await AddNotifications(managers, trx,
                        task.Id, string.Format(_outsourceConfirmationSetting.TaskInfo, task.ProjectId, task.Id),
                    $"Assign <b>{task.Title}</b> now", NotificationType.ProjectTaskNeedVendor);
                await trx.SaveAndCommitAsync();
            }
            await _sendGridService.SendMessage(task.ProjectTaskUsers
                    .Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing).Select(s => s.User.Email).ToList(),
                _emailTemplates.TimeToInviteVendor, $"It's Time To Outsource! {task.Title}/{task.Project.Name}",
                new Dictionary<string, string> { { "{{TaskName}}", task.Title }, { "{{ProjectName}}", task.Project.Name },
                    {"{{SendRequestUrl}}", string.Format(_outsourceConfirmationSetting.AssignWorker, task.ProjectId, task.Id) } });
        }

        public async Task SendStopVendorTaskOnCancelNudgeAsync(ITransactionScope trx, int requestId)
        {
            var data = await trx.Read<ProjectTaskVendor>()
                    .Include(i => i.Vendor)
                        .ThenInclude(t => t.Profile)
                    .Include(i => i.ProjectTask)
                    .Where(w => w.Id == requestId)
                    .Select(s => new
                    {
                        VendorGuid = s.Vendor.Id,
                        VendorName = s.Vendor.Profile.FullName,
                        Title = s.ProjectTask.Title,
                        TaskOwnerGuid = s.ProjectTask.OwnerGuid,
                        TaskId = s.ProjectTask.Id
                    })
                    .FirstOrDefaultAsync();

            await AddNotifications(new List<Guid> { data.TaskOwnerGuid }, trx, data.TaskId, String.Empty,
                $"<b>{data.VendorName}</b> has declined the job <b>{data.Title}</b> after 3 nudges", NotificationType.CancelNudgeNotificationToOwner);

            // Notification to self
            var newNotification = new NotificationEntity
            {
                IsRead = false,
                Message = $"<b>{data.Title}</b> has been successfully declined.",
                SenderGuid = null,
                RecipientGuid = data.VendorGuid,
                TaskId = data.TaskId,
                CreateDate = DateTime.UtcNow,
                Url = String.Empty,
                NotificationType = NotificationType.CancelNudgeNotificationToVendor
            };
            await trx.AddAsync(newNotification);
            await trx.SaveChangesAsync();
        }

        public async Task SendExpiredJobInvitesNotificationToOwner(ITransactionScope trx, int requestId)
        {
            var data = await trx.Read<ProjectTaskVendor>()
                    .Include(i => i.Vendor)
                        .ThenInclude(t => t.Profile)
                    .Include(i => i.ProjectTask)
                    .Where(w => w.Id == requestId)
                    .Select(s => new
                    {
                        VendorGuid = s.Vendor.Id,
                        VendorName = s.Vendor.Profile.FullName,
                        Title = s.ProjectTask.Title,
                        TaskOwnerGuid = s.ProjectTask.OwnerGuid,
                        TaskId = s.ProjectTask.Id,
                        ProjectId = s.ProjectTask.ProjectId
                    })
                    .FirstOrDefaultAsync();

            var url = string.Format(_outsourceConfirmationSetting.TaskInfo, data.ProjectId, data.TaskId);
            var relativeUrl = url.Replace(_accountConfirmationSetting.SiteUrl, "/");

            // Notification to self
            var newNotification = new NotificationEntity
            {
                IsRead = false,
                Message = $"No vendor accepted the job <b>{data.Title}</b>, invite has now <i>expired</i>.",
                SenderGuid = null,
                RecipientGuid = data.TaskOwnerGuid,
                TaskId = data.TaskId,
                CreateDate = DateTime.UtcNow,
                Url = relativeUrl,
                NotificationType = NotificationType.JobLostNotificationToOwner
            };

            await trx.AddAsync(newNotification);
            await trx.SaveChangesAsync();
            await _notificationSocketStore.TaskNewNotification(data.TaskOwnerGuid, new WebSockets.Stores.Models.NotificationModel
            {
                Id = newNotification.Id,
                Message = newNotification.Message,
                Url = newNotification.Url,
                SenderName = "Modus8",
                IsRead = false,
                CreateDate = newNotification.CreateDate,
                NotificationType = NotificationType.JobLostNotificationToOwner
            });
        }

        public async Task SendExpiredJobInvitesNotificationToVendor(ITransactionScope trx, int requestId)
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-us");

            var data = await trx.Read<ProjectTaskVendor>()
                    .Include(i => i.Vendor)
                        .ThenInclude(t => t.Profile)
                    .Include(i => i.ProjectTask)
                    .Where(w => w.Id == requestId)
                    .Select(s => new
                    {
                        VendorGuid = s.Vendor.Id,
                        VendorName = s.Vendor.Profile.FullName,
                        Title = s.ProjectTask.Title,
                        TaskOwnerGuid = s.ProjectTask.OwnerGuid,
                        TaskId = s.ProjectTask.Id,
                        Price = s.Price
                    })
                    .FirstOrDefaultAsync();

            // Notification to self
            var newNotification = new NotificationEntity
            {
                IsRead = false,
                Message = $"You forgot to respond the invite for <b>{data.Title}</b>. The invite has now expired after 24 hrs and you lost <b>{data.Price.ToString("C", ci)}</b>.",
                SenderGuid = data.TaskOwnerGuid,
                RecipientGuid = data.VendorGuid,
                TaskId = data.TaskId,
                CreateDate = DateTime.UtcNow,
                Url = String.Empty,
                NotificationType = NotificationType.JobLostNotificationToVendor
            };

            await trx.AddAsync(newNotification);
            await trx.SaveChangesAsync();
            await _notificationSocketStore.TaskNewNotification(data.VendorGuid, new WebSockets.Stores.Models.NotificationModel
            {
                Id = newNotification.Id,
                Message = newNotification.Message,
                SenderName = "Modus8",
                IsRead = false,
                CreateDate = newNotification.CreateDate,
                NotificationType = NotificationType.JobLostNotificationToVendor
            });
        }

        public async Task SendPublishFormulaNotification(PublishFormulaList item)
        {
            var users = await _repo.Track<User>()
                    .Include(t => t.Roles)
                        .ThenInclude(t => t.Role).Select(x => new UserProfileWithRoleDto
                        {
                            UserId = x.Id,
                            Email = x.Email,
                            FullName = x.Profile.FullName != null ? x.Profile.FullName : string.Empty,
                            Role = x.Roles.Select(r => r.Role.Name).FirstOrDefault() != null ? x.Roles.Select(r => r.Role.Name).FirstOrDefault() : string.Empty,
                        }).Where(x => x.Role.ToLower().Contains("Owner")).ToListAsync();

            foreach (var user in users)
            {
                Dictionary<string, string> obj = new Dictionary<string, string>{
                        { "{{Formula}}", item.Formula },
                        { "{{SkillList}}",item.SkillList },
                        { "{{TeamList}}",item.TeamList },
                        { "{{UserName}}", user.FullName }
                    };
                _sendGridService.SendMessage(user.Email, _emailTemplates.PublishNotification, "New update from Modus",
               obj);
            }
        }

        public async Task SendPaymentRequestAcceptedAsync(ITransactionScope trx, int requestId)
        {
            var adminUserId = _principal.GetOptionalUserId();

            var data = await trx.Read<TransferRequest>()
                .Include(t => t.Vendor)
                    .ThenInclude(t => t.Profile)
                .Where(w => w.Id == requestId)
                .Select(s => new
                {
                    Amount = s.RequestedAmountWithTax,
                    VendorGuid = s.Vendor.Id,
                    VendorName = s.Vendor.Profile.FullName,
                    VendorEmail = s.Vendor.Email
                })
                .FirstOrDefaultAsync();

            var newNotification = new NotificationEntity
            {
                IsRead = false,
                Message = $"Your payment of {data.Amount.ToString("C", CultureInfo.CreateSpecificCulture("en-US"))} is Processed.",
                SenderGuid = adminUserId,
                RecipientGuid = data.VendorGuid,
                TaskId = null,
                CreateDate = DateTime.UtcNow,
                Url = String.Empty,
                NotificationType = NotificationType.AcceptFundTransferRequest
            };
            await trx.AddAsync(newNotification);
            await trx.SaveChangesAsync();

            await _notificationSocketStore.TaskNewNotification(data.VendorGuid, new WebSockets.Stores.Models.NotificationModel
            {
                Id = newNotification.Id,
                Message = newNotification.Message,
                SenderName = "Modus8",
                IsRead = false,
                CreateDate = newNotification.CreateDate,
                NotificationType = NotificationType.AcceptFundTransferRequest
            });
             _sendGridService.SendMessage(data.VendorEmail,
                $"Your payment of {data.Amount.ToString("C", CultureInfo.CreateSpecificCulture("en-US"))} is Processed.", "Fund Request Processed");
        }

        public async Task SendPaymentRequestDeclinedAsync(ITransactionScope trx, int requestId)
        {
            var adminUserId = _principal.GetOptionalUserId();

            var data = await trx.Read<TransferRequest>()
                .Include(t => t.Vendor)
                    .ThenInclude(t => t.Profile)
                .Where(w => w.Id == requestId)
                .Select(s => new
                {
                    Amount = s.RequestedAmountWithTax,
                    VendorGuid = s.Vendor.Id,
                    VendorName = s.Vendor.Profile.FullName,
                    VendorEmail = s.Vendor.Email
                })
                .FirstOrDefaultAsync();

            var newNotification = new NotificationEntity
            {
                IsRead = false,
                Message = $"Your transfer request for an amount of <b>{data.Amount.ToString("C", CultureInfo.CreateSpecificCulture("en-US"))}</b> has been rejected. Please contact support.",
                SenderGuid = adminUserId,
                RecipientGuid = data.VendorGuid,
                TaskId = null,
                CreateDate = DateTime.UtcNow,
                Url = String.Empty,
                NotificationType = NotificationType.DeclineFundTransferRequest
            };
            await trx.AddAsync(newNotification);
            await trx.SaveChangesAsync();

            await _notificationSocketStore.TaskNewNotification(data.VendorGuid, new WebSockets.Stores.Models.NotificationModel
            {
                Id = newNotification.Id,
                Message = newNotification.Message,
                SenderName = "Modus8",
                IsRead = false,
                CreateDate = newNotification.CreateDate,
                NotificationType = NotificationType.DeclineFundTransferRequest
            });

            _sendGridService.SendMessage(
                data.VendorEmail,
                $"Your transfer request for an amount of <b>{data.Amount.ToString("C", CultureInfo.CreateSpecificCulture("en-US"))}</b> has been rejected. Please contact support.", "Fund Request Declined");
        }
    }
}
