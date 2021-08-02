using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.Resources.Dto;
using IAutoM8.Service.Vendor.Interfaces;
using Microsoft.EntityFrameworkCore;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Domain.Models;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.Vendor.Dto;
using System.Data.SqlClient;
using System.Data;
using IAutoM8.Global;
using IAutoM8.Domain.Models.Formula.Task;
using System.Threading;
using NotificationEntity = IAutoM8.Domain.Models.Notification;
using IAutoM8.Domain.Models.Project.Task;

namespace IAutoM8.Service.Vendor
{
    public class VendorService : IVendorService
    {
        private readonly IRepo _repo;
        private readonly IMapper _mapper;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IFormulaTaskNeo4jRepository _formulaTaskNeo4JRepository;
        private readonly IStorageService _storageService;
        private readonly ClaimsPrincipal _principal;
        private readonly ITaskHistoryService _taskHistoryService;
        private readonly INotificationService _notificationService;

        public VendorService(IRepo repo,
            IMapper mapper,
            ITaskNeo4jRepository taskNeo4JRepository,
            IFormulaTaskNeo4jRepository formulaTaskNeo4JRepository,
            IDateTimeService dateTimeService,
            ITaskHistoryService taskHistoryService,
            ClaimsPrincipal principal,
            IStorageService storageService,
            INotificationService notificationService)
        {
            _repo = repo;
            _mapper = mapper;
            _taskNeo4JRepository = taskNeo4JRepository;
            _dateTimeService = dateTimeService;
            _storageService = storageService;
            _formulaTaskNeo4JRepository = formulaTaskNeo4JRepository;
            _taskHistoryService = taskHistoryService;
            _principal = principal;
            _notificationService = notificationService;
        }

