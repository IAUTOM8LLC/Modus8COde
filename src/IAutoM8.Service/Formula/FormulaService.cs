using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Domain.Models.User;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Extensions;
using IAutoM8.Global.Options;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Formula.Interfaces;
using IAutoM8.Service.FormulaTasks.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Projects.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Teams.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace IAutoM8.Service.Formula
{
    public class FormulaService : IFormulaService
    {
        private readonly IRepo _repo;
        private readonly ITaskImportService _taskImportService;
        private readonly ClaimsPrincipal _principal;
        private readonly ITaskStartDateHelperService _startDateHelperService;
        private readonly IStorageService _storageService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IFormulaNeo4jRepository _formulaNeo4JRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IFormulaTaskService _formulaTaskService;
        private readonly IEntityFrameworkPlus _entityFrameworkPlus;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProjectTaskOutsourcesService _projectTaskOutsourcesService;
        private readonly PredefinedFormulaSeller _sellers;
        private const string Ascending = "ascending";
        private readonly IFormulaTaskNeo4jRepository _formulaTaskNeo4JRepository;
        public FormulaService(
            ITaskImportService taskImportService,
            IRepo repo,
            ClaimsPrincipal principal,
            ITaskStartDateHelperService startDateHelperService,
            IStorageService storageService,
            ITaskNeo4jRepository taskNeo4JRepository,
            IFormulaNeo4jRepository formulaNeo4JRepository,
            IDateTimeService dateTimeService,
            IFormulaTaskService formulaTaskService,
            IEntityFrameworkPlus entityFrameworkPlus,
            IMapper mapper,
            IProjectService projectService,
            IOptions<PredefinedFormulaSeller> sellerOptions,
            IProjectTaskOutsourcesService projectTaskOutsourcesService,
            IFormulaTaskNeo4jRepository formulaTaskNeo4JRepository)
        {
            _repo = repo;
            _principal = principal;
            _startDateHelperService = startDateHelperService;
            _taskImportService = taskImportService;
            _storageService = storageService;
            _taskNeo4JRepository = taskNeo4JRepository;
            _dateTimeService = dateTimeService;
            _formulaNeo4JRepository = formulaNeo4JRepository;
            _formulaTaskService = formulaTaskService;
            _entityFrameworkPlus = entityFrameworkPlus;
            _mapper = mapper;
            _projectService = projectService;
            _sellers = sellerOptions.Value;
            _projectTaskOutsourcesService = projectTaskOutsourcesService;
            _formulaTaskNeo4JRepository = formulaTaskNeo4JRepository;
        }

        public async Task<List<FormulaListingDto>> GetOwnedFormulas()
        {
            var ownerGuid = _principal.GetOwnerId();
            var isAdmin = _principal.IsAdmin();

            if (isAdmin)
            {
                return await _repo.Read<FormulaProject>()
                    .Include(i => i.FormulaTasks)
                    .Where(x => x.OwnerGuid == ownerGuid || (x.Owner.OwnerId == ownerGuid))
                    .Where(w => !w.IsDeleted && w.IsGlobal && w.Status == (int)FormulaProjectStatus.UserTask)
                    .ProjectTo<FormulaListingDto>(_mapper.ConfigurationProvider)
                    .OrderBy(o => o.Name)
                    .ToListAsync();
            }

            var ownedFormulas = await _repo.Read<FormulaProject>()
                .Include(i => i.FormulaTasks)
                .Where(x => !x.IsDeleted)
                .Where(x => x.OwnerGuid == ownerGuid || (x.Owner.OwnerId == ownerGuid))
                .ProjectTo<FormulaListingDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var globalFormulas = await _repo.Read<FormulaProject>()
                .Include(i => i.FormulaTasks)
                .Where(w => !w.IsDeleted && w.IsGlobal && w.Status == (int)FormulaProjectStatus.UserTask)
                .ProjectTo<FormulaListingDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            //var formulas = await _repo.Read<FormulaProject>()
            //    .Where(x => !x.IsDeleted)
            //    .Where(x => x.OwnerGuid == ownerGuid || (x.Owner.OwnerId == ownerGuid))
            //    .Union(_repo.Read<FormulaProject>()
            //        .Where(w => !w.IsDeleted && w.IsGlobal && w.Status == (int)FormulaProjectStatus.UserTask))
            //    .OrderBy(o => o.Name)
            //    .ToListAsync();

            return globalFormulas.Union(ownedFormulas)
                .OrderBy(o => o.Name)
                .ToList(); ;
        }

        public async Task<SearchFormulaResultDto<FormulaListingDto>> GetFormulas(SearchFormulaDto search)
        {
            var ownerGuid = _principal.GetOwnerId();

            var query = GetFormulaProjects(search)
                .Where(x => x.OwnerGuid == ownerGuid || (x.Owner.OwnerId == ownerGuid));
            var totalCount = query.Count();
            return new SearchFormulaResultDto<FormulaListingDto>
            {
                TotalCount = totalCount,
                Formulas = await GetPaging(query.ProjectTo<FormulaListingDto>(_mapper.ConfigurationProvider), search).ToListAsync(),
                IsAdmin = _principal.IsAdmin()
            };
        }

        public async Task<SearchFormulaResultDto<FormulaListingDto>> GetCustomFormulas(SearchFormulaDto search)
        {
            var ownerGuid = _principal.GetOwnerId();

            var query = GetFormulaProjects(search)
                .Where(x => !x.IsGlobal)
                .Where(x => x.OwnerGuid == ownerGuid || (x.Owner.OwnerId == ownerGuid));
            var totalCount = query.Count();
            return new SearchFormulaResultDto<FormulaListingDto>
            {
                TotalCount = totalCount,
                Formulas = await GetPaging(query.ProjectTo<FormulaListingDto>(_mapper.ConfigurationProvider), search).ToListAsync(),
                IsAdmin = _principal.IsAdmin()
            };
        }

        public async Task<SearchFormulaResultDto<FormulaListingDto>> GetPublicFormulas(SearchFormulaDto search)
        {


            var isAdmin = _principal.IsAdmin();

            var query = _repo.Read<FormulaProject>()
                .Include(f => f.Owner)
                    .ThenInclude(f => f.Profile).AsQueryable();

            if (isAdmin)
            {
                query = query.Where(w => w.OwnerGuid == _principal.GetUserId() && !w.IsDeleted && w.IsGlobal && w.Status != (int)FormulaProjectStatus.UserTask);
            }
            else
            {
                query = query.Where(w => !w.IsDeleted && w.IsGlobal && w.Status == (int)FormulaProjectStatus.UserTask);
            }

            if (!string.IsNullOrEmpty(search.FilterSearch))
            {
                query = query.Where(w => w.Name.Contains(search.FilterSearch));
            }

            if (search.FilterCategorieIds != null)
                query = query.Where(w => w.FormulaProjectCategories.Any(a => search.FilterCategorieIds.Contains(a.CategoryId)));



            var totalCount = query.Count();
            return new SearchFormulaResultDto<FormulaListingDto>
            {
                TotalCount = totalCount,
                Formulas = await GetPaging(query.ProjectTo<FormulaListingDto>(_mapper.ConfigurationProvider), search).ToListAsync(),
                IsAdmin = _principal.IsAdmin()
            };

        }

        public async Task<IList<AllFormulaMeanTatDto>> GetFormulaMeanTatValue(Guid userId, bool isGlobal)
        {
            try
            {
                var resultset = await _repo.ExecuteSql<AllFormulaMeanTatDto>(_mapper,
                "uspGetFormulaTat @ISGLOBAL,@LOGINGUID",
                new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@ISGLOBAL", SqlDbType = SqlDbType.Bit, Value = isGlobal },
                    new SqlParameter { ParameterName = "@LOGINGUID", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId }
                }).ToListAsync();


                return resultset;
            }
            catch (Exception ex)
            {
                return null;
            }


        }

        private IQueryable<FormulaListingDto> GetPaging(IQueryable<FormulaListingDto> queryable, SearchFormulaDto search)
        {
            var query = queryable.OrderByDescending(o => o.IsStarred);
            switch (search.SortField)
            {
                case "name":
                    query = search.SortDirection == Ascending
                        ? query.ThenBy(o => o.Name)
                        : query.ThenByDescending(o => o.Name);
                    break;
                case "owner.fullName":
                    query = search.SortDirection == Ascending
                        ? query.ThenBy(o => o.Owner.FullName)
                        : query.ThenByDescending(o => o.Owner.FullName);
                    break;
                default:
                    query = search.SortDirection == Ascending
                        ? query.ThenBy(o => o.DateCreated)
                        : query.ThenByDescending(o => o.DateCreated);
                    break;
            }

            return query.Skip((search.Page - 1) * search.PerPage).Take(search.PerPage);
        }

        public async Task<SearchFormulaResultDto<FormulaSearchListingDto>> GetAllFormulas(SearchFormulaDto search)
        {
            var ownerGuid = _principal.GetOwnerId();
            var query = GetFormulaProjects(search)
                .Where(x => ((x.OwnerGuid == ownerGuid || (x.Owner.OwnerId == ownerGuid)) && !x.OriginalFormulaProjectId.HasValue)
                    || (x.ShareType == FormulaShareType.PublicLink && (_sellers.Ids.Contains(x.OwnerGuid) || (x.Owner.OwnerId.HasValue && _sellers.Ids.Contains(x.Owner.OwnerId.Value)))));
            var totalCount = query.Count();
            return new SearchFormulaResultDto<FormulaSearchListingDto>
            {
                TotalCount = totalCount,
                Formulas = await GetSearchPaging(query
                    .Select(s => new FormulaSearchListingDto
                    {
                        Id = s.Id,
                        DateCreated = s.DateCreated,
                        Name = s.Name,
                        OwnerGuid = s.OwnerGuid,
                        Owner = new Users.Dto.OwnerDto
                        {
                            FullName = s.Owner.Profile.FullName,
                            UserId = s.OwnerGuid
                        },
                        Categories = s.FormulaProjectCategories.Select(c => c.Category.Name).ToList(),
                        IsOwned = s.OwnerGuid == ownerGuid || s.Owner.OwnerId == ownerGuid ||
                            s.ChildFormulaProjects.Any(a => a.OwnerGuid == ownerGuid) ||
                            (s.OriginalFormulaProjectId.HasValue && s.OriginalFormulaProject.ChildFormulaProjects.Any(a => a.OwnerGuid == ownerGuid)),
                        IsStarred = s.IsStarred
                    }), search).ToListAsync()
            };
        }

        private IQueryable<FormulaSearchListingDto> GetSearchPaging(IQueryable<FormulaSearchListingDto> queryable, SearchFormulaDto search)
        {
            var query = queryable.OrderByDescending(o => o.IsStarred);
            switch (search.SortField)
            {
                case "name":
                    query = search.SortDirection == Ascending
                        ? query.ThenBy(o => o.Name)
                        : query.ThenByDescending(o => o.Name);
                    break;
                case "owner.fullName":
                    query = search.SortDirection == Ascending
                        ? query.ThenBy(o => o.Owner.FullName)
                        : query.ThenByDescending(o => o.Owner.FullName);
                    break;
                default:
                    query = search.SortDirection == Ascending
                        ? query.ThenBy(o => o.DateCreated)
                        : query.ThenByDescending(o => o.DateCreated);
                    break;
            }

            return query.Skip((search.Page - 1) * search.PerPage).Take(search.PerPage);
        }

        public async Task<List<FormulaListingDto>> GetFormulas()
        {
            var ownerGuid = _principal.GetOwnerId();

            return await _repo.Read<FormulaProject>()
                .Include(f => f.Owner)
                    .ThenInclude(f => f.Profile)
                .Include(f => f.OriginalFormulaProject)
                .Include(f => f.FormulaTasks)
                .Where(x => !x.IsDeleted) // TODO: use query filter
                .Where(x => x.OwnerGuid == ownerGuid || (x.Owner.OwnerId == ownerGuid))
                .ProjectTo<FormulaListingDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
        private IQueryable<FormulaProject> GetFormulaProjects(SearchFormulaDto search)
        {
            var query = _repo.Read<FormulaProject>()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(search.FilterSearch))
            {
                query = query.Where(w => w.Name.Contains(search.FilterSearch) ||
                    (w.Owner.OwnerId.HasValue
                        ? w.Owner.Owner.Profile.FullName.Contains(search.FilterSearch)
                        : w.Owner.Profile.FullName.Contains(search.FilterSearch)));
            }
            if (search.FilterCategorieIds != null)
                query = query.Where(w => w.FormulaProjectCategories.Any(a => search.FilterCategorieIds.Contains(a.CategoryId)));
            return query;
        }
        public async Task<FormulaDto> GetFormula(int formulaId)
        {
            var formula = await _repo.Read<FormulaProject>()
                .Include(i => i.FormulaProjectCategories)
                .Include(i => i.ResourceFormula)
                    .ThenInclude(i => i.Resource)
                .Where(w => w.Id == formulaId)
                .FirstOrDefaultAsync();

            if (formula == null)
                throw new ValidationException("Formula is not found.");

            return _mapper.Map<FormulaDto>(formula,
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.Formula)));
                });
        }

        public async Task<FormulaDto> AddFormula(AddFormulaDto data)
        {
            var ownerGuid = _principal.GetUserId();
            var userRole = _principal.GetUserRole();

            //var claims = _principal.Identities.Select(c =>c.Claims).ToList();
            //var user = await  _userManager.FindByIdAsync(ownerGuid.ToString());
            //var userRole = await _repo.Track<User>().Where(x => x.Id == ownerGuid)
            //        .Include(t => t.Roles)
            //            .ThenInclude(t => t.Role).Select(x => new
            //            {
            //                Role = x.Roles.Select(r => r.Role.Name).FirstOrDefault()
            //            }).FirstOrDefaultAsync();

            using (var trx = _repo.Transaction())
            {
                var formulaProject = _mapper.Map<FormulaProject>(data);
                formulaProject.OwnerGuid = ownerGuid;
                formulaProject.DateCreated = _dateTimeService.NowUtc;
                formulaProject.IsGlobal = userRole == UserRoles.Admin ? true : false;

                //Change FormulaProject Status
                formulaProject.Status = (int)FormulaProjectStatus.Draft;

                await trx.AddAsync(formulaProject);
                await trx.SaveAndCommitAsync(CancellationToken.None);

                var formula = await trx.Read<FormulaProject>()
                    .Include(p => p.Owner)
                        .ThenInclude(f => f.Profile)
                    .FirstOrDefaultAsync(w => w.Id == formulaProject.Id);
                await _formulaNeo4JRepository.AddFormulaAsync(formula.Id);
                return _mapper.Map<FormulaDto>(formula);
            }
        }

        public async Task<FormulaDto> UpdateFormula(FormulaDto data)
        {
            using (var trx = _repo.Transaction())
            {
                var formulaProject = await trx.Track<FormulaProject>()
                    .Include(f => f.OriginalFormulaProject)
                    .Include(i => i.Owner)
                    .ThenInclude(f => f.Profile)
                    .Include(i => i.FormulaProjectCategories)
                    .FirstOrDefaultAsync(x => x.Id == data.Id);

                if (formulaProject == null)
                    throw new ValidationException("Formula is not found.");
                if (formulaProject.Name != data.Name)
                {
                    await _entityFrameworkPlus.BulkUpdateAsync(trx.Track<FormulaTask>()
                        .Where(w => w.InternalFormulaProjectId == data.Id), f => new FormulaTask { Title = data.Name });

                    await _entityFrameworkPlus.BulkUpdateAsync(trx.Track<ProjectTask>()
                        .Where(w => w.FormulaId == data.Id), f => new ProjectTask { Title = data.Name });
                }
                _mapper.Map(data, formulaProject);
                formulaProject.LastUpdated = _dateTimeService.NowUtc;
                //Change FormulaProject Status
                formulaProject.Status = (int)FormulaProjectStatus.Draft;

                formulaProject.FormulaProjectCategories
                    .Where(w => !data.CategoryIds.Contains(w.CategoryId))
                    .ToList().ForEach(cat => formulaProject.FormulaProjectCategories.Remove(cat));
                data.CategoryIds.Where(cId => formulaProject.FormulaProjectCategories.All(a => a.CategoryId != cId))
                    .ToList().ForEach(cId => formulaProject.FormulaProjectCategories.Add(new FormulaProjectCategory
                    {
                        CategoryId = cId
                    }));
                await trx.SaveAndCommitAsync(CancellationToken.None);
                return _mapper.Map<FormulaDto>(formulaProject);
            }
        }

        public async Task DeleteFormula(int formulaId)
        {
            using (var trx = _repo.Transaction())
            {
                var formulaProject = await trx.Track<FormulaProject>()
                    .Include(x => x.FormulaTasks)
                        .ThenInclude(x => x.ChildFormulaTasks)
                    .Include(x => x.ChildFormulaProjects)
                    .Include(x => x.InternalFormulaTasks)
                    .Include(x => x.ProjectTasks)
                    .Include(x => x.ResourceFormula)
                        .ThenInclude(x => x.Resource)
                    .FirstOrDefaultAsync(p => p.Id == formulaId);

                if (formulaProject == null)
                    throw new ValidationException("Formula is not found.");

                if (formulaProject.InternalFormulaTasks.Any())
                    throw new ValidationException("Formula is used as subformula in another formula.");

                if (formulaProject.FormulaTasks.SelectMany(t => t.ChildFormulaTasks).Any())
                    throw new ValidationException("Formula tasks is used in another formula.");

                await _formulaTaskService.DeleteTasks(trx, formulaProject.FormulaTasks.Select(c => c.Id));
                var resources = formulaProject.ResourceFormula.Select(s => s.Resource).ToList();
                trx.RemoveRange(formulaProject.ResourceFormula);
                await trx.SaveChangesAsync();
                trx.RemoveRange(resources);

                await _storageService.DeleteFileAsync(formulaId.ToString(), StorageType.FormulaTask);
                if (formulaProject.ChildFormulaProjects.Any() ||
                    formulaProject.ProjectTasks.Any())
                {
                    formulaProject.IsDeleted = true;
                }
                else
                {
                    trx.Remove(formulaProject);
                }

                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task ImportTasksIntoProject(int projectId, ImportTasksDto model)
        {
            Dictionary<int, int> taskDict = new Dictionary<int, int>();
            Dictionary<int, int> rootTaskIds = new Dictionary<int, int>();
            Dictionary<int, int> nonRootTaskIds = new Dictionary<int, int>();

            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var project = await trx.Track<Domain.Models.Project.Project>()
                    .Include(i => i.Owner).ThenInclude(i => i.Business)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                    throw new ValidationException("Project is not found.");

                var formulaProject = await trx.Read<FormulaProject>()
                    .Include(x => x.FormulaTasks)
                        .ThenInclude(task => task.ChildTasks)
                    .Include(x => x.FormulaTasks)
                        .ThenInclude(task => task.InternalFormulaProject)
                    .Include(x => x.FormulaTasks)
                        .ThenInclude(task => task.RecurrenceOptions)
                    .Include(x => x.ResourceFormula)
                        .ThenInclude(task => task.Resource)
                    .FirstOrDefaultAsync(p => p.Id == model.FormulaId);

                if (formulaProject == null)
                    throw new ValidationException("Formula is not found.");

                await _projectService.CopyFormulaResourcesToProject(project, formulaProject);

                model.ProjectStartDates.ProjectStartDateTime = model.ProjectStartDates.ProjectStartDateTime ??
                                                               project.StartDate.ClampBottom(_dateTimeService.NowUtc);

                var tasksMaps = await _taskImportService.ImportTasksIntoProjectAsync(
                    trx,
                    project,
                    formulaProject.FormulaTasks.ToList(),
                    model.ProjectStartDates.ProjectStartDateTime.Value,
                    model.SkillMappings.Where(w => w.IsOutsorced).Select(s => s.SkillId),
                    model.SkillMappings);

                // Don't remove this check, as this method runs both for the Run Formula
                // and Import Fomrula, and for Run Formula Start Date and RootStartDateTime IS NULL
                var startDates = new Dictionary<int, DateTime?>();

                if (model.ProjectStartDates != null && model.ProjectStartDates.RootStartDateTime != null)
                {
                    startDates = model.ProjectStartDates.RootStartDateTime
                        .ToDictionary(
                            pair => tasksMaps.First(x => x.Value == pair.Key).Key,
                            pair => pair.Value);
                }

                model.ProjectStartDates.RootStartDateTime = startDates;
                model.ProjectStartDates.ProjectStartDateTime = model.ProjectStartDates.ProjectStartDateTime ??
                                                               project.StartDate.ClampBottom(_dateTimeService.NowUtc);

                var projectRoots = await _taskNeo4JRepository.GetProjectRootTaskIdsAsync(project.Id);

                // Check if any of the task is disabled


                var result = await _startDateHelperService.InitTasksStartDate(
                    trx,
                    project.Id,
                    model.ProjectStartDates,
                    projectRoots.Where(w => tasksMaps.ContainsKey(w)).ToList());

                await trx.SaveChangesAsync();
                await _taskImportService.ScheduleJobsAsync(trx, result.RootTasks);

                await trx.SaveAndCommitAsync(CancellationToken.None);
                transaction.Commit();

                taskDict = tasksMaps;

                // Reversing the Key - Value pair with FormulaTaskId and ProjectTaskId
                rootTaskIds = tasksMaps.Where(w => projectRoots.Contains(w.Key)).ToDictionary(k => k.Value, v => v.Key);
                nonRootTaskIds = tasksMaps.Where(w => !projectRoots.Contains(w.Key)).ToDictionary(k => k.Value, v => v.Key);
            }

            // Send the job invites to the vendors for rootTasks in a project, which gets started automatically
            var outsourcedRequestList = model.SkillMappings
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
            await UpdateProjectTaskVendorStatus(model.SkillMappings, nonRootTaskIds);
        }

        public async Task SetLockStatus(int formulaId, bool lockStatus)
        {
            using (var trx = _repo.Transaction())
            {
                var guid = _principal.GetUserId();
                var formulaProject = await trx.Track<FormulaProject>()
                    .Include(i => i.Owner)
                    .FirstOrDefaultAsync(p => p.Id == formulaId);

                if (formulaProject == null)
                    throw new ValidationException("Formula is not found.");

                if (formulaProject.OwnerGuid != guid &&
                    !(formulaProject.Owner.OwnerId.HasValue && formulaProject.Owner.OwnerId.Value == guid))
                    throw new ValidationException("Formula is owned by other owner.");

                formulaProject.IsLocked = lockStatus;

                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task<FormulaDto> ChangeFormulaStatus(int formulaId)
        {
            using (var trx = _repo.Transaction())
            {
                var formulaProject = await trx.Track<FormulaProject>()
                    .FirstOrDefaultAsync(p => p.Id == formulaId);

                if (formulaProject == null)
                    throw new ValidationException("Formula is not found.");

                var isVault = await trx.Read<FormulaProject>()
                    .FirstOrDefaultAsync(p => p.PublicVaultFormulaProjectID == formulaId);

                int status = isVault == null ? 2 : 4;

                formulaProject.Status = formulaProject.Status == 1 ? status : 1;

                await trx.SaveAndCommitAsync();

                return _mapper.Map<FormulaDto>(formulaProject);
            }
        }

        public async Task SetStarredStatus(int formulaId, bool starredStatus)
        {
            using (var trx = _repo.Transaction())
            {
                var formulaProject = await trx.Track<FormulaProject>()
                    .FirstOrDefaultAsync(p => p.Id == formulaId);

                if (formulaProject == null)
                    throw new ValidationException("Formula is not found.");

                formulaProject.IsStarred = starredStatus;

                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task<IList<CopyFormula>> CopyFormula(int formulaId, string formulaName, string description, bool isAdmin)
        {
            var ownerGuid = _principal.GetOwnerId();
            // Get the original formula
            // Keep these line above the ExecuteSql() methos, the db context closes
            var originalFormula = await _repo.Read<FormulaProject>()
                .Include(p => p.Owner)
                    .ThenInclude(f => f.Profile)
                .FirstOrDefaultAsync(w => w.Id == formulaId);

            var formulastatus = await _repo.ExecuteSql<CopyFormula>(_mapper, "usp_CopyFormula @FormulaID, @FormulaName, @FormulaDescription, @isGlobal, @ownerGuid",
                new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@FormulaID",
                        SqlDbType = SqlDbType.Int,
                        Value = formulaId
                    },
                    new SqlParameter
                    {
                        ParameterName = "@FormulaName",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = formulaName
                    },
                    new SqlParameter
                    {
                        ParameterName = "@FormulaDescription",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = String.IsNullOrEmpty(description) ? String.Empty : description
                    },
                    new SqlParameter
                    {
                        ParameterName = "@isGlobal",
                        SqlDbType = SqlDbType.Bit,
                        Value = isAdmin
                    }
                    ,
                    new SqlParameter
                    {
                        ParameterName = "@ownerGuid",
                        SqlDbType = SqlDbType.UniqueIdentifier,
                        Value = ownerGuid
                    }
                })
                .ToListAsync();

            foreach (var list in formulastatus)
            {
                CopyResources(list.OldTaskId, list.NewTaskId, list.NewFormulaID);
            }

            return formulastatus;
        }

        public async Task CopyResources(int oldTaskId, int newTaskId, int newFormulaID)
        {
            foreach (var res in await _formulaTaskNeo4JRepository.GetTaskResourcesAsync(oldTaskId))
            {
                var path = res.Type == (byte)ResourceType.Link ? res.Name : $"{newTaskId}/{res.Name}";
                if ((byte)ResourceType.File == res.Type)
                    await _storageService.CopyFileAsync(res.Path, path, StorageType.FormulaTask, StorageType.FormulaTask);


                await _formulaNeo4JRepository.AddFormulaAsync(newFormulaID);
                await _formulaTaskNeo4JRepository.AddTaskAsync(newTaskId, newFormulaID);
                await _formulaTaskNeo4JRepository.AddResourceToTaskAsync(newTaskId, _mapper.Map<TaskResourceNeo4jDto>(res, opts => opts.Items.Add("path", path)));

                //await _formulaTaskNeo4jRepository.AddResourceToTaskAsync(pair.Value.Id,
                //   _mapper.Map<TaskResourceNeo4jDto>(res, opts => opts.Items.Add("path", path)));
            }
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
