using AutoMapper;
using Braintree;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.User;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Formula.Interfaces;
using IAutoM8.Service.FormulaTasks.Dto;
using IAutoM8.Service.FormulaTasks.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Projects.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace IAutoM8.Service.Formula
{
    public class FormulaToProjectConverterService : IFormulaToProjectConverterService
    {
        private readonly IRepo _repo;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly ITaskImportService _taskImportService;
        private readonly ITaskStartDateHelperService _startDateHelperService;
        private readonly IFormulaTaskOutsourcesService _formulaTaskOutsourcesService;
        private readonly IProjectTaskOutsourcesService _projectTaskOutsourcesService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IFormulaNeo4jRepository _formulaNeo4JRepository;
        private readonly IStorageService _storageService;
        private readonly ClaimsPrincipal _principal;

        public FormulaToProjectConverterService(
            IRepo repo,
            IMapper mapper,
            IProjectService projectService,
            ITaskImportService taskImportService,
            ITaskStartDateHelperService startDateHelperService,
            IFormulaTaskOutsourcesService formulaTaskOutsourcesService,
            IProjectTaskOutsourcesService projectTaskOutsourcesService,
            ITaskNeo4jRepository taskNeo4JRepository,
            IFormulaNeo4jRepository formulaNeo4JRepository,
            IStorageService storageService,
            ClaimsPrincipal principal)
        {
            _repo = repo;
            _mapper = mapper;
            _projectService = projectService;
            _taskImportService = taskImportService;
            _startDateHelperService = startDateHelperService;
            _formulaTaskOutsourcesService = formulaTaskOutsourcesService;
            _projectTaskOutsourcesService = projectTaskOutsourcesService;
            _taskNeo4JRepository = taskNeo4JRepository;
            _formulaNeo4JRepository = formulaNeo4JRepository;
            _storageService = storageService;
            _principal = principal;
        }

        public async Task<int> CreateProject(int formulaId, CreateProjectDto createProjectDto, ClaimsPrincipal user)
        {
            int projectId;
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var formulaProject = await trx.Track<FormulaProject>()
                    .Include(p => p.FormulaTasks)
                        .ThenInclude(task => task.ChildTasks)
                    .Include(p => p.FormulaTasks)
                        .ThenInclude(task => task.RecurrenceOptions)
                    .Include(p => p.FormulaTasks)
                        .ThenInclude(task => task.InternalFormulaProject)
                    .Include(p => p.FormulaTasks)
                        .ThenInclude(task => task.AssignedSkill)
                    .Include(p => p.FormulaTasks)
                        .ThenInclude(task => task.ReviewingSkill)
                    .Include(i => i.ResourceFormula)
                        .ThenInclude(i => i.Resource)
                    .FirstOrDefaultAsync(x => x.Id == formulaId && !x.IsDeleted);

                if (formulaProject == null)
                    throw new ValidationException("Formula is not found.");

                var project = await _projectService.CreateProjectFromFormula(
                    trx,
                    formulaProject,
                    createProjectDto.ProjectStartDates.ProjectStartDateTime ?? DateTime.UtcNow,
                    createProjectDto.ProjectName,
                    createProjectDto.ParentProjectId
                );

                var dictionary =
                    await _taskImportService.ImportTasksIntoProjectAsync(trx, project, formulaProject.FormulaTasks,
                    createProjectDto.ProjectStartDates.ProjectStartDateTime ?? DateTime.UtcNow,
                    createProjectDto.SkillMappings.Where(w => w.IsOutsorced).Select(s => s.SkillId),
                        createProjectDto.SkillMappings);

                //var rootStartDates = createProjectDto
                //    .ProjectStartDates
                //    .RootStartDateTime
                //    .ToDictionary(
                //        pair => dictionary.First(x => x.Value == pair.Key).Key,
                //        pair => pair.Value);

                var rootStartDates = new Dictionary<int, DateTime?>();

                createProjectDto.ProjectStartDates.RootStartDateTime = rootStartDates;

                var projectRoots = await _taskNeo4JRepository.GetProjectRootTaskIdsAsync(project.Id);
                var result = await _startDateHelperService.InitTasksStartDate(trx, project.Id, createProjectDto.ProjectStartDates, projectRoots);

                await _taskImportService.ScheduleJobsAsync(trx, result.RootTasks);
                await trx.SaveAndCommitAsync(CancellationToken.None);
                transaction.Commit();
                projectId = project.Id;

                // Reversing the Key - Value pair with FormulaTaskId and ProjectTaskId
                var rootTaskIds = dictionary.Where(w => projectRoots.Contains(w.Key)).ToDictionary(k => k.Value, v => v.Key);
                var nonRootTaskIds = dictionary.Where(w => !projectRoots.Contains(w.Key)).ToDictionary(k => k.Value, v => v.Key);

                // Send the job invites to the vendors for rootTasks in a project, which gets started automatically
                var outsourcedRequestList = createProjectDto.SkillMappings
                    .Where(w => rootTaskIds.ContainsKey(w.FormulaTaskId) && w.IsOutsorced && w.OutsourceUserIds.Count() > 0)
                    .Select(s => new OutsourceRequestDto
                    {
                        TaskId = rootTaskIds[s.FormulaTaskId],
                        Outsources = s.OutsourceUserIds.Select(o => new OutsourceRequestItemDto { Id = o, IsSelected = true }).ToList()
                    })
                    .ToList();

                if (outsourcedRequestList != null && outsourcedRequestList.Count > 0)
                {
                    foreach (var request in outsourcedRequestList)
                    {
                        await _projectTaskOutsourcesService.CreateRequest(request);
                    }
                }

                // Update the status for vendors of the non-root tasks to enum: ProjectRequestStatus.JobInviteEnqueue
                await UpdateProjectTaskVendorStatus(createProjectDto.SkillMappings, nonRootTaskIds);
            }

            return projectId;
        }

        public async Task<SkillImportDto> GetSkills(int formulaId)
        {
            var ownerId = _principal.GetOwnerId();
            var formulaIds = await _formulaNeo4JRepository.GetAllInternalFormulaIds(formulaId);
            var workerSkills = GetWorkerSkills(formulaIds);
            var reviewingSkills = GetReviewingSkills(formulaIds);
            //var workerSkillTasks = GetFormulaTaskWithWorkerSkills(formulaIds);
            //var reviewingSkillTasks = GetFormulaTaskWithReviewerSkills(formulaIds);
            //var outsourcers = GetOutsourcers(formulaIds);

            var usersWithRoles = await _repo.Read<UserProfile>()
                .Include(x => x.User)
                    .ThenInclude(i => i.Roles)
                        .ThenInclude(i => i.Role)
                .Where(p => !p.User.IsLocked && (p.User.Id == ownerId || p.User.OwnerId == ownerId))
                .Select(x => new
                {
                    x.FullName,
                    x.UserId,
                    x.Path,
                    IsOwnerOrManager = x.User.Roles.Any(w => w.Role.Name == UserRoles.Manager || w.Role.Name == UserRoles.Owner)
                })
                .ToListAsync();

            return new SkillImportDto
            {

                SkillItems = GetAllSkills(workerSkills, reviewingSkills),
                //FormulaTaskItems = GetAllFormulaTaskSkills(workerSkillTasks, reviewingSkillTasks),
                FormulaTaskItems = GetFormulaTaskSkills(formulaIds, formulaId),
                AllUsers = usersWithRoles.Select(s => new SkillUserItemDto
                {
                    Id = s.UserId,
                    FullName = s.FullName,
                    ProfileImage = !String.IsNullOrWhiteSpace(s.Path)
                        ? _storageService.GetProfileImageUri($"{s.UserId}/{s.Path}")
                        : String.Empty
                }).ToList(),
                ManagerUsers = usersWithRoles.Where(w => w.IsOwnerOrManager).Select(s => new SkillUserItemDto
                {
                    Id = s.UserId,
                    FullName = s.FullName,
                    ProfileImage = !String.IsNullOrWhiteSpace(s.Path)
                        ? _storageService.GetProfileImageUri($"{s.UserId}/{s.Path}")
                        : String.Empty
                }).ToList(),
                //OutsourcedUsers = GetOutsourcers(formulaIds)
                OutsourcedUsers = new List<FormulaTaskOutsourceDto>()
            };
        }
        private List<SkillImportItemDto> GetWorkerSkills(IEnumerable<int> formulaIds)
        {
            return _repo.Read<FormulaTask>()
                .Include(i => i.FormulaTaskVendors)
                .Where(w => formulaIds.Contains(w.FormulaProjectId) && w.AssignedSkillId.HasValue)
                .Select(s => new
                {
                    Id = s.AssignedSkillId.Value,
                    CanBeOutsourced = s.FormulaTaskVendors.Any(a => a.Status == FormulaRequestStatus.Accepted)
                })
                .GroupBy(g => g.Id)
                .Select(s => new
                {
                    Id = s.Key,
                    CanBeOutsourced = s.Any(a => a.CanBeOutsourced)
                })
                .Join(_repo.Read<Skill>().Include(i => i.UserSkills),
                outer => outer.Id, inner => inner.Id,
                (partial, skill) => new SkillImportItemDto
                {
                    SkillId = partial.Id,
                    Name = skill.Name,
                    CanBeOutsourced = partial.CanBeOutsourced,
                    CanWorkerHasSkill = true,
                    UserIds = skill.UserSkills.Select(s => s.UserId).ToList()
                }).ToList();
        }
        private List<SkillImportItemDto> GetReviewingSkills(IEnumerable<int> formulaIds)
        {
            return _repo.Read<FormulaTask>()
                .Where(w => formulaIds.Contains(w.FormulaProjectId) && w.ReviewingSkillId.HasValue)
                .Select(s => new { SkillId = s.ReviewingSkillId.Value, OwnerGuid = s.OwnerGuid })
                .GroupBy(g => g)
                .Join(_repo.Read<Skill>().Include(i => i.UserSkills),
                outer => outer.Key.SkillId, inner => inner.Id,
                (partial, skill) => new SkillImportItemDto
                {
                    SkillId = partial.Key.SkillId,
                    Name = skill.Name,
                    CanBeOutsourced = false,
                    CanWorkerHasSkill = false,
                    UserIds = skill.UserSkills
                        .Where(w => w.User.Id == partial.Key.OwnerGuid)
                        .Select(s => s.UserId)
                        .ToList()
                }).ToList();
        }

        private List<SkillImportItemDto> GetAllSkills(IEnumerable<SkillImportItemDto> workerSkills,
            IEnumerable<SkillImportItemDto> reviewingSkills)
        {
            return workerSkills.Concat(reviewingSkills)
                .GroupBy(g => g.SkillId)
                .Select(s => new SkillImportItemDto
                {
                    SkillId = s.Key,
                    Name = s.Select(sm => sm.Name).First(),
                    CanBeOutsourced = s.Any(sm => sm.CanBeOutsourced),
                    CanWorkerHasSkill = s.All(sm => sm.CanWorkerHasSkill),
                    UserIds = s.SelectMany(sm => sm.UserIds)
                        .GroupBy(g => g).Select(sm => sm.Key).ToList()
                }).ToList();
        }

        private List<FormulaSkillImportDto> GetFormulaTaskWithWorkerSkills(IEnumerable<int> formulaIds)
        {
            return _repo.Read<FormulaTask>()
                .Include(i => i.FormulaTaskVendors)
                .Include(x => x.AssignedFormulaTeam)
                .Include(x => x.AssignedSkill)
                .Where(w => formulaIds.Contains(w.FormulaProjectId) && w.AssignedSkillId.HasValue)
                .Select(s => new
                {
                    FormulaTaskId = s.Id,
                    SkillId = s.AssignedSkillId.Value,
                    Title = s.Title,
                    Skill = s.AssignedSkill.Name,
                    Team = s.AssignedFormulaTeam.Name,
                    CanBeOutsourced = s.FormulaTaskVendors.Any(a => a.Status == FormulaRequestStatus.Accepted)
                })
                .GroupBy(g => new { g.FormulaTaskId, g.SkillId })
                .Select(s => new
                {
                    FormulaTaskId = s.Key.FormulaTaskId,
                    SkillId = s.Key.SkillId,
                    Title = s.Select(st => st.Title).FirstOrDefault(),
                    Skill = s.Select(st => st.Skill).FirstOrDefault(),
                    Team = s.Select(st => st.Team).FirstOrDefault(),
                    CanBeOutsourced = s.Any(a => a.CanBeOutsourced)
                })
                .Join(_repo.Read<Skill>().Include(i => i.UserSkills),
                    outer => outer.SkillId, inner => inner.Id,
                    (partial, skill) => new FormulaSkillImportDto
                    {
                        FormulaTaskId = partial.FormulaTaskId,
                        SkillId = partial.SkillId,
                        Title = partial.Title,
                        Skill = partial.Skill,
                        Team = partial.Team,
                        CanBeOutsourced = partial.CanBeOutsourced,
                        CanWorkerHasSkill = true,
                        UserIds = skill.UserSkills.Select(s => s.UserId).ToList()
                    })
                .ToList();
        }

        private List<FormulaSkillImportDto> GetFormulaTaskWithReviewerSkills(IEnumerable<int> formulaIds)
        {
            return _repo.Read<FormulaTask>()
                .Include(x => x.AssignedFormulaTeam)
                .Include(x => x.ReviewingSkill)
                .Where(w => formulaIds.Contains(w.FormulaProjectId) && w.ReviewingSkillId.HasValue)
                .Join(_repo.Read<Skill>().Include(i => i.UserSkills),
                    outer => outer.ReviewingSkillId, inner => inner.Id,
                    (partial, skill) => new FormulaSkillImportDto
                    {
                        FormulaTaskId = partial.Id,
                        ReviewingSkillId = partial.ReviewingSkillId.Value,
                        Title = partial.Title,
                        Skill = partial.ReviewingSkill.Name,
                        Team = partial.AssignedFormulaTeam.Name,
                        CanBeOutsourced = false,
                        CanWorkerHasSkill = false,
                        CanReviewerHasSkill = true,
                        ReviewingUserIds = skill.UserSkills.Select(s => s.UserId).ToList()
                    })
                .ToList();
        }

        private List<FormulaSkillImportDto> GetAllFormulaTaskSkills(IEnumerable<FormulaSkillImportDto> workerSkillTasks,
            IEnumerable<FormulaSkillImportDto> reviewingSkillTasks,
            IEnumerable<int> disabledTaskIds)
        {
            var ownerId = new[] { _principal.GetOwnerId() };
            var result = new List<FormulaSkillImportDto>();

            foreach (var workerSkill in workerSkillTasks)
            {

                var reviewerSkill = reviewingSkillTasks.SingleOrDefault(r => r.FormulaTaskId == workerSkill.FormulaTaskId);
                var certifiedVendors = workerSkill.CanBeOutsourced
                        ? GetCertifiedVendors(workerSkill.FormulaTaskId)
                        : new List<FormulaTaskOutsourceDto>();

                List<Guid> outsourcerUserIds = new List<Guid>();
                //if (workerSkill.CanBeOutsourced)
                //{
                //    var topRatedVendors = certifiedVendors.Where(w => w.AvgRating >= 3).Select(v => v.Id);
                //    var count = topRatedVendors.Count();
                //    var topThirty = count >= 4 ? (int)Math.Round(count * 0.3) : 3;

                //    outsourcerUserIds = count > 5
                //        ? topRatedVendors.Take(topThirty).ToList()
                //        : certifiedVendors.Select(v => v.Id).Take(topThirty).ToList();
                //}

                if (reviewerSkill != null)
                {
                    var rSkill = reviewerSkill.ReviewingUserIds.Union(ownerId).ToList();

                    result.Add(new FormulaSkillImportDto
                    {
                        FormulaTaskId = workerSkill.FormulaTaskId,
                        SkillId = workerSkill.SkillId,
                        ReviewingSkillId = reviewerSkill.ReviewingSkillId,
                        Title = workerSkill.Title,
                        Skill = workerSkill.Skill,
                        Team = workerSkill.Team,
                        CanBeOutsourced = workerSkill.CanBeOutsourced,
                        CanWorkerHasSkill = workerSkill.CanWorkerHasSkill,
                        CanReviewerHasSkill = true,
                        UserIds = disabledTaskIds.Contains(workerSkill.FormulaTaskId) ? new List<Guid>() : workerSkill.UserIds,
                        ReviewingUserIds = disabledTaskIds.Contains(workerSkill.FormulaTaskId) ? new List<Guid>() : rSkill,
                        OutsourceUserIds = outsourcerUserIds,
                        CertifiedVendors = disabledTaskIds.Contains(workerSkill.FormulaTaskId) ? new List<FormulaTaskOutsourceDto>() : certifiedVendors,
                        IsDisabled = disabledTaskIds.Contains(workerSkill.FormulaTaskId)
                    });
                }
                else
                {
                    result.Add(new FormulaSkillImportDto
                    {
                        FormulaTaskId = workerSkill.FormulaTaskId,
                        SkillId = workerSkill.SkillId,
                        ReviewingSkillId = null,
                        Title = workerSkill.Title,
                        Skill = workerSkill.Skill,
                        Team = workerSkill.Team,
                        CanBeOutsourced = workerSkill.CanBeOutsourced,
                        CanWorkerHasSkill = workerSkill.CanWorkerHasSkill,
                        CanReviewerHasSkill = false,
                        UserIds = disabledTaskIds.Contains(workerSkill.FormulaTaskId) ? new List<Guid>() : workerSkill.UserIds,
                        ReviewingUserIds = new List<Guid>(),
                        OutsourceUserIds = outsourcerUserIds,
                        CertifiedVendors = disabledTaskIds.Contains(workerSkill.FormulaTaskId) ? new List<FormulaTaskOutsourceDto>() : certifiedVendors,
                        IsDisabled = disabledTaskIds.Contains(workerSkill.FormulaTaskId)
                    });
                }
            }

            return result;
        }

        private List<FormulaTaskOutsourceDto> GetCertifiedVendors(int formulaTaskId)
        {
            var CertifiedVendorList =  _repo.Read<FormulaTaskVendor>()
                .Include(i => i.Vendor)
                    .ThenInclude(t => t.Profile)
                .Include(i => i.Vendor)
                    .ThenInclude(t => t.Roles)
                    .ThenInclude(t => t.Role)
                .Include(i => i.FormulaTask)
                    .ThenInclude(t => t.FormulaProject)
                    .Where(w => w.Vendor.Roles.Any(r => r.Role.Name == UserRoles.Vendor || r.Role.Name == UserRoles.CompanyWorker || r.Role.Name == UserRoles.Company) // added logic for new role Company WRT Sprint 10B
                    && w.Status == FormulaRequestStatus.Accepted
                    && w.FormulaTaskId == formulaTaskId)
                .Select(s => new
                {
                    VendorGuid = s.VendorGuid,
                    FormulaTaskId = s.FormulaTaskId,
                    FullName = s.Vendor.Profile.FullName,
                    Price = s.Price,
                    ProfileImage = s.Vendor.Profile.Path
                })
                .GroupJoin(_repo.Read<FormulaTaskStatistic>(),
                    outer => new { outer.VendorGuid, outer.FormulaTaskId },
                    inner => new { inner.VendorGuid, inner.FormulaTaskId },
                    (partial, statistic) => new { partial, statistic })
                .SelectMany(sm => sm.statistic.DefaultIfEmpty(), (x, y) => new { partial = x.partial, statistic = y ?? new FormulaTaskStatistic() })
                .GroupBy(g => new { g.partial.VendorGuid, g.partial.FullName })
                .Select(s => new FormulaTaskOutsourceDto
                {
                    Id = s.Key.VendorGuid,
                    FullName = s.Key.FullName,
                    Role = UserRoles.RoleName(_repo.Read<UserRole>().Where(a => a.UserId == s.Key.VendorGuid).Select(a => a.RoleId).FirstOrDefault()),
                    Price = s.Select(x => x.partial.Price).FirstOrDefault(),
                    AvgRating = s.Where(w => w.statistic.Type == StatisticType.Rating).Average(sum => sum.statistic.Value) ?? 0,
                    AvgWorking = s.Where(w => w.statistic.Type == StatisticType.Working && w.statistic.Completed.HasValue)
                        .Average(avg => avg.statistic.Value < 0 ? -(avg.statistic.Value) : avg.statistic.Value) ?? 0,
                    TaskCompleted = s.Where(w => w.statistic.Type == StatisticType.Working && w.statistic.Completed.HasValue)
                        .Count(),
                    ProfileImage = _storageService
                        .GetProfileImageUri($"{s.Key.VendorGuid}/{s.Select(x => x.partial.ProfileImage).FirstOrDefault()}")
                })
                .OrderByDescending(o => o.AvgRating)
                .ThenByDescending(o => o.Price)
                .ToList();

            List<FormulaTaskOutsourceDto> certifiedVendorsWithCompanyWorker = new List<FormulaTaskOutsourceDto>();
            List<FormulaTaskOutsourceDto> certifiedVendorsWithOutCompanyWorker = new List<FormulaTaskOutsourceDto>();
            List<FormulaTaskOutsourceDto> certifiedFinalvendorList = new List<FormulaTaskOutsourceDto>();
            foreach (var item in CertifiedVendorList)
            {
             if(item.Role == "CompanyWorker")
                {
                    var UpdatePrice = _repo.Read<FormulaTaskVendor>().Where(a => a.FormulaTaskId == formulaTaskId && a.VendorGuid == item.Id).Select(a => a).FirstOrDefault();
                    UpdatePrice.Price = _repo.Read<FormulaTaskVendor>().Where(a => a.FormulaTaskId == formulaTaskId && a.ChildCompanyWorkerID == item.Id).Select(a => a.Price).FirstOrDefault();
                    _repo.SaveChanges();
                    item.Price = UpdatePrice.Price;
                    certifiedVendorsWithCompanyWorker.Add(item);
                }
                else
                {
                    certifiedVendorsWithOutCompanyWorker.Add(item);
                }

            }
            certifiedVendorsWithCompanyWorker.AddRange(certifiedVendorsWithOutCompanyWorker);
            certifiedFinalvendorList = certifiedVendorsWithCompanyWorker;

            return certifiedFinalvendorList;  
            //Below Code are Original --
            //return _repo.Read<FormulaTaskVendor>()
            //    .Include(i => i.Vendor)
            //        .ThenInclude(t => t.Profile)
            //    .Include(i => i.Vendor)
            //        .ThenInclude(t => t.Roles)
            //        .ThenInclude(t => t.Role)
            //    .Include(i => i.FormulaTask)
            //        .ThenInclude(t => t.FormulaProject)
            //        //.Where(w => w.Vendor.Roles.Any(r => r.Role.Name == UserRoles.Vendor)
            //        //.Where(w => w.Vendor.Roles.Any(r => r.Role.Name == UserRoles.Vendor || r.Role.Name == UserRoles.CompanyWorker) // added logic for new role CompanyWorker WRT Sprint 10B
            //        .Where(w => w.Vendor.Roles.Any(r => r.Role.Name == UserRoles.Vendor || r.Role.Name == UserRoles.CompanyWorker || r.Role.Name == UserRoles.Company) // added logic for new role Company WRT Sprint 10B
            //        && w.Status == FormulaRequestStatus.Accepted
            //        && w.FormulaTaskId == formulaTaskId)
            //    .Select(s => new
            //    {
            //        VendorGuid = s.VendorGuid,
            //        FormulaTaskId = s.FormulaTaskId,
            //        FullName = s.Vendor.Profile.FullName,
            //        Price = s.Price,
            //        ProfileImage = s.Vendor.Profile.Path
            //    })
            //    .GroupJoin(_repo.Read<FormulaTaskStatistic>(),
            //        outer => new { outer.VendorGuid, outer.FormulaTaskId },
            //        inner => new { inner.VendorGuid, inner.FormulaTaskId },
            //        (partial, statistic) => new { partial, statistic })
            //    .SelectMany(sm => sm.statistic.DefaultIfEmpty(), (x, y) => new { partial = x.partial, statistic = y ?? new FormulaTaskStatistic() })
            //    .GroupBy(g => new { g.partial.VendorGuid, g.partial.FullName })
            //    .Select(s => new FormulaTaskOutsourceDto
            //    {
            //        Id = s.Key.VendorGuid,
            //        FullName = s.Key.FullName,
            //        Price = s.Select(x => x.partial.Price).FirstOrDefault(),
            //        AvgRating = s.Where(w => w.statistic.Type == StatisticType.Rating).Average(sum => sum.statistic.Value) ?? 0,
            //        AvgWorking = s.Where(w => w.statistic.Type == StatisticType.Working && w.statistic.Completed.HasValue)
            //            .Average(avg => avg.statistic.Value < 0 ? -(avg.statistic.Value) : avg.statistic.Value) ?? 0,
            //        TaskCompleted = s.Where(w => w.statistic.Type == StatisticType.Working && w.statistic.Completed.HasValue)
            //            .Count(),
            //        ProfileImage = _storageService
            //            .GetProfileImageUri($"{s.Key.VendorGuid}/{s.Select(x => x.partial.ProfileImage).FirstOrDefault()}")
            //    })
            //    .OrderByDescending(o => o.AvgRating)
            //    .ThenByDescending(o => o.Price)
            //    .ToList();
        }

        // formulaIds contains the internal forumalaids for a given formulaId
        private List<FormulaSkillImportDto> GetFormulaTaskSkills(IEnumerable<int> formulaIds, int formulaId)
        {
            var disabledTaskIds = _repo.Read<FormulaTaskDisableStatus>()
                .Where(f => f.ParentFormulaId == formulaId)
                .Select(f => f.InternalChildFormulaTaskId)
                .ToList();

            var workerTaskSkills = _repo.Read<FormulaTask>()
                .Include(i => i.FormulaTaskVendors)
                .Include(x => x.AssignedFormulaTeam)
                .Include(x => x.AssignedSkill)
                .Where(w => formulaIds.Contains(w.FormulaProjectId) && w.AssignedSkillId.HasValue)
                .Select(s => new
                {
                    FormulaTaskId = s.Id,
                    SkillId = s.AssignedSkillId.Value,
                    Title = s.Title,
                    Skill = s.AssignedSkill.Name,
                    Team = s.AssignedFormulaTeam.Name,
                    CanBeOutsourced = s.FormulaTaskVendors.Any(a => a.Status == FormulaRequestStatus.Accepted)
                })
                .GroupBy(g => new { g.FormulaTaskId, g.SkillId })
                .Select(s => new
                {
                    FormulaTaskId = s.Key.FormulaTaskId,
                    SkillId = s.Key.SkillId,
                    Title = s.Select(st => st.Title).FirstOrDefault(),
                    Skill = s.Select(st => st.Skill).FirstOrDefault(),
                    Team = s.Select(st => st.Team).FirstOrDefault(),
                    CanBeOutsourced = s.Any(a => a.CanBeOutsourced)
                })
                .Join(_repo.Read<Skill>().Include(i => i.UserSkills),
                    outer => outer.SkillId, inner => inner.Id,
                    (partial, skill) => new FormulaSkillImportDto
                    {
                        FormulaTaskId = partial.FormulaTaskId,
                        SkillId = partial.SkillId,
                        Title = partial.Title,
                        Skill = partial.Skill,
                        Team = partial.Team,
                        CanBeOutsourced = partial.CanBeOutsourced,
                        CanWorkerHasSkill = true,
                        UserIds = skill.UserSkills.Select(s => s.UserId).ToList()
                    })
                .ToList();

            var reviewerTaskSkills = _repo.Read<FormulaTask>()
                .Include(x => x.AssignedFormulaTeam)
                .Include(x => x.ReviewingSkill)
                .Where(w => formulaIds.Contains(w.FormulaProjectId) && w.ReviewingSkillId.HasValue)
                .Join(_repo.Read<Skill>().Include(i => i.UserSkills),
                    outer => outer.ReviewingSkillId, inner => inner.Id,
                    (partial, skill) => new FormulaSkillImportDto
                    {
                        FormulaTaskId = partial.Id,
                        ReviewingSkillId = partial.ReviewingSkillId.Value,
                        Title = partial.Title,
                        Skill = partial.ReviewingSkill.Name,
                        Team = partial.AssignedFormulaTeam.Name,
                        CanBeOutsourced = false,
                        CanWorkerHasSkill = false,
                        CanReviewerHasSkill = true,
                        ReviewingUserIds = skill.UserSkills
                            .Select(s => s.UserId)
                            .ToList()
                    })
                .ToList();


            var combinedSkills = GetAllFormulaTaskSkills(workerTaskSkills, reviewerTaskSkills, disabledTaskIds);


            var resultset = _repo.ExecuteSql<FormulaTaskSortOrderDto>(_mapper,
                "uspAssignSkillSortOrder @formulaid",
                new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@formulaid", SqlDbType = SqlDbType.Int, Value = formulaId }
                }).ToListAsync();

            var formulaTaskIds = resultset.Result;

            var result = new List<FormulaSkillImportDto>();

            foreach (var task in formulaTaskIds)
            {
                var skill = combinedSkills.SingleOrDefault(ee => ee.FormulaTaskId == task.TaskId);
                if (skill != null)
                {
                    result.Add(skill);
                }
                combinedSkills.Remove(skill);
            }

            if (formulaTaskIds.Count != 0)
            {
                combinedSkills.ForEach(s => result.Insert(0, s));
            }

            if (formulaTaskIds.Count == 0 && combinedSkills.Count != 0)
            {
                combinedSkills.ForEach(s => result.Add(s));
            }

            return result;
        }

        //public async Task<string> GetFormulaMeanTat(int formulaId)
        //{
        //    var resultset = await _repo.ExecuteSql<FormulaMeanTatDto>(_mapper,
        //        "uspGetFormulaMeanTat @formulaid",
        //        new List<SqlParameter>
        //        {
        //            new SqlParameter { ParameterName = "@formulaid", SqlDbType = SqlDbType.Int, Value = formulaId }
        //        })
        //        .ToListAsync();

        //    if (resultset != null && resultset.Count > 0)
        //    {
        //        return resultset[0].MeanTat;
        //    }

        //    return "00:00";
        //}


        public async Task<AllFormulaMeanTatDto> GetFormulaMeanTat(int formulaId)
        {
            var resultset = await _repo.ExecuteSql<AllFormulaMeanTatDto>(_mapper,
                "uspGetAssignskillFormulaTat @formulaId",
                new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@formulaId", SqlDbType = SqlDbType.Int, Value = formulaId }
                })
                .ToListAsync();

            return resultset.FirstOrDefault();
        }



        private async Task UpdateProjectTaskVendorStatus(IEnumerable<SkillMapDto> mappings, Dictionary<int, int> nonRootTaskIds)
        {
            using (var trx = _repo.Transaction())
            {
                foreach (var mapping in mappings)
                {
                    if (nonRootTaskIds.ContainsKey(mapping.FormulaTaskId))
                    {
                        foreach (var vendorId in mapping.OutsourceUserIds)
                        {
                            var projectTaskVendor = await trx.Track<ProjectTaskVendor>()
                                .SingleOrDefaultAsync(w => w.ProjectTaskId == nonRootTaskIds[mapping.FormulaTaskId] && w.VendorGuid == vendorId);

                            if (projectTaskVendor != null)
                            {
                                projectTaskVendor.Status = ProjectRequestStatus.JobInviteEnqueue;
                            }
                        }
                    }
                }
                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }
    }
}