        public async Task<FormulaTaskVendorDto> GetFormulaTaskVendorNotification(int notificationId)
        {
            var formulaTaskVendor = await _repo.Read<FormulaTaskVendor>()
                    .Include(i => i.Vendor.Roles)
                    .Include(i => i.FormulaTask)
                    .FirstOrDefaultAsync(w => w.Id == notificationId && w.VendorGuid == _principal.GetUserId());

            var roleName = UserRoles.RoleName(formulaTaskVendor.Vendor.Roles.FirstOrDefault().RoleId);

            if (formulaTaskVendor == null)
            {
                throw new ValidationException("Notification doesn't exist");
            }

            if (formulaTaskVendor.Status != FormulaRequestStatus.None)
            {
                throw new ValidationException("Answer already committed for this notification");
            }

            var resources = _mapper.Map<List<ResourceDto>>(
                await _formulaTaskNeo4JRepository.GetTaskAndSharedResourcesAsync(formulaTaskVendor.FormulaTask.Id, formulaTaskVendor.FormulaTask.FormulaProjectId),
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.ProjectTask)));
                }
            );

            var projectResources = await _repo.Read<ResourceFormula>()
                .Include(c => c.Resource)
                .Where(c => c.FormulaId == formulaTaskVendor.FormulaTask.FormulaProjectId)
                .Select(s => s.Resource)
                .ToListAsync();
            var mappedProjectResources = _mapper.Map<List<ResourceDto>>(projectResources,
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.Formula)));
                    opts.Items.Add("isShared", true);
                });

            resources.AddRange(mappedProjectResources);

            if (roleName == "Company")
            {
                return new FormulaTaskVendorDto
                {
                    NotificationId = formulaTaskVendor.Id,
                    Description = formulaTaskVendor.FormulaTask.Description,
                    Duration = formulaTaskVendor.FormulaTask.Duration,
                    Title = formulaTaskVendor.FormulaTask.Title,
                    Answer = Global.Enums.FormulaRequestStatus.None,
                    Price = formulaTaskVendor.Price,
                    Resources = resources,
                    Role = roleName
                };
            }
            else
            {
                return new FormulaTaskVendorDto
                {
                    NotificationId = formulaTaskVendor.Id,
                    Description = formulaTaskVendor.FormulaTask.Description,
                    Duration = formulaTaskVendor.FormulaTask.Duration,
                    Title = formulaTaskVendor.FormulaTask.Title,
                    Answer = Global.Enums.FormulaRequestStatus.None,
                    Price = 0,
                    Resources = resources,
                    Role = roleName
                };
            }
        }

        public async Task<ProjectTaskVendorDto> GetProjectTaskVendorNotification(int notificationId)
        {
            var projectTaskVendor = await _repo.Read<ProjectTaskVendor>()
                    .Include(i => i.Vendor)
                    .Include(i => i.ProjectTask)
                        .ThenInclude(t => t.RecurrenceOptions)
                    .FirstOrDefaultAsync(w => w.Id == notificationId && w.VendorGuid == _principal.GetUserId());

            if (projectTaskVendor == null)
            {
                throw new ValidationException("Notification doesn't exist");
            }

            if (projectTaskVendor.Status != ProjectRequestStatus.Send)
            {
                throw new ValidationException("Answer already committed for this notification");
            }

            var recources = _mapper.Map<List<ResourceDto>>(
                await _taskNeo4JRepository.GetTaskAndSharedResourcesAsync(projectTaskVendor.ProjectTask.Id, projectTaskVendor.ProjectTask.ProjectId),
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.ProjectTask)));
                }
            );

            var projectResources = await _repo.Read<ResourceProject>()
                .Include(c => c.Resource)
                .Where(c => c.ProjectId == projectTaskVendor.ProjectTask.ProjectId)
                .Select(s => s.Resource)
                .ToListAsync();
            var mappedProjectResources = _mapper.Map<List<ResourceDto>>(projectResources,
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.Project)));
                    opts.Items.Add("isShared", true);
                });
            recources.AddRange(mappedProjectResources);

            return new ProjectTaskVendorDto
            {
                NotificationId = projectTaskVendor.Id,
                Description = projectTaskVendor.ProjectTask.Description,
                Duration = projectTaskVendor.ProjectTask.Duration,
                Price = projectTaskVendor.Price,
                Title = projectTaskVendor.ProjectTask.Title,
                Answer = ProjectRequestStatus.Send,
                IsRecurrent = projectTaskVendor.ProjectTask.RecurrenceOptionsId != null,
                IsAutomated = projectTaskVendor.ProjectTask.IsAutomated,
                IsConditional = projectTaskVendor.ProjectTask.Condition != null,
                RecurrenceType = projectTaskVendor.ProjectTask.RecurrenceOptions?.RecurrenceType,
                RecurrenceOptions = projectTaskVendor.ProjectTask.RecurrenceOptions,
                Resources = recources
            };
        }

        public async Task<FormulaTaskVendorDto> GetFormulaTaskCertification(Guid userId, int formulaTaskId)
        {
            var formulaTask = await _repo.Read<FormulaTask>()
                .SingleOrDefaultAsync(w => w.Id == formulaTaskId);

            if (formulaTask == null)
            {
                throw new ValidationException("Formula doesn't exist");
            }

            var resources = _mapper.Map<List<ResourceDto>>(
                await _formulaTaskNeo4JRepository.GetTaskAndSharedResourcesAsync(formulaTask.Id, formulaTask.FormulaProjectId),
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.ProjectTask)));
                }
            );

            var projectResources = await _repo.Read<ResourceFormula>()
                .Include(c => c.Resource)
                .Where(c => c.FormulaId == formulaTask.FormulaProjectId)
                .Select(s => s.Resource)
                .ToListAsync();
            var mappedProjectResources = _mapper.Map<List<ResourceDto>>(projectResources,
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.Formula)));
                    opts.Items.Add("isShared", true);
                });

            resources.AddRange(mappedProjectResources);

            return new FormulaTaskVendorDto
            {
                NotificationId = formulaTask.Id,
                Description = formulaTask.Description,
                Duration = formulaTask.Duration,
                Title = formulaTask.Title,
                Answer = FormulaRequestStatus.None,
                Price = 0,
                Resources = resources
            };
        }

        public async Task<IList<VendorPerformanceDto>> GetPerformanceData(Guid userId)
        {
            var responding = StatisticType.Responding;
            var working = StatisticType.Working;
            var rating = StatisticType.Rating;

            //var sqlForVendorPerformance = @"[dbo].[uspGetVendorPerformanceData] @userId, @role, @rating, @working, @responding"; // added logic for new role CompanyWorker WRT Sprint 10B

            var sqlForVendorPerformance = @"[dbo].[uspGetVendorPerformanceData] @userId, @rating, @working, @responding";

            return await _repo.ExecuteSql<VendorPerformanceDto>(_mapper, sqlForVendorPerformance,
                new List<SqlParameter> {
                    new SqlParameter{ ParameterName = "@rating", SqlDbType = SqlDbType.TinyInt, Value = rating },
                    new SqlParameter{ ParameterName = "@working", SqlDbType = SqlDbType.TinyInt, Value = working },
                    new SqlParameter{ ParameterName = "@responding", SqlDbType = SqlDbType.TinyInt, Value = responding },
                    new SqlParameter{ ParameterName = "@userId", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId },
                    //new SqlParameter{ ParameterName = "@role", SqlDbType = SqlDbType.VarChar, Value = UserRoles.Vendor } // added logic for new role CompanyWorker WRT Sprint 10B
                }).ToListAsync();
        }

        public async Task<IList<VendorUpSkillDto>> GetVendorUpSkills(Guid userId)
        {
            var sqlForVendorUpSkill = @"
IF OBJECT_ID('TEMPDB..#T1') IS NOT NULL
BEGIN
DROP TABLE #T1
END

	SELECT DISTINCT FT.Id AS FormulaTaskId, FP.Id AS FormulaId, anu.Id AS VendorId, S.Id AS SkillId, FP.[Name] AS FormulaName, S.[Name] AS SkillName, T.[Name] AS TeamName
	INTO #T1
	FROM AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.[Name] IN ('VENDOR','COMPANYWORKER') -- and anr.[Name]='VENDOR' --@role -- made changes in sql logic to adjust new roles.
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN FormulaTaskVendor FTV on FTV.VendorGuid=anu.Id 
	INNER JOIN FormulaTask FT ON FTV.FormulaTaskId = FT.Id
	INNER JOIN FormulaProject FP ON FT.FormulaProjectId = FP.Id
	LEFT JOIN SKILL S on FT.AssignedSkillId = s.Id
	LEFT JOIN TEAM T ON FT.TeamId = T.Id
	where anu.id = @userId

IF OBJECT_ID('TEMPDB..#T2') IS NOT NULL
BEGIN
DROP TABLE #T2
END
	SELECT DISTINCT FT.Id AS FormulaTaskId, FP.Id AS FormulaId, anu.Id AS VendorId, S.Id AS SkillId, FP.[Name] AS FormulaName, S.[Name] AS SkillName, T.[Name] AS TeamName
	INTO #T2
	FROM AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.[Name] IN ('VENDOR','COMPANYWORKER') -- and anr.[Name]='VENDOR' --@role -- made changes in sql logic to adjust new roles.
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN FormulaTaskVendor FTV on FTV.VendorGuid=anu.Id 
	INNER JOIN FormulaTask FT ON FTV.FormulaTaskId = FT.Id
	INNER JOIN FormulaProject FP ON FT.FormulaProjectId = FP.Id
	LEFT JOIN SKILL S on FT.AssignedSkillId = s.Id and S.IsGlobal = 1 AND S.Status = 5 --NEW CHECK ADDED ON REQUEST TO ONLY GET GLOBAL SKILLS
	LEFT JOIN TEAM T ON FT.TeamId = T.Id AND T.IsGlobal =1 AND T.Status = 5			--NEW CHECK ADDED ON REQUEST TO ONLY GET GLOBAL TEAMS
	where anu.id <>  @userId

SELECT DISTINCT #T2.FormulaId, #T2.TeamName, #T1.SkillName, #T2.FormulaName, #T2.FormulaTaskId
FROM #T1
INNER JOIN #T2 ON #T1.SkillId = #T2.SkillId
WHERE #T2.FormulaId NOT IN (SELECT FormulaId FROM #T1)
AND #T1.VendorId <> #T2.VendorId";

            return await _repo.ExecuteSql<VendorUpSkillDto>(_mapper, sqlForVendorUpSkill,
                new List<SqlParameter> {
                    new SqlParameter{ ParameterName = "@userId", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId },
                    //new SqlParameter{ ParameterName = "@role", SqlDbType = SqlDbType.VarChar, Value = UserRoles.Vendor } // added logic for new role CompanyWorker WRT Sprint 10B
                }).ToListAsync();
        }

        public async Task<IList<SelectedVendorsByTaskDto>> GetSelectedVendorsByFormulaId(int formulaId)
        {
            IList<SelectedVendorsByTaskDto> selectedVendors = new List<SelectedVendorsByTaskDto>();
            var uspSelectedVendors = @"[dbo].[USP_AutoSelectOutsourcer] @FORMULAID, @OPTIONTYPE";

            var result = await _repo.ExecuteSql<VendorRankByRatingDto>(_mapper, uspSelectedVendors,
                new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@FORMULAID", SqlDbType = SqlDbType.Int, Value = formulaId },
                    new SqlParameter { ParameterName = "@OPTIONTYPE", SqlDbType = SqlDbType.VarChar, Value = "TOPRATED" }
                })
                .ToListAsync();

            if (result != null && result.Count > 0)
            {
                selectedVendors = result
                    .GroupBy(g => g.FormulaTaskId)
                    .Select(v => new SelectedVendorsByTaskDto
                    {
                        FormulaTaskId = v.Key,
                        OutsourcerIds = v.Select(s => s.VendorGuid).ToList()
                    })
                    .ToList();
            }

            return selectedVendors;
        }

        public async Task<IList<Guid>> GetSelectedVendorsByTaskId(int formulaId, int formulaTaskId, string optionType)
        {
            IList<Guid> selectedVendors = new List<Guid>();
            var uspSelectedVendors = @"[dbo].[USP_AutoSelectOutsourcer] @FORMULAID, @OPTIONTYPE";

            var result = await _repo.ExecuteSql<VendorRankByRatingDto>(_mapper, uspSelectedVendors,
                new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@FORMULAID", SqlDbType = SqlDbType.Int, Value = formulaId },
                    new SqlParameter { ParameterName = "@OPTIONTYPE", SqlDbType = SqlDbType.VarChar, Value = optionType.ToUpper()}
                })
                .ToListAsync();

            if (result != null && result.Count > 0)
            {
                selectedVendors = result
                    .Where(w => w.FormulaTaskId == formulaTaskId)
                    .Select(v => v.VendorGuid)
                    .ToList();
            }

            return selectedVendors;
        }

        public async Task UpdateProjectTaskVendorNotificationStatus(ProjectTaskVendorDto vendorNotificationDto)
        {
            using (var trx = _repo.Transaction())
            {
                var vendorNotification = await trx.Track<ProjectTaskVendor>()
                    .Include(t => t.ProjectTask)
                        .ThenInclude(t => t.Project)
                    .Include(t => t.Vendor)
                    .FirstOrDefaultAsync(t => t.Id == vendorNotificationDto.NotificationId);

                if (vendorNotification == null)
                {
                    throw new ValidationException("Notification doesn't exist");
                }


                //28dec
                var ftsCheck = await trx.Track<FormulaTaskStatistic>()
                    .FirstOrDefaultAsync(t => t.FormulaTaskId == vendorNotification.ProjectTask.FormulaTaskId.Value
                    && t.ProjectTaskId == vendorNotification.ProjectTaskId && t.Type == StatisticType.AcceptedButNotStarted
                    && t.VendorGuid == _principal.GetUserId());


                if (ftsCheck != null)
                {
                    throw new ValidationException("Already Exist");
                }

                if (vendorNotification.Status != ProjectRequestStatus.Send)
                {
                    await UpdateStatistic(trx, vendorNotification);
                    trx.SaveAndCommit();
                    throw new ValidationException("Answer already committed for this notification");
                }

                if (vendorNotification.VendorGuid != _principal.GetOwnerId())
                {
                    throw new ValidationException("This notification is linked to another vendor");
                }

                vendorNotification.Status = vendorNotificationDto.Answer;
                if (vendorNotificationDto.Answer == ProjectRequestStatus.Accepted)
                {
                    //vendorNotification.ProjectTask.ProccessingUserGuid = vendorNotification.Vendor.Id;
                    //await _taskHistoryService.Write(vendorNotification.ProjectTask.Id,
                    //    ActivityType.UpdateProcessingUser,
                    //    saveChanges: true);

                    //trx.Add(new CreditLog
                    //{
                    //    //Amount = vendorNotificationDto.Price,
                    //    //AmountWithTax = await CalculateAmountWithTax(trx, vendorNotificationDto.Price),
                    //    Amount = vendorNotification.Price,
                    //    AmountWithTax = await CalculateAmountWithTax(trx, vendorNotification.Price),
                    //    HistoryTime = _dateTimeService.NowUtc,
                    //    ManagerId = vendorNotification.ProjectTask.Project.OwnerGuid,
                    //    VendorId = _principal.GetUserId(),
                    //    ProjectTaskId = vendorNotification.ProjectTaskId,
                    //    Type = CreditsLogType.VendorAcceptRequest
                    //});

                    // Get the list of other vendors, to whom the invite was sent
                    var notificationToDeclineByOwner = trx.Track<ProjectTaskVendor>()
                        .Include(t => t.ProjectTask)
                        .Include(t => t.Vendor)
                        .Where(t => t.ProjectTaskId == vendorNotification.ProjectTaskId
                            && t.Id != vendorNotification.Id
                            && t.Status == ProjectRequestStatus.Send);

                    var requestSentToOtherVendors = notificationToDeclineByOwner
                        .Select(x => x.VendorGuid)
                        .ToList();

                    foreach (var notification in notificationToDeclineByOwner)
                    {
                        if (notification.Status == ProjectRequestStatus.Send)
                            await _notificationService.SendProjectTaskOutsourcesUnavailableAsync(trx, notification.Id);

                        notification.Status = ProjectRequestStatus.AcceptedByOther;
                    }

                    // Dated: September 01, 2020
                    // Changing the code logic after discussion with my manager
                    //if (vendorNotification.ProjectTask.Status == TaskStatusType.InProgress)
                    //{
                    //    trx.Add(new FormulaTaskStatistic
                    //    {
                    //        Created = _dateTimeService.NowUtc,
                    //        FormulaTaskId = vendorNotification.ProjectTask.FormulaTaskId.Value,
                    //        ProjectTaskId = vendorNotification.ProjectTaskId,
                    //        Type = StatisticType.Working,
                    //        VendorGuid = _principal.GetUserId()
                    //    });

                    //    var otherlostVendorsList = trx.Track<FormulaTaskStatistic>()
                    //        .Where(x => x.ProjectTaskId == vendorNotification.ProjectTaskId && x.VendorGuid != _principal.GetUserId()).ToList();

                    //    if (otherlostVendorsList.Count > 0)
                    //    {
                    //        foreach (var vendor in otherlostVendorsList)
                    //        {
                    //            trx.Add(new FormulaTaskStatistic
                    //            {
                    //                Created = vendor.Created,
                    //                Completed = _dateTimeService.NowUtc,
                    //                FormulaTaskId = vendor.FormulaTaskId,
                    //                ProjectTaskId = vendor.ProjectTaskId,
                    //                Type = StatisticType.Lost,
                    //                VendorGuid = vendor.VendorGuid
                    //            });
                    //        }
                    //    }
                    //}

                    if (vendorNotification.ProjectTask.Status == TaskStatusType.InProgress)
                    {
                        // The vendor who accepted the request
                        trx.Add(new FormulaTaskStatistic
                        {
                            Created = _dateTimeService.NowUtc,
                            FormulaTaskId = vendorNotification.ProjectTask.FormulaTaskId.Value,
                            ProjectTaskId = vendorNotification.ProjectTaskId,
                            Type = StatisticType.AcceptedButNotStarted,
                            VendorGuid = _principal.GetUserId()
                        });
                    }

                    var otherlostVendorsList = trx.Track<FormulaTaskStatistic>()
                        .Where(x => x.ProjectTaskId == vendorNotification.ProjectTaskId
                            && x.VendorGuid != _principal.GetUserId())
                        .Where(x => x.Type == StatisticType.Responding)
                        .Where(x => requestSentToOtherVendors.Contains(x.VendorGuid))
                        .ToList();

                    if (otherlostVendorsList.Count > 0)
                    {
                        foreach (var vendor in otherlostVendorsList)
                        {
                            trx.Add(new FormulaTaskStatistic
                            {
                                Created = vendor.Created,
                                Completed = _dateTimeService.NowUtc,
                                FormulaTaskId = vendor.FormulaTaskId,
                                ProjectTaskId = vendor.ProjectTaskId,
                                Type = StatisticType.Lost,
                                VendorGuid = vendor.VendorGuid
                            });
                        }
                    }

                    await _notificationService.SendProjectTaskOutsourcesAcceptAsync(trx, vendorNotification.ProjectTaskId);
                }
                await UpdateStatistic(trx, vendorNotification);
                trx.SaveAndCommit();
            }
        }

        public async Task UpdateFormulaTaskVendorCertificationStatus(FormulaTaskVendorDto vendorNotificationDto, Guid userId)
        {
            using (var trx = _repo.Transaction())
            {
                var formulaTaskVendor = new FormulaTaskVendor
                {
                    FormulaTaskId = vendorNotificationDto.NotificationId,
                    VendorGuid = userId,
                    Price = 0.0m,
                    Status = FormulaRequestStatus.None,
                    Created = _dateTimeService.NowUtc,
                };

                await trx.AddAsync(formulaTaskVendor);
                await trx.SaveAndCommitAsync(CancellationToken.None);

                var id = formulaTaskVendor.Id;

                vendorNotificationDto.NotificationId = id;

                await UpdateFormulaTaskVendorNotificationStatus(vendorNotificationDto);
            }
        }

        private async Task UpdateStatistic(ITransactionScope trx, ProjectTaskVendor vendorNotification)
        {
            var statistic = await trx.Track<FormulaTaskStatistic>()
                .Where(w => w.FormulaTaskId == vendorNotification.ProjectTask.FormulaTaskId &&
                    w.ProjectTaskId == vendorNotification.ProjectTaskId && w.Type == StatisticType.Responding &&
                    w.VendorGuid == vendorNotification.VendorGuid && !w.Completed.HasValue)
                .FirstOrDefaultAsync();

            if (statistic == null)
                return;

            statistic.Completed = _dateTimeService.NowUtc;
            statistic.Value = (short)(statistic.Completed.Value - statistic.Created).TotalMinutes;
        }

        public async Task UpdateFormulaTaskVendorNotificationStatus(FormulaTaskVendorDto vendorNotificationDto)
        {
            if (vendorNotificationDto.Role == UserRoles.Vendor)
            {
                var vendorNotification = await _repo.Track<FormulaTaskVendor>()
                .Include(t => t.FormulaTask)
                    .ThenInclude(t => t.ChildFormulaTasks)
                        .ThenInclude(t => t.FormulaTaskVendors)
                .FirstOrDefaultAsync(t => t.Id == vendorNotificationDto.NotificationId);

                if (vendorNotification == null)
                {
                    throw new ValidationException("Notification doesn't exist");
                }

                if (vendorNotification.Status != FormulaRequestStatus.None)
                {
                    throw new ValidationException("Answer already committed for this notification");
                }

                vendorNotification.Status = vendorNotificationDto.Answer;
                vendorNotification.Price = vendorNotificationDto.Price.HasValue ? vendorNotificationDto.Price.Value : 0;

                if (vendorNotificationDto.Answer == FormulaRequestStatus.Accepted)
                {
                    foreach (var childFormulaTask in vendorNotification.FormulaTask.ChildFormulaTasks)
                    {
                        var existingRequest = childFormulaTask.FormulaTaskVendors
                            .FirstOrDefault(t => t.VendorGuid == vendorNotification.VendorGuid);

                        if (existingRequest == null)
                        {
                            childFormulaTask.FormulaTaskVendors.Add(_mapper.Map<FormulaTaskVendor>(vendorNotification));
                        }
                        else
                        {
                            existingRequest.Status = FormulaRequestStatus.Accepted;
                            existingRequest.Price = vendorNotification.Price;
                        }
                    }
                }
            }

            if (vendorNotificationDto.Role == UserRoles.Company) // added for compnay in sprint WRT 10B
            {
                var vendorNotification = await _repo.Track<FormulaTaskVendor>()
                    .Include(t => t.FormulaTask)
                        .ThenInclude(t => t.ChildFormulaTasks)
                            .ThenInclude(t => t.FormulaTaskVendors)
                    .FirstOrDefaultAsync(t => t.Id == vendorNotificationDto.NotificationId);

                var vendorchildNotification = await _repo.Track<FormulaTaskVendor>()
                    .Include(t => t.FormulaTask)
                        .ThenInclude(t => t.ChildFormulaTasks)
                            .ThenInclude(t => t.FormulaTaskVendors)
                    .FirstOrDefaultAsync(x => x.FormulaTaskId == vendorNotification.FormulaTaskId && x.VendorGuid == vendorNotification.ChildCompanyWorkerID);

                if (vendorNotification == null)
                {
                    throw new ValidationException("Notification doesn't exist");
                }

                if (vendorNotification.Status != FormulaRequestStatus.None)
                {
                    throw new ValidationException("Answer already committed for this notification");
                }

                if (vendorNotificationDto.Answer == FormulaRequestStatus.Accepted)
                {
                    vendorNotification.Status = FormulaRequestStatus.AcceptedByCompanyAndSentToWorker;
                    vendorNotification.Price = vendorNotificationDto.Price.HasValue ? vendorNotificationDto.Price.Value : 0;

                    vendorchildNotification.Status = FormulaRequestStatus.Accepted;
                    vendorchildNotification.Price = vendorNotificationDto.CompanyWorkerPrice.HasValue ? vendorNotificationDto.CompanyWorkerPrice.Value : 0;
                }
                else
                {
                    vendorNotification.Status = vendorNotificationDto.Answer;
                    vendorNotification.Price = vendorNotificationDto.Price.HasValue ? vendorNotificationDto.Price.Value : 0;

                    vendorchildNotification.Status = vendorNotificationDto.Answer;
                    vendorchildNotification.Price = vendorNotificationDto.CompanyWorkerPrice.HasValue ? vendorNotificationDto.CompanyWorkerPrice.Value : 0;
                }


                if (vendorNotificationDto.Answer == FormulaRequestStatus.Accepted) //Need to look into this with respect to company, company worker WRT 10B (Using As is for now)
                {
                    foreach (var childFormulaTask in vendorNotification.FormulaTask.ChildFormulaTasks)
                    {
                        var existingRequest = childFormulaTask.FormulaTaskVendors
                            .FirstOrDefault(t => t.VendorGuid == vendorNotification.VendorGuid);

                        if (existingRequest == null)
                        {
                            childFormulaTask.FormulaTaskVendors.Add(_mapper.Map<FormulaTaskVendor>(vendorNotification));
                        }
                        else
                        {
                            existingRequest.Status = FormulaRequestStatus.Accepted;
                            existingRequest.Price = vendorNotification.Price;
                        }
                    }
                }
            }

            await _repo.SaveChangesAsync();
        }

        public async Task UpdatePriceForFormulaTask(Guid userId, int formulaTaskId, decimal price)
        {
            var formulaTaskVendor = await _repo.Track<FormulaTaskVendor>()
                .FirstOrDefaultAsync(t => t.FormulaTaskId == formulaTaskId && t.VendorGuid == userId);

            formulaTaskVendor.Price = price;

            await _repo.SaveChangesAsync();
        }

        public async Task DeletePriceForFormulaTask(Guid userId, int formulaTaskId)
        {
            var formulaTaskVendor = await _repo.Track<FormulaTaskVendor>()
                .FirstOrDefaultAsync(t => t.FormulaTaskId == formulaTaskId && t.VendorGuid == userId);

            //formulaTaskVendor.Price = 0.0m;
            _repo.Remove(formulaTaskVendor);
            await _repo.SaveChangesAsync();
        }

        private async Task<decimal> CalculateAmountWithTax(ITransactionScope trx, decimal amount)
        {
            var vendorTax = await trx.Read<CreditsTax>().FirstOrDefaultAsync(t => t.Type == CreditsTaxType.Vendor);
            double amountWithTax = (double)amount - vendorTax.Fee - (vendorTax.Percentage / 100 * (double)amount);
            return (decimal)amountWithTax;
        }

        public async Task<SnapshotDetailDto> GetSnapshotDetail(Guid userId)
        {
            var result = await _repo.ExecuteSql<SnapshotDetailDto>(
                    _mapper,
                    "[dbo].[uspGetVendorStatsByVendorGuid] @VendorGuid",
                    new List<SqlParameter> { new SqlParameter { ParameterName = "@VendorGuid", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId } }
                )
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public async Task SyncVendorData(Guid userId)
        {
            // need to create a new method, for the executenonquery()
            await _repo.ExecuteSql<object>(
                    _mapper,
                    "[dbo].[USPVENDORDATAUPDATEFORLOST] @INPUT_VENDORID",
                    new List<SqlParameter> { new SqlParameter { ParameterName = "@INPUT_VENDORID", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId } }
                )
                .ToListAsync();
        }


        public async Task SendETANotification(int taskId)
        {
            Guid vendorGuid = _principal.GetUserId();

            string vendorName = _principal.GetFullName();

            var task = await _repo.Read<ProjectTask>()
                .Where(c => c.Id == taskId)
                .FirstOrDefaultAsync();

            var taskVendor = await _repo.Read<ProjectTaskVendor>()

                .Where(c => c.ProjectTaskId == taskId && c.VendorGuid == vendorGuid)
                .FirstOrDefaultAsync();


            // send notification to vendor
            List<NotificationEntity> notification = new List<NotificationEntity>();

            notification.Add(new NotificationEntity
            {
                CreateDate = _dateTimeService.NowUtc,
                IsRead = false,
                Message = "You have lost job <b>" + task.Title + "</b> and lost " + taskVendor.Price,
                NotificationType = Global.Enums.NotificationType.ETATimeOut,
                RecipientGuid = vendorGuid,
                SenderGuid = null,
                TaskId = taskId
            });

            // send notification to owner
            notification.Add(new NotificationEntity
            {
                CreateDate = _dateTimeService.NowUtc,
                IsRead = false,
                Message = "Vendor has lost Job <b>" + task.Title + "</b> beign overdue, Please reassign.",
                NotificationType = Global.Enums.NotificationType.ETATimeOut,
                RecipientGuid = task.OwnerGuid,
                SenderGuid = null,
                TaskId = taskId
            });

            _repo.AddRange(notification);
            _repo.SaveChanges();
        }

        public async Task<IList<VendorTaskBidDto>> GetVendorsTaskBids(Guid userId)
        {
            return await _repo.ExecuteSql<VendorTaskBidDto>(_mapper,
                "[dbo].[USP_GetFormulaBidsForVendors] @inputvendorguid",
                new List<SqlParameter> { new SqlParameter { ParameterName = "@inputvendorguid", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId } })
                .ToListAsync();
        }

        //public async Task<IList<CompanyPerformanceDto>> GetPerformanceDataForCompany(Guid userId)
        //{
        //    var rating = StatisticType.Rating;
        //    var working = StatisticType.Working;
        //    var responding = StatisticType.Responding;

        //    var sqlForVendorPerformance = @"[dbo].[uspGetVendorPerformanceData_Company] @userId, @rating, @working, @responding";

        //    return await _repo.ExecuteSql<CompanyPerformanceDto>(_mapper, sqlForVendorPerformance,
        //        new List<SqlParameter> {
        //            new SqlParameter{ ParameterName = "@userId", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId },
        //            new SqlParameter{ ParameterName = "@rating", SqlDbType = SqlDbType.TinyInt, Value = rating },
        //            new SqlParameter{ ParameterName = "@working", SqlDbType = SqlDbType.TinyInt, Value = working },
        //            new SqlParameter{ ParameterName = "@responding", SqlDbType = SqlDbType.TinyInt, Value = responding },
        //        }).ToListAsync();
        //}

        //public async Task<IList<CompanyUserDetailDto>> GetCompanyUserDetails(Guid userId)
        //{
        //    var rating = StatisticType.Rating;
        //    var sqlForVendorPerformance = @"[dbo].[uspGetCompanyUserDetails] @userId, @rating";

        //    return await _repo.ExecuteSql<CompanyUserDetailDto>(_mapper, sqlForVendorPerformance,
        //        new List<SqlParameter> {
        //            new SqlParameter{ ParameterName = "@userId", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId },
        //            new SqlParameter{ ParameterName = "@rating", SqlDbType = SqlDbType.TinyInt, Value = rating },
        //        }).ToListAsync();
        //}
    }
}
