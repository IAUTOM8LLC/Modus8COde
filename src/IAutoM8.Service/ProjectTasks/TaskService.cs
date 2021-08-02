using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAutoM8.Domain.Models;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.User;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Exceptions;
using IAutoM8.Global.Extensions;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Resources.Dto;
using IAutoM8.Service.Scheduler.Interfaces;
using IAutoM8.WebSockets.Stores.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using NotificationEntity = IAutoM8.Domain.Models.Notification;
using IAutoM8.Neo4jRepository.Dto;
using System.Data.SqlClient;
using System.Data;
using Braintree.Exceptions;
using IAutoM8.Service.Users.Dto;
using IAutoM8.Service.Infrastructure;

namespace IAutoM8.Service.ProjectTasks
{
    public class TaskService : ITaskService
    {
        private readonly ClaimsPrincipal _principal;
        private readonly UserManager<User> _userManager;
        private readonly IRepo _repo;
        private readonly IScheduleService _scheduleService;
        private readonly ITaskScheduleService _taskScheduleService;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationService _notificationService;
        private readonly IStorageService _storageService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly ITaskHistoryService _taskHistoryService;
        private readonly ITaskStartDateHelperService _startDateHelperService;
        private readonly ITaskSocketStore _taskSocketStore;
        private readonly IMapper _mapper;
        private readonly IFormulaTaskJobService _formulaTaskJobService;
        private readonly IEntityFrameworkPlus _entityFrameworkPlus;
        private readonly INotificationSocketStore _notificationSocketStore;

        public TaskService(IRepo repo,
            IScheduleService scheduleService,
            ITaskScheduleService taskScheduleService,
            ClaimsPrincipal principal,
            UserManager<User> userManager,
            IDateTimeService dateTimeService,
            INotificationService notificationService,
            IStorageService storageService,
            ITaskNeo4jRepository taskNeo4JRepository,
            ITaskHistoryService taskHistoryService,
            ITaskStartDateHelperService startDateHelperService,
            ITaskSocketStore taskSocketStore,
            IMapper mapper,
            IFormulaTaskJobService formulaTaskJobService,
            IEntityFrameworkPlus entityFrameworkPlus,
            INotificationSocketStore notificationSocketStore)
        {
            _repo = repo;
            _userManager = userManager;
            _scheduleService = scheduleService;
            _taskScheduleService = taskScheduleService;
            _principal = principal;
            _dateTimeService = dateTimeService;
            _notificationService = notificationService;
            _storageService = storageService;
            _taskNeo4JRepository = taskNeo4JRepository;
            _taskHistoryService = taskHistoryService;
            _startDateHelperService = startDateHelperService;
            _taskSocketStore = taskSocketStore;
            _mapper = mapper;
            _formulaTaskJobService = formulaTaskJobService;
            _entityFrameworkPlus = entityFrameworkPlus;
            _notificationSocketStore = notificationSocketStore;
        }

        #region Task Service

        public async Task<List<TaskDto>> GetTasksAsync(IEnumerable<int> projectIds, bool selectTasksForToday = false)
        {
            if (!projectIds.Any())
            {
                return new List<TaskDto>();
            }
            var userId = _principal.GetUserId();

            var user = await _repo.Read<User>()
                .Include(t => t.UserSkills)
                .SingleOrDefaultAsync(t => t.Id == userId);

            await CheckAccess(userId);

            var tasksQuery = _repo.Read<ProjectTask>()
                .Include(c => c.RecurrenceOptions)
                .Include(c => c.ProjectTaskUsers)
                .Include(c => c.ChildTasks)
                .Include(c => c.ParentTasks)
                .Include(c => c.AssignedSkill)
                .Include(c => c.AssignedConditionOptions)
                    .ThenInclude(co => co.Condition)
                    .ThenInclude(c => c.Task)
                .Include(c => c.Condition)
                    .ThenInclude(t => t.Options)
                .Include(c => c.ProjectTaskVendors)
                    .ThenInclude(t => t.Vendor)
                    .ThenInclude(t => t.Profile)
                .Include(t => t.ProccessingUser)
                    .ThenInclude(t => t.Profile)
                .Include(t => t.ReviewingUser)
                    .ThenInclude(t => t.Profile)
                .Include(t => t.TaskHistories)
                .Include(t => t.FormulaTask)
                    .ThenInclude(t => t.FormulaProject)
                .Where(c => projectIds.Contains(c.ProjectId));

            if (_principal.IsVendor())
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.ProjectTaskVendors.Any(x => x.VendorGuid == userId && x.Status == ProjectRequestStatus.Accepted));
            }
            else if (_principal.IsWorker())
            {
                tasksQuery = tasksQuery.Where(t => t.ProjectTaskUsers.Any(x => x.UserId == userId));
            }

            if (selectTasksForToday)
            {
                tasksQuery = tasksQuery.Where(t => t.Status != TaskStatusType.New
                                                   || t.StartDate < _dateTimeService.TodayUtc.AddDays(1));
            }

            var sql = tasksQuery.ToSql();
            var result = _mapper.Map<List<TaskDto>>(tasksQuery);
            var stats = new Dictionary<int, short?>();
            if (_principal.IsVendor())
            {
                stats = await _repo.Read<FormulaTaskStatistic>().Where(w => w.Type == StatisticType.Rating)
                    .Join(_repo.Read<ProjectTask>().Where(c => projectIds.Contains(c.ProjectId)),
                    outer => outer.ProjectTaskId, inner => inner.Id, (stat, task) => stat)
                    .GroupBy(g => g.ProjectTaskId)
                    .ToDictionaryAsync(k => k.Key, v => v.OrderByDescending(o => o.Created).FirstOrDefault().Value);
            }
            foreach (var task in result)
            {
                task.CanBeProccessed = task.Status == "InProgress" && task.AssignedUserIds.Contains(userId);
                task.CanBeReviewed = task.Status == "NeedsReview" && task.ReviewingUserIds.Contains(userId);
                if (stats.ContainsKey(task.Id))
                    task.ReviewRating = stats[task.Id];
            };

