using AutoMapper;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Global.Enums;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks
{
    public class TaskHistoryService : ITaskHistoryService
    {
        private readonly ClaimsPrincipal _principal;
        private readonly IRepo _repo;
        private readonly IDateTimeService _dateTimeService;

        public TaskHistoryService(
            ClaimsPrincipal principal,
            IRepo repo,
            IDateTimeService dateTimeService)
        {
            _principal = principal;
            _repo = repo;
            _dateTimeService = dateTimeService;
        }

        public async Task<List<TaskHistoryItemDto>> GetTaskHistoryByProjectId(int projectId, int count = 50)
        {
            using (var trx = _repo.Transaction())
            {
                var ownerGuid = _principal.GetOwnerId();

                var history = await trx.Read<TaskHistory>()
                    .Include(x => x.Task)
                    .ThenInclude(x => x.Project)
                    .Include(x => x.ProjectTaskConditionOption)
                    .ThenInclude(x => x.Condition)
                    .Where(t => t.Task.ProjectId == projectId && t.Task.Project.OwnerGuid == ownerGuid)
                    .OrderByDescending(t => t.HistoryTime)
                    .Take(count)
                    .ToListAsync();

                return Mapper.Map<List<TaskHistoryItemDto>>(history);
            }
        }

        public async Task<List<TaskHistoryItemDto>> GetTasksHistory(IEnumerable<int> projectIds, int count = 50)
        {
            using (var trx = _repo.Transaction())
            {
                var ownerGuid = _principal.GetOwnerId();

                var history = await trx.Read<TaskHistory>()
                    .Include(x => x.Task)
                    .ThenInclude(x => x.Project)
                    .Include(x => x.ProjectTaskConditionOption)
                    .ThenInclude(x => x.Condition)
                    .Where(t => projectIds.Contains(t.Task.ProjectId) && t.Task.Project.OwnerGuid == ownerGuid)
                    .OrderByDescending(t => t.HistoryTime)
                    .Take(count)
                    .ToListAsync();

                return Mapper.Map<List<TaskHistoryItemDto>>(history);
            }
        }

        public async Task Write(
         int taskId,
         ActivityType activityType,
         int? selectedConditionOptionId = null,
         ITransactionScope trx = null,
         bool saveChanges = false)
        {
            var historyEntry = new TaskHistory
            {
                TaskId = taskId,
                Type = activityType,
                ProjectTaskConditionOptionId = selectedConditionOptionId,
                HistoryTime = _dateTimeService.NowUtc,
                UserGuid = _principal.GetUserId()
            };

            if (trx != null)
            {
                await trx.AddAsync(historyEntry);
                if (saveChanges) await trx.SaveAndCommitAsync();
            }
            else
            {
                await _repo.AddAsync(historyEntry);
                if (saveChanges) await _repo.SaveChangesAsync();
            }
        }
    }
}
