using AutoMapper;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Projects.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks
{
    public class TaskFormulaService : ITaskFormulaService
    {
        private readonly IRepo _repo;
        private readonly ITaskImportService _taskImportService;
        private readonly ClaimsPrincipal _principal;
        private readonly ITaskStartDateHelperService _startDateHelperService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;

        public TaskFormulaService(
            ITaskImportService taskImportService,
            IRepo repo,
            ClaimsPrincipal principal,
            ITaskStartDateHelperService startDateHelperService,
            ITaskNeo4jRepository taskNeo4JRepository,
            IDateTimeService dateTimeService,
            IMapper mapper,
            IProjectService projectService)
        {
            _repo = repo;
            _principal = principal;
            _startDateHelperService = startDateHelperService;
            _taskImportService = taskImportService;
            _taskNeo4JRepository = taskNeo4JRepository;
            _dateTimeService = dateTimeService;
            _mapper = mapper;
            _projectService = projectService;
        }

        public async Task<IEnumerable<TaskDto>> CreateFormulaTaskAsync(TaskFormulaDto model)
        {
            var formulaTaskId = 0;
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            {
                var userGuid = _principal.GetUserId();
                using (var trx = _repo.Transaction())
                {
                    var project = await trx.Track<Domain.Models.Project.Project>()
                        .Include(i => i.Owner).ThenInclude(i => i.Business)
                        .FirstOrDefaultAsync(p => p.Id == model.ProjectId);

                    if (project == null)
                        throw new ValidationException("Project is not found.");

                    var formulaProject = await trx.Read<FormulaProject>()
                        .Include(x => x.FormulaTasks)
                            .ThenInclude(task => task.ChildTasks)
                        .Include(x => x.FormulaTasks)
                            .ThenInclude(task => task.RecurrenceOptions)
                        .Include(x => x.FormulaTasks)
                            .ThenInclude(x => x.InternalFormulaProject)
                        .Include(x => x.ResourceFormula)
                            .ThenInclude(task => task.Resource)
                        .FirstOrDefaultAsync(p => p.Id == model.FormulaId);

                    if (formulaProject == null)
                        throw new ValidationException("Formula is not found.");

                    await _projectService.CopyFormulaResourcesToProject(project, formulaProject);

                    var formulaTask = new ProjectTask
                    {
                        FormulaId = formulaProject.Id,
                        Title = formulaProject.Name,
                        DateCreated = _dateTimeService.NowUtc,
                        OwnerGuid = userGuid,
                        ProjectId = model.ProjectId,
                        PosX = model.PosX,
                        PosY = model.PosY
                    };

                    await trx.AddAsync(formulaTask);
                    await trx.SaveChangesAsync();
                    await _taskNeo4JRepository.AddTaskAsync(formulaTask.Id, formulaTask.ProjectId);

                    var tasksMaps = await _taskImportService.ImportTasksIntoProjectAsync(
                        trx,
                        project,
                        formulaProject.FormulaTasks,
                        model.StartDate,
                        model.SkillMappings.Where(w => w.IsOutsorced).Select(s => s.SkillId),
                        model.SkillMappings,
                        formulaTask.Id,
                        (model.PosX, model.PosY)
                    );

                    var result = await _startDateHelperService.InitTasksStartDate(
                        trx,
                        project.Id,
                        new ProjectStartDatesDto
                        {
                            ProjectStartDateTime = model.StartDate
                        },
                        await _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(formulaTask.Id)
                    );

                    formulaTask.StartDate = result.StartTime;
                    formulaTask.Duration = result.TotalDuration;

                    await trx.SaveChangesAsync();
                    await _taskImportService.ScheduleJobsAsync(trx, result.RootTasks);
                    await trx.SaveAndCommitAsync();
                    transaction.Commit();
                    formulaTaskId = formulaTask.Id;
                }
            }

            var tasks = await _repo.Read<ProjectTask>()
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
                .Where(c => c.Id == formulaTaskId || c.ParentTaskId == formulaTaskId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }
    }
}