            return result;
        }

        public async Task<List<TaskDto>> GetProjectTaskResourcesAsync(int projectId)
        {
            var projectTasks = await GetTasksAsync(new List<int> { projectId });

            foreach (var task in projectTasks)
            {
                var notes = await _repo.Read<ProjectNote>()
                    .Where(w => w.ProjectTaskId == task.Id && w.IsPublished)
                    .ToListAsync();

                task.Resources = _mapper.Map<List<ResourceDto>>(
                                    await _taskNeo4JRepository.GetTaskAndSharedResourcesAsync(task.Id, task.ProjectId),
                                    opts =>
                                    {
                                        opts.Items.Add("urlBuilder",
                                            (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.ProjectTask)));
                                    }
                                )
                    .Where(w => w.IsPublished)
                    .ToList();

                var projectResources = await _repo.Read<ResourceProject>()
                    .Include(c => c.Resource)
                    .Where(c => c.ProjectId == projectId)
                    .Select(s => s.Resource)
                    .ToListAsync();

                var mappedProjectResources = _mapper.Map<List<ResourceDto>>(projectResources,
                    opts =>
                    {
                        opts.Items.Add("urlBuilder",
                            (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.Project)));
                        opts.Items.Add("isShared", true);
                    })
                    .Where(w => w.IsPublished)
                    .ToList();

                task.Notes = notes.Select(Mapper.Map<ProjectNotesDto>).ToList();
                task.Resources.AddRange(mappedProjectResources);
            }

            return projectTasks;
        }

        public async Task<ListViewTaskDto> GetTaskInStatusById(int taskId)
        {
            var task = await _repo.Read<ProjectTask>()
                .SingleOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
                throw new NotFoundException("Task not found.");

            ProjectTask result = new ProjectTask();

            if (task.Status == TaskStatusType.InProgress)
            {
                result = await _repo.Read<ProjectTask>()
                    .Include(i => i.ProjectTaskUsers)
                        .ThenInclude(t => t.User)
                        .ThenInclude(t => t.Profile)
                    .Where(w => w.Id == taskId
                        && w.Status == TaskStatusType.InProgress
                        && w.ProjectTaskUsers.Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Assigned))
                    .SingleOrDefaultAsync();
            }

            if (task.Status == TaskStatusType.NeedsReview)
            {
                result = await _repo.Read<ProjectTask>()
                    .Include(i => i.ProjectTaskUsers)
                        .ThenInclude(t => t.User)
                        .ThenInclude(t => t.Profile)
                    .Where(w => w.Id == taskId
                        && w.Status == TaskStatusType.NeedsReview
                        && w.ProjectTaskUsers.Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing))
                    .SingleOrDefaultAsync();
            }

            return _mapper.Map<ListViewTaskDto>(result);
        }

        private string GetProfileImage(HomeListViewTaskDto task, Dictionary<Guid, string> imageUrlMap)
        {
            string profileImageUrl = String.Empty;

            if (task != null && task.ProccessingUserId.HasValue)
            {
                if (imageUrlMap.ContainsKey(task.ProccessingUserId.Value))
                {
                    profileImageUrl = imageUrlMap[task.ProccessingUserId.Value];
                }
                else
                {
                    profileImageUrl = String.IsNullOrEmpty(task.ProfileImage)
                        ? String.Empty
                        : _storageService.GetProfileImageUri($"{task.ProccessingUserId.Value}/{task.ProfileImage}");

                    imageUrlMap.Add(task.ProccessingUserId.Value, profileImageUrl);
                }
            }

            return profileImageUrl;
        }

        public async Task<HomeDashboardDto> GetTasksInStatusAsync(Guid userId)
        {
            await CheckAccess(userId, false);

            var userTasks = await _repo.ExecuteSql<HomeListViewTaskDto>(
                _mapper,
                "[dbo].[uspGetAllTasksForUsers] @INPUT_PRIMARYUSERID",
                new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@INPUT_PRIMARYUSERID", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId }
                }
            )
            .ToListAsync();

            Dictionary<Guid, string> imageUrlMap = new Dictionary<Guid, string>();

            var tasks = userTasks
                .Select(s => new HomeListViewTaskDto
                {
                    Id = s.Id,
                    FormulaId = s.FormulaId,
                    ProjectId = s.ProjectId,
                    Title = s.Title,
                    StatusEnum = s.StatusEnum,
                    Status = s.Status,
                    FormulaName = s.FormulaName,
                    ProjectName = s.ProjectName,
                    ProccessingUserId = s.ProccessingUserId,
                    ProccessingUserName = s.ProccessingUserName,
                    ProcessingUserRole = s.ProcessingUserRole,
                    ProfileImage = s.ProccessingUserId.HasValue ? GetProfileImage(s, imageUrlMap) : String.Empty,
                    AverageTAT = s.AverageTAT,
                    StartDate = s.StartDate,
                    DueDate = s.DueDate,
                    IsRead = s.IsRead
                })
                .ToList();

            var users = userTasks
                //.Where(w => w.ProccessingUserId.HasValue && w.ProcessingUserRole != UserRoles.Vendor) // added logic for new role CompanyWorker WRT Sprint 10B
                //.Where(w => w.ProccessingUserId.HasValue && (w.ProcessingUserRole != UserRoles.Vendor || w.ProcessingUserRole != UserRoles.CompanyWorker)) // added logic for new role CompanyWorker WRT Sprint 10B
                .Where(w => w.ProccessingUserId.HasValue && (w.ProcessingUserRole != UserRoles.Vendor || w.ProcessingUserRole != UserRoles.CompanyWorker || w.ProcessingUserRole != UserRoles.Company)) // added logic for new role Company WRT Sprint 10B
                .GroupBy(g => new { g.ProccessingUserId, g.ProccessingUserName })
                .Select(s => new UserFilterItemDto
                {
                    UserId = s.Key.ProccessingUserId.Value,
                    FullName = s.Key.ProccessingUserName,
                    Roles = new List<string> { s.Select(x => x.ProcessingUserRole).FirstOrDefault() }
                })
                .ToList();

            var outsouceUsers = userTasks
                //.Where(w => w.ProccessingUserId.HasValue && w.ProcessingUserRole == UserRoles.Vendor) // added logic for new role CompanyWorker WRT Sprint 10B
                //.Where(w => w.ProccessingUserId.HasValue && (w.ProcessingUserRole != UserRoles.Vendor || w.ProcessingUserRole != UserRoles.CompanyWorker)) // added logic for new role CompanyWorker WRT Sprint 10B
                .Where(w => w.ProccessingUserId.HasValue && (w.ProcessingUserRole != UserRoles.Vendor || w.ProcessingUserRole != UserRoles.CompanyWorker || w.ProcessingUserRole != UserRoles.Company)) // added logic for new role Company WRT Sprint 10B
                .GroupBy(g => new { g.ProccessingUserId, g.ProccessingUserName })
                .Select(s => new UserFilterItemDto
                {
                    UserId = s.Key.ProccessingUserId.Value,
                    FullName = s.Key.ProccessingUserName,
                    Roles = new List<string> { s.Select(x => x.ProcessingUserRole).FirstOrDefault() }
                })
                .ToList();

            if (_principal.IsWorker())
            {
                var workerTasks = tasks
                    .Where(w => w.ProccessingUserId.HasValue && w.ProccessingUserId == userId && w.ProcessingUserRole.Contains(UserRoles.Worker))
                    .OrderBy(x => x.StatusEnum)
                    .ThenBy(x => x.DueDate)
                    .ToList();

                return new HomeDashboardDto
                {
                    UserTasks = workerTasks,
                    UsersList = users.Where(w => w.UserId == userId).ToList(),
                    OutsourceList = outsouceUsers.Where(w => w.UserId == userId).ToList(),
                };
            }

            return new HomeDashboardDto
            {
                UserTasks = tasks
                    .OrderBy(x => x.StatusEnum)
                    .ThenBy(x => x.DueDate)
                    .ToList(),
                UsersList = users,
                OutsourceList = outsouceUsers
            };
        }


        public async Task<IList<ListViewTaskDto>> GetVendorTasksInStatusAsync(Guid userId)
        {
            var vendorTasks = await _repo.ExecuteSql<ListViewTaskDto>(
                    _mapper,
                    "[dbo].[uspGetVendorTasksByVendorGuid] @VendorGuid",
                    new List<SqlParameter> { new SqlParameter { ParameterName = "@VendorGuid", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId } }
                ).ToListAsync();

            return vendorTasks;
        }

        public async Task<IList<ListViewTaskDto>> GetTasksInStatusAsync(IEnumerable<int> projectIds,
            string statuses,
            int skip,
            int take,
            bool selectTasksForToday = false,
            Guid? userIdToLoad = null
            )
        {
            var userId = userIdToLoad ?? _principal.GetUserId();

            await CheckAccess(userId, false);

            List<ProjectTask> tasks = new List<ProjectTask>();

            foreach (string status in statuses.Split(','))
            {
                var statusEnum = (TaskStatusType)Enum.Parse(typeof(TaskStatusType), status);

                if (status == "New")
                {
                    tasks.AddRange(_repo.Read<ProjectTask>()
                    .Include(t => t.ProjectTaskVendors)
                    .Include(t => t.ProjectTaskUsers)
                    .Include(t => t.Project)
                    .Include(t => t.RecurrenceOptions)
                    .Include(t => t.FormulaTask)
                        .ThenInclude(i => i.FormulaProject)
                    .Where(c => projectIds.Contains(c.ProjectId)
                        && c.Status == statusEnum
                        && ((c.ProjectTaskVendors.Count > 0
                        && !c.ProjectTaskVendors.Any(t => t.Status == ProjectRequestStatus.Accepted))))
                    .OrderBy(t => t.Id));
                }
                else if (status == "NeedsReview")
                {
                    tasks.AddRange(_repo.Read<ProjectTask>()
                        .Include(t => t.Project)
                        .Include(t => t.ProjectTaskUsers)
                        .Include(t => t.ProjectTaskUsers)
                        .Include(t => t.RecurrenceOptions)
                        .Include(t => t.ReviewingUser)
                            .ThenInclude(t => t.Profile)
                        .Include(t => t.FormulaTask)
                            .ThenInclude(i => i.FormulaProject)
                    .Where(c => projectIds.Contains(c.ProjectId)
                        && c.Status == statusEnum && c.ProjectTaskUsers
                                    .Any(t => t.UserId == userId && t.ProjectTaskUserType == ProjectTaskUserType.Reviewing))
                    .OrderBy(t => t.Id));
                }
                else
                {
                    if (_principal.IsVendor())
                    {

                        tasks.AddRange(_repo.Read<ProjectTask>()
                            .Include(t => t.Project)
                            .Include(t => t.ProjectTaskUsers)
                            .Include(t => t.ProjectTaskVendors)
                            .Include(t => t.RecurrenceOptions)
                            .Include(t => t.ProccessingUser)
                            .ThenInclude(t => t.Profile)
                            .Where(c => projectIds.Contains(c.ProjectId)
                                && c.Status == statusEnum &&
                                c.ProjectTaskVendors.Any(t => t.Status == ProjectRequestStatus.Accepted && t.VendorGuid == userId))
                            .OrderBy(t => t.Id)
                            .Skip(skip)
                            .Take(take));

                    }
                    else
                    {
                        tasks.AddRange(_repo.Read<ProjectTask>()
                            .Where(c => projectIds.Contains(c.ProjectId) && c.Status == statusEnum
                                    && (c.ProccessingUserGuid == userId || (c.ProccessingUserGuid == null && c.ProjectTaskUsers
                                                                                .Any(t => t.UserId == userId
                                                                                          && t.ProjectTaskUserType == ProjectTaskUserType.Assigned))))
                            .Include(t => t.ProjectTaskUsers)
                            .Include(t => t.ProccessingUser)
                                .ThenInclude(t => t.Profile)
                            .Include(t => t.Project)
                            .Include(t => t.ProjectTaskVendors)
                            .Include(t => t.RecurrenceOptions)
                            .Include(t => t.FormulaTask)
                                .ThenInclude(i => i.FormulaProject)
                            .OrderBy(t => t.Id));
                    }
                }
            }

            return _mapper.Map<IList<ListViewTaskDto>>(tasks);
        }

        private async Task CheckAccess(Guid userId, bool shouldRedirect = true)
        {
            var accessQuery = _repo
                .Read<Project>();
            var user = _repo.Read<User>().FirstOrDefault(t => t.Id == userId);

            //TODO need to refactor this for now EF Core doesnt support IQueryable Union #246
            var isAssignedAsManager = true;

            if (await _userManager.IsInRoleAsync(user, UserRoles.Owner))
            {
                accessQuery = accessQuery.Where(w => w.OwnerGuid == userId);
            }
            else if (await _userManager.IsInRoleAsync(user, UserRoles.Manager))
            {
                isAssignedAsManager = await _repo.Read<UserProject>().Where(w => w.UserId == userId).AnyAsync();
            }
            //else if (await _userManager.IsInRoleAsync(user, UserRoles.Vendor))
            //{
            //    accessQuery = accessQuery.Join(_repo.Read<ProjectTaskVendor>()
            //            .Include(i => i.ProjectTask)
            //            .Where(w => w.VendorGuid == userId && w.Status == ProjectRequestStatus.Accepted)
            //            .Select(s => s.ProjectTask.ProjectId),
            //        outer => outer.Id,
            //        inner => inner,
            //        (project, userProject) => project
            //    );
            //}
            //else if (await _userManager.IsInRoleAsync(user, UserRoles.Vendor) || await _userManager.IsInRoleAsync(user, UserRoles.CompanyWorker)) // added logic for new role CompanyWorker WRT Sprint 10B
            //{
            //    accessQuery = accessQuery.Join(_repo.Read<ProjectTaskVendor>()
            //            .Include(i => i.ProjectTask)
            //            .Where(w => w.VendorGuid == userId && w.Status == ProjectRequestStatus.Accepted)
            //            .Select(s => s.ProjectTask.ProjectId),
            //        outer => outer.Id,
            //        inner => inner,
            //        (project, userProject) => project
            //    );
            //}
            else if (await _userManager.IsInRoleAsync(user, UserRoles.Vendor) || await _userManager.IsInRoleAsync(user, UserRoles.CompanyWorker) || await _userManager.IsInRoleAsync(user, UserRoles.Company)) // added logic for new role Company WRT Sprint 10B
            {
                accessQuery = accessQuery.Join(_repo.Read<ProjectTaskVendor>()
                        .Include(i => i.ProjectTask)
                        .Where(w => w.VendorGuid == userId && w.Status == ProjectRequestStatus.Accepted)
                        .Select(s => s.ProjectTask.ProjectId),
                    outer => outer.Id,
                    inner => inner,
                    (project, userProject) => project
                );
            }
            else
            {
                accessQuery = accessQuery
                    .Join(_repo.Read<ProjectTaskUser>().Where(w => w.UserId == userId)
                            .Select(sm => sm.ProjectTask.ProjectId),
                        inner => inner.Id, outer => outer,
                        (project, userProject) => project);
            }

            if (!await accessQuery.AnyAsync() && isAssignedAsManager)
                throw new ForbiddenException(shouldRedirect: shouldRedirect);
        }

        public async Task<TaskDto> GetTaskAsync(int taskId)
        {
            var task = await _repo.Read<ProjectTask>()
                .Include(c => c.ProjectTaskUsers)
                .Include(c => c.RecurrenceOptions)
                .Include(c => c.ChildTasks)
                .Include(c => c.ParentTasks)
                .Include(c => c.AssignedConditionOptions)
                    .ThenInclude(co => co.Condition)
                    .ThenInclude(c => c.Task)
                .Include(c => c.Condition)
                    .ThenInclude(t => t.Options)
                .Include(c => c.ProjectTaskVendors)
                    .ThenInclude(t => t.Vendor)
                    .ThenInclude(t => t.Profile)
                .Include(t => t.FormulaTask)
                    .ThenInclude(t => t.FormulaProject)
                .Where(c => c.Id == taskId)
                .FirstOrDefaultAsync();

            if (task == null)
                throw new ValidationException("Task with specified id doesn't exist");

            var mappedTask = _mapper.Map<TaskDto>(task);

            mappedTask.Resources = _mapper.Map<List<ResourceDto>>(
                await _taskNeo4JRepository.GetTaskAndSharedResourcesAsync(task.Id, task.ProjectId),
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.ProjectTask)));
                }
            );

            var projectResources = await _repo.Read<ResourceProject>()
                .Include(c => c.Resource)
                .Where(c => c.ProjectId == mappedTask.ProjectId)
                .Select(s => s.Resource)
                .ToListAsync();

            var mappedProjectResources = _mapper.Map<List<ResourceDto>>(projectResources,
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.Project)));
                    opts.Items.Add("isShared", true);
                });

            mappedTask.Resources.AddRange(mappedProjectResources);

            if (task.ProccessingUserGuid.HasValue)
            {
                mappedTask.ProccessingUserName = await _repo.Read<User>()
                    .Include(co => co.Profile)
                    .Where(c => c.Id == task.ProccessingUserGuid.Value)
                    .Select(s => s.Profile.FullName)
                    .FirstAsync();
            }

            if (task.ReviewingUserGuid.HasValue)
            {
                mappedTask.ReviewingUserName = await _repo.Read<User>()
                    .Include(co => co.Profile)
                    .Where(c => c.Id == task.ReviewingUserGuid.Value)
                    .Select(s => s.Profile.FullName)
                    .FirstAsync();
            }

            var userId = _principal.GetUserId();

            mappedTask.CanBeProccessed = task.ProjectTaskUsers.Any(t => t.UserId == userId
                                                                        && t.ProjectTaskUserType == ProjectTaskUserType.Assigned);

            mappedTask.CanBeReviewed = task.ProjectTaskUsers.Any(t => t.UserId == userId
                                                                      && t.ProjectTaskUserType == ProjectTaskUserType.Reviewing);

            mappedTask.ProccessingUserId = task.ProccessingUserGuid;
            mappedTask.ReviewingUserId = task.ReviewingUserGuid;

            var isVendor = _principal.IsVendor();

            // Commenting the check, as Client wants the Training tab to be shown in all cases, irrespective of the Public or Custom Formula
            // Keep the check commented and ShowTrainingTab = true till further update from client.
            // Code changes done after discussion with the manager, Dated: Nov 19, 2020
            //if (task.FormulaTask != null)
            //{
            //    if (!isVendor && task.FormulaTask.PublicVaultFormulaTaskID != null)
            //    {
            //        mappedTask.ShowTrainingTab = false;
            //    }
            //    else
            //    {
            //        mappedTask.ShowTrainingTab = true;
            //    }
            //}

            if (task.FormulaTask != null)
            {
                if (task.IsTrainingLocked && !isVendor && !_principal.IsAdmin())
                {
                    mappedTask.ShowTrainingTab = false;
                }
                else
                {
                    mappedTask.ShowTrainingTab = true;
                }
            }

            //mappedTask.ShowTrainingTab = true;

            mappedTask = await ApplyVendorStatistic(mappedTask);

            if (mappedTask.Status.Equals("New") || mappedTask.Status.Equals("InProgress") || mappedTask.Status.Equals("Completed"))
            {
                mappedTask.TaskCheckLists = await _repo.Read<ProjectTaskChecklist>()
                    .Where(t => t.ProjectTaskId == taskId && t.Type == TodoType.Resource)
                    .Select(t => new TaskChecklistDto
                    {
                        Id = t.Id,
                        Type = (int)TodoType.Resource,
                        Name = t.Name,
                        ProjectTaskId = t.ProjectTaskId,
                        TodoIsChecked = t.TodoIsChecked
                    })
                    .ToListAsync();
            }

            if (mappedTask.Status.Equals("NeedsReview"))
            {
                mappedTask.TaskCheckLists = await _repo.Read<ProjectTaskChecklist>()
                    .Where(t => t.ProjectTaskId == taskId && t.Type == TodoType.Reviewer)
                    .Select(t => new TaskChecklistDto
                    {
                        Id = t.Id,
                        Type = (int)TodoType.Reviewer,
                        Name = t.Name,
                        ProjectTaskId = t.ProjectTaskId,
                        ReviewerIsChecked = t.ReviewerIsChecked
                    })
                    .OrderBy(t => t.Type).ThenBy(t => t.Id)
                    .ToListAsync();
            }

            return mappedTask;
        }

        private async Task<TaskDto> ApplyVendorStatistic(TaskDto mappedTask)
        {
            if (mappedTask.Outsources.Count > 0)
            {
                var vendorStatistics = await _repo.Read<ProjectTask>()
                    .Where(c => c.Id == mappedTask.Id)
                    .Join(_repo.Read<FormulaTaskStatistic>()
                        .Where(w => !w.FormulaTaskStatisticId.HasValue && w.Value.HasValue),
                        outer => outer.FormulaTaskId,
                        inner => inner.FormulaTaskId,
                        (task, statistic) => new
                        {
                            statistic.VendorGuid,
                            statistic.Value.Value,
                            statistic.Type
                        })
                        .GroupBy(g => new { g.Type, g.VendorGuid })
                        .Select(s => new
                        {
                            s.Key.Type,
                            s.Key.VendorGuid,
                            Avg = (double)s.Sum(sum => sum.Value) / s.Count()
                        }).ToListAsync();
                var vendorRespondStatistics = await _repo.Read<ProjectTask>()
                    .Where(c => c.Id == mappedTask.Id)
                    .SelectMany(sm => sm.ProjectTaskVendors)
                    .Join(_repo.Read<FormulaTaskStatistic>()
                        .Where(w => !w.FormulaTaskStatisticId.HasValue && w.Value.HasValue && (w.Type == StatisticType.Responding || w.Type == StatisticType.Messaging)),
                        outer => outer.VendorGuid,
                        inner => inner.VendorGuid,
                        (task, statistic) => new
                        {
                            statistic.VendorGuid,
                            statistic.Value.Value,
                            statistic.Type
                        })
                        .GroupBy(g => new { g.Type, g.VendorGuid })
                        .Select(s => new
                        {
                            s.Key.Type,
                            s.Key.VendorGuid,
                            Avg = (double)s.Sum(sum => sum.Value) / s.Count()
                        }).ToListAsync();
                mappedTask.Outsources.ForEach(vendor =>
                {
                    var vendorStat = vendorStatistics.Where(w => w.VendorGuid == vendor.Id);
                    vendor.AvgRating = vendorStat.FirstOrDefault(w => w.Type == StatisticType.Rating)?.Avg;
                    vendor.AvgResponding = vendorRespondStatistics
                        .FirstOrDefault(w => w.VendorGuid == vendor.Id && w.Type == StatisticType.Responding)?.Avg;
                    vendor.AvgMessaging = vendorRespondStatistics
                        .FirstOrDefault(w => w.VendorGuid == vendor.Id && w.Type == StatisticType.Messaging)?.Avg;
                    vendor.AvgWorking = vendorStat.FirstOrDefault(w => w.Type == StatisticType.Working)?.Avg;
                });
            }
            return mappedTask;
        }

        public async Task<TaskDto> AddTaskAsync(UpdateTaskDto model)
        {
            if (_principal.IsWorker())
                throw new ForbiddenException("You have no rigths for adding task.");

            if (model == null)
                throw new ArgumentException(nameof(model));

            //TODO fix this on UI
            if (!model.IsRecurrent)
            {
                model.RecurrenceOptions = null;
            }

            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            {
                using (var trx = _repo.Transaction())
                {
                    var task = _mapper.Map<ProjectTask>(model);
                    var now = _dateTimeService.NowUtc;
                    task.OwnerGuid = _principal.GetUserId();
                    task.DateCreated = now;
                    task.TaskHistories.Add(new TaskHistory
                    {
                        HistoryTime = now,
                        Type = ActivityType.New
                    });

                    if (model.IsRecurrent)
                    {
                        var recurrenceDetails = _dateTimeService.ParseRecurrenceAsap(task.RecurrenceOptions, now);
                        task.RecurrenceOptions.Cron = recurrenceDetails.Cron;

                        // ReSharper disable once PossibleNullReferenceException
                        task.RecurrenceOptions.NextOccurenceDate = _dateTimeService.GetNextOccurence(recurrenceDetails);
                    }

                    await trx.AddAsync(task);
                    await trx.SaveChangesAsync();

                    await _taskNeo4JRepository.AddTaskAsync(task.Id, task.ProjectId);
                    await trx.SaveChangesAsync();

                    if (model.IsConditional)
                    {
                        var conditionIds = model.Condition.Options.Where(w => w.AssignedTaskId != 0).Select(s => s.AssignedTaskId);
                        var conditionTasks = await trx.Track<ProjectTask>()
                            .Where(w => conditionIds.Contains(w.Id)).ToListAsync();
                        if (conditionTasks.Any(t => t.ParentTaskId != task.ParentTaskId))
                        {
                            throw new ValidationException("Connection between this tasks is not allowed.");
                        }
                        foreach (var condition in conditionTasks)
                        {
                            if (condition.Status == TaskStatusType.New)
                            {
                                var removeJobTasks = new List<Task> { _scheduleService.RemoveJob(trx, condition.Id) };
                                if (condition.FormulaId.HasValue && await _taskNeo4JRepository.IsRootAsync(condition.FormulaId.Value))
                                {
                                    removeJobTasks.AddRange(
                                        from formulaTaskId in await _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(condition.Id)
                                        select _scheduleService.RemoveJob(trx, formulaTaskId));
                                }
                                await Task.WhenAll(removeJobTasks);
                            }
                            await _taskNeo4JRepository.AddTaskConditionAsync(
                                task.Condition.Options.Where(w => w.AssignedTaskId == condition.Id).Select(s => s.Id).First(),
                                task.Id, condition.Id);

                            await _formulaTaskJobService.UpdateFormulaTaskTime(trx, condition);
                            await _startDateHelperService.UpdateStartDatesForTreeIfNeeded(trx, task, condition);
                        }
                    }

                    await _taskScheduleService.ScheduleNewTask(trx, task, task.RecurrenceOptions?.IsAsap ?? false);

                    if (model.AssignedUserIds != null && model.AssignedUserIds.Any())
                    {
                        await UpdateProjectTaskUsers(task.Id, ProjectTaskUserType.Assigned, model.AssignedUserIds, trx);
                    }
                    else
                    {
                        throw new ValidationException("Assigned users is required");
                    }
                    if (model.ReviewingUserIds != null && model.ReviewingUserIds.Any())
                    {
                        await UpdateProjectTaskUsers(task.Id, ProjectTaskUserType.Reviewing, model.ReviewingUserIds, trx);
                    }

                    // Add the Todo Checklist
                    if (model.AddTodoCheckList != null && model.AddTodoCheckList.Milestones.Count > 0)
                    {
                        foreach (var milestone in model.AddTodoCheckList.Milestones)
                        {
                            await trx.AddAsync(new ProjectTaskChecklist()
                            {
                                ProjectTaskId = task.Id,
                                Type = TodoType.Resource,
                                Name = milestone,
                                DateCreated = _dateTimeService.NowUtc
                            });

                            // added the line to preserve the order of the inserted records
                            await trx.SaveChangesAsync();
                        }
                    }

                    // Add the Reviewer Checklist
                    if (model.AddReviewerCheckList != null && model.AddReviewerCheckList.Milestones.Count > 0)
                    {
                        foreach (var milestone in model.AddReviewerCheckList.Milestones)
                        {
                            await trx.AddAsync(new ProjectTaskChecklist()
                            {
                                ProjectTaskId = task.Id,
                                Type = TodoType.Reviewer,
                                Name = milestone,
                                DateCreated = _dateTimeService.NowUtc
                            });

                            // added the line to preserve the order of the inserted records
                            await trx.SaveChangesAsync();
                        }
                    }

                    // Replace the projectTaskIds to the ProjectTaskNotes, when the new task is created
                    // and a projectnote is added with a taskid = -999, for New Task cases
                    var taskNotes = await trx.Track<ProjectNote>()
                        .Where(n => n.ProjectId == task.ProjectId && n.ProjectTaskId == -999)
                        .ToListAsync();

                    foreach (var note in taskNotes)
                    {
                        note.ProjectTaskId = task.Id;
                    }

                    await trx.SaveAndCommitAsync();
                    transaction.Commit();

                    if (task.ProjectTaskUsers.Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Assigned))
                        await _notificationService.SendAssignToTaskAsync(trx, task.Id);

                    var res = await _repo.Read<ProjectTask>()
                        .Include(c => c.RecurrenceOptions)
                        .Include(c => c.Condition)
                        .ThenInclude(c => c.Options)
                        .FirstOrDefaultAsync(x => x.Id == task.Id);

                    return _mapper.Map<TaskDto>(res);
                }
            }
        }

        public async Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskDto model)
        {
            if (_principal.IsWorker())
                throw new ForbiddenException("You have no rigths for updating task.");

            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var task = await trx.Track<ProjectTask>()
                    .Include(c => c.ChildTasks)
                    .Include(c => c.ParentTasks)
                    .Include(i => i.RecurrenceOptions)
                    .Include(t => t.Condition)
                        .ThenInclude(t => t.Options)
                    .FirstOrDefaultAsync(x => x.Id == taskId);

                if (task == null)
                    throw new ValidationException("Task not found.");

                var taskOld = await trx.Read<ProjectTask>()
                    .Include(i => i.ParentTasks)
                        .ThenInclude(t => t.ParentTask)
                    .Include(i => i.RecurrenceOptions)
                    .Include(i => i.AssignedConditionOptions)
                    .FirstOrDefaultAsync(x => x.Id == taskId);

                var oldConditions = task.TaskConditionId.HasValue
                    ? task.Condition.Options
                        .Where(w => w.AssignedTaskId.HasValue)
                        .Select(w => w.AssignedTaskId.Value)
                        .ToList()
                    : new List<int>();

                model.Id = taskId;
                _mapper.Map(model, task);
                task.Status = taskOld.Status;
                if (model.IsRecurrent && task.Status == TaskStatusType.New)
                {
                    var recurrenceDetail = await _taskScheduleService.GetNextOccurence(trx, taskId, task.RecurrenceOptions);
                    task.RecurrenceOptions.NextOccurenceDate = recurrenceDetail.StartFrom;
                    task.RecurrenceOptions.Cron = recurrenceDetail.Cron;
                }
                task.LastUpdated = _dateTimeService.NowUtc;

                if (model.AssignedUserIds.Any())
                {
                    await UpdateProjectTaskUsers(taskId, ProjectTaskUserType.Assigned, model.AssignedUserIds, trx);
                }

                if (model.ReviewingUserIds.Any())
                {
                    await UpdateProjectTaskUsers(taskId, ProjectTaskUserType.Reviewing, model.ReviewingUserIds, trx);
                }

                await trx.SaveChangesAsync();
                await _taskScheduleService.UpdateTaskTree(trx, taskOld, task, model, oldConditions);

                // Edit the staus of the todos
                if (model.EditCheckLists != null && model.EditCheckLists.Count != 0)
                {
                    foreach (var item in model.EditCheckLists)
                    {
                        var todo = await trx.Track<ProjectTaskChecklist>()
                            .SingleOrDefaultAsync(t => t.Id == item.Id);

                        // the property is null from the UI, then save whaterver is the value from the database
                        todo.TodoIsChecked = item.TodoIsChecked.HasValue ? item.TodoIsChecked.Value : todo.TodoIsChecked;
                        todo.ReviewerIsChecked = item.ReviewerIsChecked.HasValue ? item.ReviewerIsChecked.Value : todo.ReviewerIsChecked;
                        todo.LastUpdated = _dateTimeService.NowUtc;
                    }

                    await trx.SaveChangesAsync();
                }

                await trx.SaveAndCommitAsync();
                transaction.Commit();
            }

            return await GetTaskAsync(model.Id);
        }

        public async Task<TaskDto> StopOutsource(int taskId)
        {
            using (var trx = _repo.Transaction())
            {
                var vendorNotification = await trx.Track<ProjectTaskVendor>()
                .Include(t => t.ProjectTask)
                    .ThenInclude(t => t.Project)
                .Include(t => t.Vendor)
                .FirstOrDefaultAsync(t => t.ProjectTaskId == taskId && t.Status == ProjectRequestStatus.Accepted);

                if (vendorNotification == null)
                {
                    throw new ValidationException("Notification doesn't exist");
                }

                var now = _dateTimeService.NowUtc;

                var creditLog = new CreditLog
                {
                    Amount = vendorNotification.Price,
                    AmountWithTax = await CalculateAmountWithTax(trx, vendorNotification.Price),
                    HistoryTime = now,
                    Type = Global.Enums.CreditsLogType.StopOutsource,
                    ProjectTaskId = taskId
                };

                var userId = _principal.GetUserId();

                if (vendorNotification.VendorGuid == userId)
                {
                    vendorNotification.Status = ProjectRequestStatus.Declined;
                    vendorNotification.LastModified = now;
                    creditLog.ManagerId = vendorNotification.ProjectTask.Project.OwnerGuid;
                    creditLog.VendorId = userId;
                }
                else if (_principal.IsOwner() || _principal.IsManager())
                {
                    vendorNotification.Status = ProjectRequestStatus.DeclinedByOwner;
                    vendorNotification.LastModified = now;
                    creditLog.ManagerId = userId;
                    creditLog.VendorId = vendorNotification.VendorGuid;
                }
                else
                {
                    throw new ValidationException("You don't have rights to do that");
                }

                vendorNotification.ProjectTask.ProccessingUserGuid = null;
                await _taskHistoryService.Write(vendorNotification.ProjectTask.Id,
                    ActivityType.UpdateProcessingUser,
                    null,
                    trx);

                trx.Add(creditLog);

                trx.SaveAndCommit();
            }

            return await GetTaskAsync(taskId);
        }

        public async Task StopVendorTaskOnCancelNudge(int taskId)
        {
            using (var trx = _repo.Transaction())
            {
                var vendorNotification = await trx.Track<ProjectTaskVendor>()
                    .Include(t => t.ProjectTask)
                        .ThenInclude(t => t.Project)
                    .Include(t => t.Vendor)
                    .FirstOrDefaultAsync(t => t.ProjectTaskId == taskId && t.Status == ProjectRequestStatus.Accepted);

                if (vendorNotification == null)
                {
                    throw new ValidationException("Notification doesn't exist");
                }

                var now = _dateTimeService.NowUtc;
                var userId = _principal.GetUserId();

                var creditLog = new CreditLog
                {
                    Amount = vendorNotification.Price,
                    AmountWithTax = await CalculateAmountWithTax(trx, vendorNotification.Price),
                    HistoryTime = now,
                    Type = CreditsLogType.StopOutsource,
                    ProjectTaskId = taskId
                };

                vendorNotification.Status = ProjectRequestStatus.CancelAfterNudge;
                vendorNotification.LastModified = now;
                creditLog.ManagerId = vendorNotification.ProjectTask.Project.OwnerGuid;
                creditLog.VendorId = userId;

                vendorNotification.ProjectTask.ProccessingUserGuid = null;

                await _taskHistoryService.Write(vendorNotification.ProjectTask.Id,
                    ActivityType.UpdateProcessingUser,
                    null,
                    trx);

                var formulaTaskStatistic = _repo.Read<FormulaTaskStatistic>()
                    .FirstOrDefault(x => x.ProjectTaskId == taskId && x.Type == StatisticType.Working && x.VendorGuid == userId);

                trx.Add(new FormulaTaskStatistic
                {
                    Created = formulaTaskStatistic?.Created ?? now,
                    Completed = now,
                    FormulaTaskId = formulaTaskStatistic?.FormulaTaskId ?? 1,
                    FormulaTaskStatisticId = null,
                    ProjectTaskId = formulaTaskStatistic?.ProjectTaskId ?? taskId,
                    Type = StatisticType.CancelAfterNudge,
                    Value = null,
                    VendorGuid = formulaTaskStatistic?.VendorGuid ?? userId
                });

                trx.Add(creditLog);

                await _notificationService.SendStopVendorTaskOnCancelNudgeAsync(trx, vendorNotification.Id);

                trx.SaveAndCommit();
            }
        }

        private async Task UpdateProjectTaskUsers(int projectTaskId, ProjectTaskUserType projectTaskUserType, List<Guid> assignedUserIds, ITransactionScope trx)
        {
            var projectTaskUsers = _repo.Read<ProjectTaskUser>()
                                .Where(t => t.ProjectTaskId == projectTaskId
                                            && t.ProjectTaskUserType == projectTaskUserType).ToList();

            var projectTaskUsersToRemove = projectTaskUsers.Where(t => !assignedUserIds.Contains(t.UserId));
            trx.RemoveRange(projectTaskUsersToRemove);

            var userToAddForProjectTaskUsers = assignedUserIds.Where(newMapUserId => !projectTaskUsers.Any(t => t.UserId == newMapUserId));
            foreach (var userId in userToAddForProjectTaskUsers)
            {
                ProjectTaskUser projectTaskUser = new ProjectTaskUser
                {
                    UserId = userId,
                    ProjectTaskId = projectTaskId,
                    ProjectTaskUserType = projectTaskUserType
                };
                await trx.AddAsync(projectTaskUser);
                await _notificationService.SendAssignToTaskAsync(trx, projectTaskId);
            }
        }

        public async Task DeleteTask(int taskId)
        {
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            {
                using (var trx = _repo.Transaction())
                {
                    var task = trx.Track<ProjectTask>()
                        .Include(t => t.ProjectTaskUsers)
                        .Include(c => c.ChildTasks)
                        .Include(c => c.ParentTasks)
                        .Include(c => c.TaskHistories)
                        .Include(c => c.RecurrenceOptions)
                        .Include(c => c.Condition)
                        .Include(c => c.ResourceProjectTask)
                            .ThenInclude(c => c.Resource)
                        .Include(c => c.NotificationSettings)
                        .FirstOrDefault(p => p.Id == taskId);

                    if (task == null)
                        throw new ValidationException("Project is not found.");
                    var parentIds = (await _taskNeo4JRepository.GetParentTaskIdsAsync(task.Id)).ToList();
                    var childIds = await _taskNeo4JRepository.GetChildTaskIdsAsync(task.Id);
                    await _taskNeo4JRepository.DeleteTaskAsync(task.Id);

                    var resources = task.ResourceProjectTask.Select(s => s.Resource).ToList();
                    trx.RemoveRange(task.ResourceProjectTask);
                    await trx.SaveChangesAsync();
                    trx.RemoveRange(resources);
                    await _storageService.DeleteFileAsync(taskId.ToString(), StorageType.ProjectTask);
                    trx.RemoveRange(task.NotificationSettings);
                    await _scheduleService.RemoveJob(trx, task.Id);
                    trx.RemoveRange(task.ChildTasks);
                    trx.RemoveRange(task.ParentTasks);
                    trx.RemoveRange(task.TaskHistories);
                    trx.RemoveRange(task.ProjectTaskUsers);
                    if (task.RecurrenceOptions != null)
                        trx.Remove(task.RecurrenceOptions);
                    if (task.Condition != null)
                        trx.Remove(task.Condition);
                    if (task.FormulaId.HasValue)
                    {
                        await DeleteTasks(trx,
                            await trx.Read<ProjectTask>()
                            .Where(w => task.Id == w.ParentTaskId).Select(s => s.Id).ToListAsync());
                    }
                    trx.Remove(task);
                    if (parentIds.Any())
                    {
                        var graphIds = new List<int> { parentIds[0] };
                        for (int i = 1; i < parentIds.Count; i++)
                        {
                            if (!graphIds.Any(id => _taskNeo4JRepository.HasRelationsAsync(id, parentIds[i]).Result))
                            {
                                graphIds.Add(parentIds[i]);
                            }
                        }
                        foreach (var id in graphIds)
                        {
                            if (await _taskNeo4JRepository.IsGraphCompleted(id))
                            {
                                await _scheduleService.ResetProjectTaskTreeStatuses(id);
                            }
                        }
                    }
                    if (childIds.Any())
                    {
                        foreach (var childTask in await trx.Track<ProjectTask>()
                           .Include(c => c.RecurrenceOptions)
                           .Where(p => p.Status == TaskStatusType.New && !p.FormulaId.HasValue && childIds.Contains(p.Id))
                           .ToListAsync())
                        {
                            if (await _taskNeo4JRepository.IsRootAsync(childTask.Id))
                            {
                                if (childTask.RecurrenceOptionsId.HasValue)
                                {
                                    var recurrenceDetail = await _taskScheduleService.GetNextOccurence(trx, childTask.Id, childTask.RecurrenceOptions);
                                    task.RecurrenceOptions.NextOccurenceDate = recurrenceDetail.StartFrom;
                                    task.RecurrenceOptions.Cron = recurrenceDetail.Cron;
                                }
                                await _taskScheduleService.ScheduleNewTask(trx, childTask);
                            }
                        }
                    }

                    // delete the task checklists
                    await DeleteCheckListAsync(trx, taskId);

                    await trx.SaveAndCommitAsync();
                    transaction.Commit();
                }
            }
        }

        public async Task DeleteTasks(ITransactionScope trx, IEnumerable<int> taskIds)
        {
            if (taskIds.Any())
            {
                var tasks = await trx.Track<ProjectTask>()
                        .Include(c => c.ChildTasks)
                        .Include(c => c.ParentTasks)
                        .Include(c => c.TaskHistories)
                        .Include(c => c.AssignedConditionOptions)
                        .Include(c => c.RecurrenceOptions)
                        .Include(c => c.Condition)
                        .Include(c => c.ResourceProjectTask)
                            .ThenInclude(d => d.Resource)
                        .Include(c => c.NotificationSettings)
                        .Where(p => taskIds.Contains(p.Id))
                        .ToListAsync();

                if (!tasks.Any())
                {
                    throw new ValidationException("Tasks with with specified ids don't exist");
                }

                var resources = tasks.SelectMany(sm => sm.ResourceProjectTask.Select(s => s.Resource).ToList()).ToList();
                trx.RemoveRange(tasks.SelectMany(sm => sm.ResourceProjectTask));
                await trx.SaveChangesAsync();
                trx.RemoveRange(resources);
                foreach (var taskId in taskIds)
                {
                    await _taskNeo4JRepository.DeleteTaskAsync(taskId);
                    await _storageService.DeleteFileAsync(taskId.ToString(), StorageType.ProjectTask);
                    await _scheduleService.RemoveJob(trx, taskId);
                }
                trx.RemoveRange(tasks.SelectMany(c => c.NotificationSettings));
                trx.RemoveRange(tasks.SelectMany(c => c.ChildTasks));
                trx.RemoveRange(tasks.SelectMany(c => c.ParentTasks));
                trx.RemoveRange(tasks.SelectMany(c => c.TaskHistories));
                trx.RemoveRange(tasks.SelectMany(c => c.AssignedConditionOptions));
                trx.RemoveRange(tasks.Where(w => w.RecurrenceOptionsId.HasValue).Select(c => c.RecurrenceOptions));
                trx.RemoveRange(tasks.Where(w => w.TaskConditionId.HasValue).Select(c => c.Condition));
                foreach (var task in tasks.Where(w => w.FormulaId.HasValue))
                {
                    await DeleteTasks(trx,
                        await trx.Read<ProjectTask>()
                        .Where(w => task.Id == w.ParentTaskId).Select(s => s.Id).ToListAsync());
                }
                trx.RemoveRange(tasks);

                await trx.SaveChangesAsync();
            }
        }
        public async Task DeleteProjectTasksAsync(ITransactionScope trx, int projectId)
        {
            foreach (var taskId in await _taskNeo4JRepository.GetAllTaskWithResourcesAsync(projectId))
            {
                await _storageService.DeleteFileAsync(taskId.ToString(), StorageType.ProjectTask);
            }
            foreach (var taskId in await trx.Track<TaskJob>()
                    .Include(i => i.Task)
                    .Where(w => w.Task.ProjectId == projectId)
                    .GroupBy(s => s.TaskId).Select(s => s.Key)
                    .ToListAsync())
            {
                await _scheduleService.RemoveJob(trx, taskId);
            }

            await _entityFrameworkPlus.BulkDeleteAsync(trx.Track<NotificationEntity>()
                .Include(i => i.Task)
                .Where(w => w.Task.ProjectId == projectId));

            await _entityFrameworkPlus.BulkDeleteAsync(trx.Track<ResourceProjectTask>()
                .Include(i => i.ProjectTask)
                .Where(w => w.ProjectTask.ProjectId == projectId));

            await _entityFrameworkPlus.BulkDeleteAsync(trx.Track<ProjectTaskDependency>()
                .Include(i => i.ChildTask)
                .Where(w => w.ChildTask.ProjectId == projectId));
            await _entityFrameworkPlus.BulkDeleteAsync(trx.Track<ProjectTaskDependency>()
                .Include(i => i.ParentTask)
                .Where(w => w.ParentTask.ProjectId == projectId));
            await _entityFrameworkPlus.BulkDeleteAsync(trx.Track<TaskHistory>()
                .Include(i => i.Task)
                .Where(w => w.Task.ProjectId == projectId));
            await _entityFrameworkPlus.BulkDeleteAsync(trx.Track<ProjectTaskConditionOption>()
                .Include(i => i.Condition)
                .ThenInclude(i => i.Task)
                .Where(w => w.Condition.Task.ProjectId == projectId));
            await _entityFrameworkPlus.BulkUpdateAsync(trx.Track<ProjectTask>()
                .Where(w => w.ProjectId == projectId), f => new ProjectTask { TaskConditionId = null });
            await _entityFrameworkPlus.BulkDeleteAsync(trx.Track<ProjectTaskCondition>()
                .Include(i => i.Task)
                .Where(w => w.Task.ProjectId == projectId));
            await _entityFrameworkPlus.BulkDeleteAsync(trx.Track<ProjectTaskUser>()
                .Include(i => i.ProjectTask)
                .Where(w => w.ProjectTask.ProjectId == projectId));
            await _taskNeo4JRepository.DeleteAllProjectTasksAsync(projectId);
            await _entityFrameworkPlus.BulkDeleteAsync(trx.Track<ProjectTask>()
                    .Where(w => w.ProjectId == projectId));

            await trx.SaveChangesAsync();
        }
        #endregion

        #region Task Condition Service

        public async Task AssignTaskToConditionOption(int conditionOptionId, int? taskId)
        {
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var opt = trx.Track<ProjectTaskConditionOption>()
                    .Include(i => i.Condition)
                        .ThenInclude(i => i.Task)
                    .Include(i => i.AssignedTask)
                        .ThenInclude(i => i.RecurrenceOptions)
                    .FirstOrDefault(t => t.Id == conditionOptionId);

                if (opt == null)
                    throw new ValidationException("ConditionOption doesn't exist");

                // If we are detaching task and option is selected - deselect it
                if (opt.IsSelected && !taskId.HasValue)
                    opt.IsSelected = false;

                var assignedTaskId = opt.AssignedTaskId;
                opt.AssignedTaskId = taskId;
                var assignedTask = opt.AssignedTask;
                await trx.SaveChangesAsync();
                if (taskId.HasValue)
                {
                    var parentTask = await trx.Track<ProjectTask>()
                        .Include(i => i.RecurrenceOptions)
                        .Include(i => i.ChildTasks)
                        .Include(i => i.ParentTasks)
                        .Include(i => i.AssignedConditionOptions)
                        .FirstOrDefaultAsync(w => w.Id == opt.Condition.Task.Id);

                    var childTask = await trx.Track<ProjectTask>()
                        .Include(i => i.RecurrenceOptions)
                        .Include(i => i.ChildTasks)
                        .Include(i => i.ParentTasks)
                        .Include(i => i.AssignedConditionOptions)
                        .FirstOrDefaultAsync(w => w.Id == taskId.Value);

                    if (parentTask.ParentTaskId != childTask.ParentTaskId)
                    {
                        throw new ValidationException("Connection between this tasks is not allowed.");
                    }

                    bool isFormulaTaskRoot = childTask.FormulaId.HasValue &&
                        await _taskNeo4JRepository.IsRootAsync(childTask.Id);

                    await _taskNeo4JRepository.AddTaskConditionAsync(opt.Id, opt.Condition.Task.Id, taskId.Value);
                    await _formulaTaskJobService.UpdateFormulaTaskTime(trx, childTask);
                    await _startDateHelperService.UpdateStartDatesForTreeIfNeeded(trx, parentTask, childTask);

                    await _formulaTaskJobService.RemoveFormulaTaskJobs(trx, childTask, isFormulaTaskRoot);
                }
                else
                {
                    if (assignedTaskId.HasValue)
                    {
                        await _taskNeo4JRepository.RemoveTaskConditionAsync(opt.Condition.Task.Id,
                            assignedTaskId.Value);
                    }

                    await _formulaTaskJobService.TryResetProjectTaskTreeStatuses(opt.Condition.Task.Status, opt.Condition.Task.Id,
                        assignedTask.Id);
                    await _formulaTaskJobService.ScheduleTaskJobs(assignedTask, trx);
                }

                await trx.SaveAndCommitAsync();
                transaction.Commit();
            }
        }

        public async Task CompleteConditionalTask(int taskId, ConditionOptionDto model)
        {
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var task = await trx.Track<ProjectTask>()
                    .Include(c => c.RecurrenceOptions)
                    .Include(t => t.Project)
                    .Include(t => t.ProjectTaskUsers)
                    .Include(t => t.ProjectTaskVendors)
                    .Include(t => t.ProccessingUser)
                        .ThenInclude(t => t.Roles)
                            .ThenInclude(t => t.Role)
                    .Include(t => t.Condition)
                        .ThenInclude(t => t.Options)
                            .ThenInclude(t => t.AssignedTask)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                    throw new ValidationException("Task doesn't exist");

                if (task.Condition?.Options == null)
                    throw new ValidationException("Task condition option doesn't exist");

                if (task.ProjectTaskUsers.Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing)
                    && task.Status == TaskStatusType.NeedsReview)
                {
                    // Then review is moving task now.
                    // Reviewer cannot change selected condition option
                    var selectedConditionOption = task.Condition.Options.FirstOrDefault(o => o.IsSelected);
                    if (selectedConditionOption?.Id > 0)
                    {
                        model.ConditionOptionId = selectedConditionOption.Id;
                    }
                }

                await _taskNeo4JRepository.DeselectTaskConditionsAsync(task.Id);
                foreach (var opt in task.Condition.Options)
                {
                    if (opt.Id == model.ConditionOptionId)
                    {
                        opt.IsSelected = true;
                        if (opt.AssignedTaskId.HasValue)
                            await _taskNeo4JRepository.SetTaskConditionSelectedAsync(task.Id, opt.AssignedTaskId.Value);
                    }
                    else
                    {
                        opt.IsSelected = false;
                    }
                }
                await _taskScheduleService.ChangeTaskStatus(trx, task, model.Status, model.ConditionOptionId == 0 ? (int?)null : model.ConditionOptionId);
                if (model.Status == TaskStatusType.Completed && model.Rating.HasValue)
                    trx.Add(new FormulaTaskStatistic
                    {
                        Created = _dateTimeService.NowUtc,
                        FormulaTaskId = task.FormulaTaskId.Value,
                        ProjectTaskId = task.Id,
                        Type = StatisticType.Rating,
                        Value = model.Rating,
                        VendorGuid = task.ProccessingUserGuid.Value
                    });
                await trx.SaveAndCommitAsync();
                transaction.Commit();
            }
        }

        #endregion

        #region Task Dependency Service

        public async Task AddTaskDependency(TaskDependencyDto model)
        {
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var dependency = trx.Track<ProjectTaskDependency>()
                    .FirstOrDefault(p =>
                        p.ChildTaskId == model.ChildTaskId
                        && p.ParentTaskId == model.ParentTaskId);

                if (dependency == null)
                {
                    var parentTask = await trx.Track<ProjectTask>()
                        .Include(i => i.RecurrenceOptions)
                        .Include(i => i.ChildTasks)
                        .Include(i => i.ParentTasks)
                        .Include(i => i.AssignedConditionOptions)
                        .FirstAsync(w => w.Id == model.ParentTaskId);

                    var childTask = await trx.Track<ProjectTask>()
                        .Include(i => i.RecurrenceOptions)
                        .Include(i => i.ChildTasks)
                        .Include(i => i.ParentTasks)
                        .Include(i => i.AssignedConditionOptions)
                        .FirstAsync(w => w.Id == model.ChildTaskId);

                    if (parentTask.ParentTaskId != childTask.ParentTaskId)
                    {
                        throw new ValidationException("Connection between this tasks is not allowed.");
                    }

                    dependency = _mapper.Map<ProjectTaskDependency>(model);
                    await trx.AddAsync(dependency);
                    await trx.SaveChangesAsync();

                    var isFormulaTaskRoot =
                        childTask.FormulaId.HasValue
                        && await _taskNeo4JRepository.IsRootAsync(childTask.Id);

                    await _taskNeo4JRepository.AddTaskDependencyAsync(model.ParentTaskId, model.ChildTaskId);
                    await _formulaTaskJobService.UpdateFormulaTaskTime(trx, childTask);
                    await _formulaTaskJobService.RemoveFormulaTaskJobs(trx, childTask, isFormulaTaskRoot);
                    await _startDateHelperService.UpdateStartDatesForTreeIfNeeded(trx, parentTask, childTask);
                }

                await trx.SaveChangesAsync();
                await _scheduleService.RemoveJob(trx, model.ChildTaskId);
                await trx.SaveAndCommitAsync();
                transaction.Commit();
            }
        }

        public async Task RemoveTaskDependency(TaskDependencyDto model)
        {
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var dependency = trx.Track<ProjectTaskDependency>()
                    .Include(i => i.ChildTask)
                    .ThenInclude(i => i.RecurrenceOptions)
                    .Include(i => i.ParentTask)
                    .FirstOrDefault(p =>
                        p.ChildTaskId == model.ChildTaskId
                        && p.ParentTaskId == model.ParentTaskId);

                if (dependency == null)
                    throw new ValidationException("TaskDependency doesn't exist");

                trx.Remove(dependency);
                await trx.SaveChangesAsync();

                var task = dependency.ChildTask;
                await _taskNeo4JRepository.RemoveTaskDependencyAsync(model.ParentTaskId, model.ChildTaskId);

                await _formulaTaskJobService.TryResetProjectTaskTreeStatuses(dependency.ParentTask.Status, model.ParentTaskId,
                    model.ChildTaskId);
                await _formulaTaskJobService.ScheduleTaskJobs(task, trx);

                await trx.SaveAndCommitAsync();
                transaction.Commit();
            }
        }

        #endregion

        public async Task<TaskDto> UpdateTaskStatusAsync(int taskId, StatusDto model)
        {
            using (var trx = _repo.Transaction())
            {
                var task = await trx.Track<ProjectTask>()
                    .Include(c => c.Project)
                    .Include(t => t.ProjectTaskVendors)
                    .Include(t => t.ProjectTaskUsers)
                    .Include(c => c.RecurrenceOptions)
                    .Include(t => t.ProccessingUser)
                        .ThenInclude(t => t.Roles)
                            .ThenInclude(t => t.Role)
                    .Include(c => c.ChildTasks)
                    .Include(c => c.ParentTasks)
                        .ThenInclude(c => c.ParentTask)
                    .Include(f => f.FormulaTask)
                        .ThenInclude(f => f.FormulaProject)
                    .FirstOrDefaultAsync(x => x.Id == taskId);

                if (task == null)
                    throw new ValidationException("Task not found.");

                await _taskScheduleService.ChangeTaskStatus(trx, task, model.Status);
                await _taskSocketStore.TaskStatusChanged(task.ProjectId, taskId, model.Status);
                //Added Below Line of code to trigger mail for the  Immediate child of parent task for vendors.WRT //Production Bug Not Sending Email To Child Task For Task Invitation On 12 May2021
                         if (task.Status == TaskStatusType.Completed)
                {
                    var ChildProjectTaskId = trx.Track<ProjectTaskDependency>().Where(a => a.ParentTaskId == task.Id).Select(a => a.ChildTaskId).FirstOrDefault();
                    var childProjectTaskDetails = trx.Track<ProjectTaskVendor>().Where(a => a.ProjectTaskId == ChildProjectTaskId).Select(a => a).FirstOrDefault();
                                    if (childProjectTaskDetails != null)
                        await _notificationService.SendProjectTaskOutsourcesAsync(trx, childProjectTaskDetails.Id);
                                }
                if (model.Status == TaskStatusType.Completed && model.Rating.HasValue)
                    trx.Add(new FormulaTaskStatistic
                    {
                        Created = _dateTimeService.NowUtc,
                        FormulaTaskId = task.FormulaTaskId.Value,
                        ProjectTaskId = task.Id,
                        Type = StatisticType.Rating,
                        Value = model.Rating,
                        VendorGuid = task.ProccessingUserGuid.Value
                    });
                await trx.SaveAndCommitAsync();
                return _mapper.Map<TaskDto>(task);
            }
        }

        public async Task<bool> PublishTaskResourceAsync(int taskId, PublishTaskResourceDto model)
        {
            var task = await _repo.Read<ProjectTask>()
                     .Include(c => c.Project)
                     .Include(t => t.ProjectTaskVendors)
                     .Include(t => t.ProjectTaskUsers)
                     .Include(c => c.RecurrenceOptions)
                     .Include(t => t.ProccessingUser)
                         .ThenInclude(t => t.Roles)
                             .ThenInclude(t => t.Role)
                     .Include(c => c.ChildTasks)
                     .Include(c => c.ParentTasks)
                     .FirstOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
                throw new ValidationException("Task not found.");

            await _taskNeo4JRepository.PublishTaskResourceAsync(new PublishTaskResourceNeo4jDto
            {
                Id = model.Id,
                IsPublished = model.IsPublished
            });
            return model.IsPublished;
        }

        public async Task<TaskDto> UpdateTaskNotificationAsync(int taskId, NotificationDto model)
        {
            using (var trx = _repo.Transaction())
            {
                var task = await trx.Track<ProjectTask>()
                    .Include(c => c.Project)
                    .Include(t => t.ProjectTaskVendors)
                    .Include(t => t.ProjectTaskUsers)
                    .Include(c => c.RecurrenceOptions)
                    .Include(t => t.ProccessingUser)
                        .ThenInclude(t => t.Roles)
                            .ThenInclude(t => t.Role)
                    .Include(c => c.ChildTasks)
                    .Include(c => c.ParentTasks)
                    .FirstOrDefaultAsync(x => x.Id == taskId);

                if (task == null)
                    throw new ValidationException("Task not found.");

                if (model != null)
                    task.DescNotificationFlag = model.IsRead;
                await trx.SaveAndCommitAsync();
                return _mapper.Map<TaskDto>(task);
            }
        }

        public async Task UpdateTasksPositionAsync(List<TaskPositionDto> list)
        {
            if (list == null || !list.Any())
                throw new ValidationException("Incorrect TaskPositionDto list");

            using (var trx = _repo.Transaction())
            {
                var taskIds = list.Select(x => x.Id);
                var tasks = trx.Track<ProjectTask>().Where(c => taskIds.Contains(c.Id));
                foreach (var task in tasks)
                {
                    var model = list.FirstOrDefault(c => c.Id == task.Id);
                    if (model == null)
                        continue;
                    task.PosX = model.PosX;
                    task.PosY = model.PosY;
                }

                if (tasks.Any())
                    await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task<TaskDto> DoTask(int taskId)
        {
            var userId = _principal.GetUserId();

            using (var trx = _repo.Transaction())
            {
                var task = await trx.Track<ProjectTask>()
                    .Include(t => t.ProjectTaskUsers)
                    .Include(t => t.ProjectTaskVendors)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                    throw new ForbiddenException($"Task with {taskId} id doesn't exist.");

                if (!task.ProjectTaskUsers.Any(t => t.UserId == userId
                                                    && t.ProjectTaskUserType == ProjectTaskUserType.Assigned))
                    throw new ForbiddenException("You dont assigned to the task.");

                if (task.Status != TaskStatusType.InProgress)
                    throw new ValidationException("The task cannot be processed.");

                task.ProccessingUserGuid = userId;

                foreach (var notification in task.ProjectTaskVendors)
                {
                    if (notification.Status == ProjectRequestStatus.Send)
                        notification.Status = ProjectRequestStatus.AcceptedByOther;
                }
                await _taskHistoryService.Write(task.Id, ActivityType.Processing, trx: trx);
                await _notificationService.SendStartProjectTaskAsync(trx, taskId);
                await trx.SaveAndCommitAsync();
            }
            return await GetTaskAsync(taskId);
        }

        public async Task<TaskDto> ReviewTask(int taskId)
        {
            var userId = _principal.GetUserId();
            var task = await _repo.Track<ProjectTask>()
                .Include(t => t.ProjectTaskUsers)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new ForbiddenException($"Task with {taskId} id doesn't exist.");

            if (task.Status != TaskStatusType.NeedsReview)
                throw new ValidationException("The task cannot be reviewed.");

            if (!task.ProjectTaskUsers.Any(t => t.UserId == userId
                                                && t.ProjectTaskUserType == ProjectTaskUserType.Reviewing))
                throw new ValidationException("You dont assigned to the task.");

            task.ReviewingUserGuid = userId;

            await _taskHistoryService.Write(task.Id, ActivityType.Reviewing, saveChanges: true);
            await _repo.SaveChangesAsync();
            return await GetTaskAsync(taskId);
        }

        public async Task<TaskDto> DoVendorTask(int taskId)
        {
            var userId = _principal.GetUserId();
            using (var trx = _repo.Transaction())
            {
                var vendorNotification = await trx.Track<ProjectTaskVendor>()
                    .Include(t => t.ProjectTask)
                        .ThenInclude(t => t.Project)
                    .Include(t => t.Vendor)
                    .FirstOrDefaultAsync(t => t.ProjectTaskId == taskId && t.Status == ProjectRequestStatus.Accepted);



                //28dec
                var ftsCheck = await trx.Track<FormulaTaskStatistic>()
                 .FirstOrDefaultAsync(t => t.FormulaTaskId == vendorNotification.ProjectTask.FormulaTaskId.Value
                 && t.ProjectTaskId == vendorNotification.ProjectTaskId && t.Type == StatisticType.Working
                 && t.VendorGuid == _principal.GetUserId());


                if (ftsCheck != null)
                {
                    throw new ValidationException("Already Exist");
                }



                var task = await _repo.Track<ProjectTask>()
                    .Include(t => t.ProjectTaskUsers)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                    throw new ForbiddenException($"Task with {taskId} id doesn't exist.");

                if (task.Status != TaskStatusType.InProgress)
                    throw new ValidationException("The task cannot be processed.");

                if (vendorNotification.VendorGuid != _principal.GetOwnerId())
                {
                    throw new ValidationException("This task is linked to another user");
                }

                task.ProccessingUserGuid = userId;

                await _repo.SaveChangesAsync();

                await trx.SaveChangesAsync();

                trx.Add(new CreditLog
                {
                    Amount = vendorNotification.Price,
                    AmountWithTax = await CalculateAmountWithTax(trx, vendorNotification.Price),
                    HistoryTime = _dateTimeService.NowUtc,
                    ManagerId = vendorNotification.ProjectTask.Project.OwnerGuid,
                    VendorId = _principal.GetUserId(),
                    ProjectTaskId = vendorNotification.ProjectTaskId,
                    Type = CreditsLogType.VendorAcceptRequest
                });

                trx.Add(new FormulaTaskStatistic
                {
                    Created = _dateTimeService.NowUtc,
                    FormulaTaskId = vendorNotification.ProjectTask.FormulaTaskId.Value,
                    ProjectTaskId = vendorNotification.ProjectTaskId,
                    Type = StatisticType.Working,
                    VendorGuid = _principal.GetUserId()
                });



                await _taskHistoryService.Write(vendorNotification.ProjectTask.Id,
                        ActivityType.UpdateProcessingUser,
                        saveChanges: true);
                await _notificationService.SendProjectTaskOutsourcesAcceptAsync(trx, vendorNotification.ProjectTaskId);

                await UpdateStatistic(trx, vendorNotification);

                await trx.SaveAndCommitAsync();
            }
            return await GetTaskAsync(taskId);
        }

        private async Task UpdateStatistic(ITransactionScope trx, ProjectTaskVendor vendorNotification)
        {
            var statistic = await trx.Track<FormulaTaskStatistic>()
                .Where(w => w.FormulaTaskId == vendorNotification.ProjectTask.FormulaTaskId &&
                    w.ProjectTaskId == vendorNotification.ProjectTaskId && w.Type == StatisticType.AcceptedButNotStarted &&
                    w.VendorGuid == vendorNotification.VendorGuid && !w.Completed.HasValue)
                .FirstOrDefaultAsync();

            if (statistic == null)
                return;

            statistic.Completed = _dateTimeService.NowUtc;
            statistic.Value = (short)(statistic.Completed.Value - statistic.Created).TotalMinutes;
        }

        public async Task<TaskDto> ChangeProcessingUser(ProcessingUserDto processingUserDto)
        {
            var user = await _repo.Track<User>()
                .Where(t => t.Id == processingUserDto.ProcessingUserId)
                .FirstOrDefaultAsync();

            var task = await _repo.Track<ProjectTask>()
                .Where(t => t.Id == processingUserDto.TaskId)
                .FirstOrDefaultAsync();

            if (task.Status != TaskStatusType.InProgress && task.Status != TaskStatusType.NeedsReview)
            {
                throw new ValidationException("Processing user cannot be changed.");
            }

            else if (task.Status == TaskStatusType.NeedsReview)
            {
                var checkTask = await _repo.Read<ProjectTaskUser>()
                .FirstOrDefaultAsync(w => w.UserId == user.Id &&
                    w.ProjectTaskUserType == ProjectTaskUserType.Reviewing && w.ProjectTaskId == processingUserDto.TaskId);

                if (checkTask == null)
                    throw new ForbiddenException("You dont assigned to the task.");

                task.ReviewingUserGuid = user.Id;
                await _taskHistoryService.Write(task.Id, ActivityType.Reviewing, saveChanges: true);
            }

            else
            {
                var checkTask = await _repo.Read<ProjectTaskUser>()
                .FirstOrDefaultAsync(w => w.UserId == user.Id &&
                    w.ProjectTaskUserType == ProjectTaskUserType.Assigned && w.ProjectTaskId == processingUserDto.TaskId);

                if (checkTask == null)
                    throw new ForbiddenException("You dont assigned to the task.");

                task.ProccessingUserGuid = user.Id;

                await _taskHistoryService.Write(task.Id, ActivityType.UpdateProcessingUser, saveChanges: true);
            }

            await _repo.SaveChangesAsync();
            return await GetTaskAsync(task.Id);
        }

        public async Task<TaskDto> ChangeNewProcessingUser(ProcessingUserDto processingUserDto)
        {
            var user = await _repo.Track<User>()
                .Where(t => t.Id == processingUserDto.ProcessingUserId)
                .FirstOrDefaultAsync();

            var task = await _repo.Track<ProjectTask>()
                .Where(t => t.Id == processingUserDto.TaskId)
                .FirstOrDefaultAsync();

            if (task.Status != TaskStatusType.InProgress && task.Status != TaskStatusType.NeedsReview)
            {
                throw new ValidationException("Processing user cannot be changed.");
            }

            else if (task.Status == TaskStatusType.NeedsReview)
            {
                task.ReviewingUserGuid = user.Id;
                await _taskHistoryService.Write(task.Id, ActivityType.Reviewing, saveChanges: true);
            }
            else
            {
                task.ProccessingUserGuid = user.Id;
                await _taskHistoryService.Write(task.Id, ActivityType.UpdateProcessingUser, saveChanges: true);
            }

            await _repo.SaveChangesAsync();
            return await GetTaskAsync(task.Id);
        }

        public async Task<List<ProjectNotesDto>> GetTaskNotesAsync(int? taskId)
        {
            //var result = new List<ProjectNotesDto>();

            //var projectId = _repo.Read<ProjectTask>()
            //    .Where(t => t.Id == taskId).FirstOrDefault().ProjectId;

            //var notesList = await _repo.Read<ProjectNote>()
            //    .Where(t => t.ProjectId == projectId)
            //    .ToListAsync();

            //if (notesList.Count() > 0)
            //{

            //    var taskNotes = notesList.Where(t => t.ProjectTaskId == taskId);
            //    int? formulaId = 0;

            //    bool notesAvailable = false;

            //    if (taskNotes.Count() > 0)
            //    {
            //        formulaId = taskNotes.FirstOrDefault().FormulaId;
            //        notesAvailable = true;
            //    }


            //    var projectNotes = notesList
            //    .Where(t => t.IsPublished == true && t.ProjectTaskId != taskId && t.ProjectId == projectId
            //    && (notesAvailable ? t.FormulaId != formulaId : 1 == 1)
            //    )
            //    .OrderBy(t => t.DateCreated)
            //    .ToList();

            //    if (taskNotes.Count() > 0)
            //    {
            //        var formulaNotes = notesList
            //              .Where(t => t.FormulaId == formulaId
            //              &&
            //             (formulaId == 0 ?
            //              t.ProjectTaskId == taskId : 1 == 1)
            //              )
            //              .GroupBy(x => x.Text)
            //              .Select(g => new ProjectNote
            //              {
            //                  Id = g.FirstOrDefault().Id,
            //                  Text = g.FirstOrDefault().Text,
            //                  IsPublished = g.FirstOrDefault().IsPublished,
            //                  FormulaId = g.FirstOrDefault().FormulaId,
            //                  ProjectId = projectId,
            //                  ProjectTaskId = taskId,
            //                  DateCreated = g.FirstOrDefault().DateCreated

            //              })
            //              .ToList();

            //        //var formulaNotes = notesList
            //        //        .Where(t => t.FormulaId == taskNotes.FirstOrDefault().FormulaId && t.ProjectTaskId == taskId)
            //        //        .OrderBy(t => t.DateCreated)
            //        //        .ToList();

            //        projectNotes = formulaNotes.Concat(projectNotes).ToList();
            //    }

            //    result = projectNotes.Select(Mapper.Map<ProjectNotesDto>).OrderByDescending(t => t.DateCreated).ToList();
            //}

            //return result;

            var projectTask = await _repo.Read<ProjectTask>().SingleOrDefaultAsync(t => t.Id == taskId);

            if (projectTask == null)
                throw new ValidationException("Project not found");

            var projectId = projectTask.ProjectId;


            var projectPublishedNotes = await _repo.Read<ProjectNote>()
               .Where(w => w.ProjectId == projectId && w.IsPublished)
               .ProjectTo<ProjectNotesDto>(_mapper.ConfigurationProvider)
               .ToListAsync();



            var projectTaskNotes = await _repo.Read<ProjectNote>()
                .Where(w => w.ProjectId == projectId && w.ProjectTaskId == taskId)
                .ProjectTo<ProjectNotesDto>(_mapper.ConfigurationProvider)
                .ToListAsync();


            foreach (var pNotes in projectPublishedNotes)
            {
                // Resolved notes duplicacy when published from same task.
                var noteRecord = projectTaskNotes.Where(x => x.ShareNoteParentID == pNotes.ProjectTaskId
                && x.Text == pNotes.Text).FirstOrDefault();
                projectTaskNotes.Remove(noteRecord);
            }

            var projectNotes = projectTaskNotes.Union(projectPublishedNotes, new ProjectNoteDtoComparer())
                .OrderByDescending(n => n.DateCreated)
                .ToList();

            var noteList = projectNotes;

            List<ProjectNotesDto> finalResult = new List<ProjectNotesDto>();
            finalResult = projectNotes;


            // Resolved notes duplicacy when published from other task.
            for (int i = 0; i < projectNotes.Count(); i++)
            {
                var recCount = projectNotes.Where(x => x.Text == projectNotes[i].Text && x.IsPublished != projectNotes[i].IsPublished).ToList();

                if (recCount.Count > 0)
                {
                    var rec = recCount.Where(x => x.IsPublished == false).FirstOrDefault();
                    finalResult.Remove(rec);
                }

            }

            return finalResult;
        }

        public async Task<ProjectNotesDto> AddTaskNotesAsync(AddProjectNotesDto model)
        {
            if (_principal.IsWorker())
                throw new ForbiddenException("You have no rigths for adding notes.");

            if (model == null)
                throw new ArgumentException(nameof(model));

            using (var trx = _repo.Transaction())
            {
                //var formula = await _repo.Read<ProjectNote>()
                //   .FirstOrDefaultAsync(x => x.ProjectTaskId == model.ProjectTaskId);

                var projectTask = await _repo.Read<ProjectTask>()
                    .Include(i => i.FormulaTask)
                        .ThenInclude(t => t.FormulaProject)
                    .SingleOrDefaultAsync(x => x.Id == model.ProjectTaskId);

                var note = _mapper.Map<ProjectNote>(model);

                note.FormulaId = projectTask?.FormulaTask?.FormulaProjectId ?? 0;
                note.DateCreated = _dateTimeService.NowUtc;

                await trx.AddAsync(note);
                await trx.SaveChangesAsync();

                await trx.SaveAndCommitAsync();

                var res = await _repo.Read<ProjectNote>()
                    .SingleOrDefaultAsync(x => x.Id == note.Id);

                return _mapper.Map<ProjectNotesDto>(res);
            }
        }

        public async Task ShareProjectNote(AddProjectNotesDto model, List<int> sharedIds)
        {
            if (_principal.IsWorker())
                throw new ForbiddenException("You have no rigths for adding notes.");

            using (var trx = _repo.Transaction())
            {
                foreach (var id in sharedIds)
                {
                    var projectTask = await _repo.Read<ProjectTask>()
                        .Include(i => i.FormulaTask)
                            .ThenInclude(t => t.FormulaProject)
                        .SingleOrDefaultAsync(x => x.Id == model.ProjectTaskId);

                    await trx.AddAsync(new ProjectNote
                    {
                        Text = model.Text,
                        FormulaId = projectTask?.FormulaTask?.FormulaProjectId ?? 0,
                        ProjectId = model.ProjectId,
                        ProjectTaskId = id,
                        DateCreated = _dateTimeService.NowUtc,
                        ShareNoteParentID = model.ProjectTaskId
                    });

                    await trx.SaveChangesAsync();
                }

                await trx.SaveAndCommitAsync();
            }
        }

        public async Task<ProjectNotesDto> UpdateTaskNotesAsync(int noteId, bool isPublished)
        {
            using (var trx = _repo.Transaction())
            {
                var note = await trx.Track<ProjectNote>()
                    .SingleOrDefaultAsync(x => x.Id == noteId);

                if (note == null)
                    throw new ValidationException("Note not found.");

                note.IsPublished = isPublished;

                await trx.SaveAndCommitAsync();

                var res = await _repo.Read<ProjectNote>()
                    .SingleOrDefaultAsync(x => x.Id == note.Id);

                return _mapper.Map<ProjectNotesDto>(res);
            }
        }

        public async Task DeleteTaskNotesAsync(int noteId)
        {
            using (var trx = _repo.Transaction())
            {
                var note = await trx.Track<ProjectNote>()
                    .SingleOrDefaultAsync(x => x.Id == noteId);

                if (note == null)
                    throw new ValidationException("Note not found.");

                var notesListToRemove = await trx.Track<ProjectNote>()
                    .Where(x => x.Text == note.Text && x.ProjectId == note.ProjectId).ToListAsync();

                trx.RemoveRange(notesListToRemove);
                await trx.SaveAndCommitAsync();
            }
        }

        public async Task UpdateExpiredJobInvitesToOwner(int taskId)
        {
            using (var trx = _repo.Transaction())
            {
                var vendorNotificationIds = _repo.Read<ProjectTaskVendor>()
                    .Where(w => w.ProjectTaskId == taskId && w.Status == ProjectRequestStatus.Send)
                    .Select(s => s.Id)
                    .ToList();

                var formulaTaskStatistics = _repo.Read<FormulaTaskStatistic>()
                    .Where(w => w.ProjectTaskId == taskId && w.Type == StatisticType.Responding)
                    .ToList();

                if (vendorNotificationIds.Count == 0)
                {
                    throw new ValidationException("Notification doesn't exist");
                }

                var now = _dateTimeService.NowUtc;
                int requestId = vendorNotificationIds.First();

                foreach (var id in vendorNotificationIds)
                {
                    var notification = trx.Track<ProjectTaskVendor>()
                        .SingleOrDefault(x => x.Id == id);

                    notification.Status = ProjectRequestStatus.Lost;
                    notification.LastModified = now;

                    await _notificationService.SendExpiredJobInvitesNotificationToVendor(trx, notification.Id);

                    await trx.SaveChangesAsync();
                }

                foreach (var item in formulaTaskStatistics)
                {
                    trx.Add(new FormulaTaskStatistic
                    {
                        Created = item?.Created ?? now,
                        Completed = now,
                        FormulaTaskId = item.FormulaTaskId,
                        FormulaTaskStatisticId = null,
                        ProjectTaskId = item.ProjectTaskId,
                        Type = StatisticType.Lost,
                        Value = null,
                        VendorGuid = item.VendorGuid
                    });
                }

                await _notificationService.SendExpiredJobInvitesNotificationToOwner(trx, requestId);

                await trx.SaveAndCommitAsync();
            }
        }

        private async Task<decimal> CalculateAmountWithTax(ITransactionScope trx, decimal amount)
        {
            var vendorTax = await trx.Read<CreditsTax>().FirstOrDefaultAsync(t => t.Type == Global.Enums.CreditsTaxType.Vendor);
            double amountWithTax = (double)amount - vendorTax.Fee - (vendorTax.Percentage / 100 * (double)amount);
            return (decimal)amountWithTax;
        }

        private async Task DeleteCheckListAsync(ITransactionScope trx, int taskId)
        {
            var todos = await trx.Track<ProjectTaskChecklist>()
                .Where(t => t.ProjectTaskId == taskId)
                .ToListAsync();

            if (todos.Any())
            {
                trx.RemoveRange(todos);
                await trx.SaveChangesAsync();
            }
        }

        public async Task<IList<VendorJobInvitesDto>> GetVendorJobInvites(Guid UserId)
        {
            var vendorJobInvites = await _repo.ExecuteSql<VendorJobInvitesDto>(_mapper, "[dbo].[uspGetPendingJobInvitesForVendor] @VendorGuid",
          new List<SqlParameter> { new SqlParameter { ParameterName = "@VendorGuid", SqlDbType = SqlDbType.UniqueIdentifier, Value = UserId } }).ToListAsync();
            return vendorJobInvites;
        }

        public async Task UpdateTaskChecklist(List<UpdateTaskChecklistDto> todos)
        {
            if (todos != null && todos.Any())
            {
                using (var trx = _repo.Transaction())
                {
                    var todoIds = todos.Select(x => x.Id);
                    var taskChecklists = trx.Track<ProjectTaskChecklist>().Where(c => todoIds.Contains(c.Id));

                    foreach (var taskChecklist in taskChecklists)
                    {
                        var model = todos.FirstOrDefault(c => c.Id == taskChecklist.Id);
                        if (model == null)
                            continue;

                        taskChecklist.TodoIsChecked = model.TodoIsChecked.HasValue
                            ? model.TodoIsChecked.Value
                            : taskChecklist.TodoIsChecked;

                        taskChecklist.ReviewerIsChecked = model.ReviewerIsChecked.HasValue
                            ? model.ReviewerIsChecked.Value
                            : taskChecklist.ReviewerIsChecked;
                    }

                    if (taskChecklists.Any())
                        await trx.SaveAndCommitAsync(CancellationToken.None);
                }
            }
        }

        public async Task<IList<int>> GetDownstreamShareIds(int taskId)
        {
            var result = await _repo.ExecuteSql<SharedProjectTaskDto>(
                    _mapper,
                    "[dbo].[usp_ShareNotesDownstream] @inputprojecttaskid",
                    new List<SqlParameter> { new SqlParameter { ParameterName = "@inputprojecttaskid", SqlDbType = SqlDbType.Int, Value = taskId } })
                .ToListAsync();

            if (result.Count > 0)
            {
                var list = result.Select(e => e.TaskId).ToList();
                return list;
                //return result.Select(e => e.TaskId).ToList();
            }





            return new List<int>();
        }

        public async Task<IList<int>> GetFormulaShareIds(int taskId)
        {
            var result = await _repo.ExecuteSql<SharedProjectTaskDto>(
                    _mapper,
                    "[dbo].[usp_ShareNotesInFormula] @inputprojecttaskid",
                    new List<SqlParameter> { new SqlParameter { ParameterName = "@inputprojecttaskid", SqlDbType = SqlDbType.Int, Value = taskId } })
                .ToListAsync();

            if (result.Count > 0)
                return result.Select(e => e.TaskId).ToList();

            return new List<int>();
        }
    }
}

