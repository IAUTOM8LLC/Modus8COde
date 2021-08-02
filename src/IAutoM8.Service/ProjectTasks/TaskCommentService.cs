using AutoMapper;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Exceptions;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks
{
    public class TaskCommentService : ITaskCommentService
    {
        private readonly ClaimsPrincipal _principal;
        private readonly IRepo _repo;
        private readonly IScheduleService _scheduleService;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationService _notificationService;

        public TaskCommentService(
            IRepo repo,
            ClaimsPrincipal principal,
            IScheduleService scheduleService,
            IDateTimeService dateTimeService,
            INotificationService notificationService)
        {
            _repo = repo;
            _principal = principal;
            _scheduleService = scheduleService;
            _dateTimeService = dateTimeService;
            _notificationService = notificationService;
        }

        public async Task<List<CommentDto>> GetCommentsAsync(int taskId)
        {
            var comments = await _repo.Read<ProjectTaskComment>()
                .Include(i => i.User)
                .ThenInclude(i => i.Profile)
                .Where(w => w.ProjectTaskId == taskId)
                .ToListAsync();

            return Mapper.Map<List<CommentDto>>(comments);
        }

        public async Task<CommentDto> AddCommentAsync(AddCommentDto comment)
        {
            var entity = Mapper.Map<ProjectTaskComment>(comment);
            using (var transaction = _repo.Transaction())
            {
                entity.DateCreated = _dateTimeService.NowUtc;
                entity.UserGuid = _principal.GetUserId();
                if (_principal.IsVendor())
                {
                    var messageStats = await transaction.Track<FormulaTaskStatistic>()
                           .Where(w => w.ProjectTaskId == comment.TaskId && w.Type == StatisticType.Messaging && !w.Value.HasValue)
                           .ToListAsync();
                    foreach(var stat in messageStats)
                    {
                        stat.Completed = entity.DateCreated;
                        stat.Value = (short)(entity.DateCreated - stat.Created).TotalMinutes;
                    }
                }
                else
                {
                    var taskVendor = await transaction.Read<ProjectTaskVendor>()
                        .Include(i => i.ProjectTask)
                        .FirstOrDefaultAsync(w => w.ProjectTaskId == comment.TaskId && w.Status == ProjectRequestStatus.Accepted);
                    if (taskVendor != null)
                    {
                        await transaction.AddAsync(new FormulaTaskStatistic
                        {
                            Created = entity.DateCreated,
                            FormulaTaskId = taskVendor.ProjectTask.FormulaTaskId.Value,
                            ProjectTaskId = taskVendor.ProjectTaskId,
                            Type = StatisticType.Messaging,
                            VendorGuid = taskVendor.VendorGuid
                        });

                        // Reset the timer
                        await _scheduleService.RemoveVendorTaskReviewJob(transaction, taskVendor.ProjectTaskId);
                        if (taskVendor.ProjectTask.Status == TaskStatusType.NeedsReview)
                        {
                            await _scheduleService.CreateVendorTaskReviewJob(transaction, taskVendor.ProjectTaskId);
                        }
                    }
                }

                await transaction.AddAsync(entity);
                await transaction.SaveChangesAsync();
                transaction.Commit();
            }

            await _notificationService.SendTaskCommentAsync(entity.ProjectTaskId);

            var model = Mapper.Map<CommentDto>(entity);
            model.Author = _principal.GetFullName();
            return model;
        }

        public async Task<int> DeleteCommenAsynct(int commentId)
        {
            var entity = await _repo.FindAsync<ProjectTaskComment>(commentId);

            if (entity.UserGuid != _principal.GetUserId())
                throw new ForbiddenException("You have no access to delete a comment.");

            _repo.Remove(entity);

            await _repo.SaveChangesAsync();
            return commentId;
        }

    }
}
