using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAutoM8.Domain.Models;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Skills.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IAutoM8.Service.ProjectTasks.Dto;

namespace IAutoM8.Service.CommonService
{
    public class ProjectTaskEntityImportService : IProjectTaskEntityImportService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IStorageService _storageService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IFormulaTaskNeo4jRepository _formulaTaskNeo4JRepository;
        private readonly IMapper _mapper;
        private readonly ClaimsPrincipal _principal;
        private readonly Regex _regex = new Regex("\\d+");

        public ProjectTaskEntityImportService(
             IDateTimeService dateTimeService,
             IStorageService storageService,
             ITaskNeo4jRepository taskNeo4JRepository,
             IFormulaTaskNeo4jRepository formulaTaskNeo4JRepository,
             IMapper mapper,
             ClaimsPrincipal principal)
        {
            _dateTimeService = dateTimeService;
            _storageService = storageService;
            _taskNeo4JRepository = taskNeo4JRepository;
            _formulaTaskNeo4JRepository = formulaTaskNeo4JRepository;
            _mapper = mapper;
            _principal = principal;
        }

        public async Task<Dictionary<int, int>> MapFormulaTaskAsync(ITransactionScope trx,
            Project project, FormulaTask formulaTask,
            DateTime startFrom,
            IEnumerable<int> outsourcesSkills,
            IEnumerable<SkillMapDto> skillMappings,
            int? parentTaskId,
            Func<ITransactionScope, Project, IEnumerable<FormulaTask>,
                DateTime, IEnumerable<int>, IEnumerable<SkillMapDto>, int?, (int x, int y), Task<Dictionary<int, int>>> importTasksIntoProjectAsync)
        {
            var tasksMaps = new Dictionary<int, int>();
            var userGuid = _principal.GetUserId();
            
            var formulaTaskNew = new ProjectTask
            {
                FormulaId = formulaTask.InternalFormulaProjectId,
                Title = formulaTask.InternalFormulaProject.Name,
                DateCreated = _dateTimeService.NowUtc,
                OwnerGuid = userGuid,
                ProjectId = project.Id,
                PosX = formulaTask.PosX,
                PosY = formulaTask.PosY,
                ParentTaskId = parentTaskId,
                FormulaTaskId = formulaTask.OriginalFormulaTaskId ?? formulaTask.Id,
                // Added DescNotificationFlag to show notification on Project page.
                DescNotificationFlag = formulaTask.DescNotificationFlag
            };

            await trx.AddAsync(formulaTaskNew);
            await trx.SaveChangesAsync();
            tasksMaps.Add(formulaTaskNew.Id, formulaTask.Id);

            if (parentTaskId.HasValue)
            {
                await _taskNeo4JRepository.AddFormulaTaskAsync(formulaTaskNew.Id, project.Id, parentTaskId.Value);
            }
            else
            {
                await _taskNeo4JRepository.AddTaskAsync(formulaTaskNew.Id, project.Id);
            }

            var tasks = await trx.Track<FormulaTask>()
                    .Include(x => x.ChildTasks)
                    .Include(x => x.RecurrenceOptions)
                    .Include(x => x.InternalFormulaProject)
                .Where(w => w.FormulaProjectId == formulaTask.InternalFormulaProjectId.Value)
                .ToListAsync();

            return tasksMaps.Concat(await importTasksIntoProjectAsync(trx, project, tasks,
                startFrom, outsourcesSkills, skillMappings, formulaTaskNew.Id, default((int, int))))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<(int, int)> MapTaskAsync(ITransactionScope trx, Project project, FormulaTask formulaTask,
            int? parentTaskId, (int x, int y) positionOffset,
            DateTime startFrom, IEnumerable<int> outsourcesSkills,
            IEnumerable<SkillMapDto> skillMappings)
        {
            var now = _dateTimeService.NowUtc;
            var userGuid = _principal.GetUserId();
            var task = _mapper.Map<ProjectTask>(formulaTask,
                opts =>
                {
                    opts.Items.Add("OwnerGuid", userGuid);
                    opts.Items.Add("NowUtc", now);
                    opts.Items.Add("ParentTaskId", parentTaskId);
                    opts.Items.Add("PositionOffset", positionOffset);
                    opts.Items.Add("SkillMappings", skillMappings);
                });


            //added notification flag to Project Task table
            task.DescNotificationFlag = formulaTask.DescNotificationFlag;

            
            if (!task.IsDisabled.HasValue || (task.IsDisabled.HasValue && !task.IsDisabled.Value))
            {
                RecalculateCron(task, startFrom);
                if (outsourcesSkills.Any(a => formulaTask.AssignedSkillId.HasValue &&
                    a == formulaTask.AssignedSkillId.Value))
                {
                    task.ProjectTaskVendors =
                        await trx.Read<FormulaTaskVendor>()
                            .Where(w => w.FormulaTaskId == formulaTask.Id && w.Status == FormulaRequestStatus.Accepted)
                            .ProjectTo<ProjectTaskVendor>(_mapper.ConfigurationProvider,
                            new { now }).ToListAsync();
                }
            }

            project.Tasks.Add(task);
            await trx.SaveChangesAsync();

            if (parentTaskId.HasValue)
                await _taskNeo4JRepository.AddFormulaTaskAsync(task.Id, project.Id, parentTaskId.Value);
            else
            {
                if (!task.IsDisabled.HasValue || (task.IsDisabled.HasValue && !task.IsDisabled.Value))
                    await _taskNeo4JRepository.AddTaskAsync(task.Id, project.Id);
            }

            if (!task.IsDisabled.HasValue || (task.IsDisabled.HasValue && !task.IsDisabled.Value))
            {
                foreach (var res in await _formulaTaskNeo4JRepository.GetTaskResourcesAsync(formulaTask.Id))
                {
                    var path = res.Type == (byte)ResourceType.Link ? res.Name : $"{task.Id}/{res.Name}";
                    if ((byte)ResourceType.File == res.Type)
                        await _storageService.CopyFileAsync(res.Path, path, StorageType.FormulaTask, StorageType.ProjectTask);
                    await _taskNeo4JRepository.AddResourceToTaskAsync(task.Id, _mapper.Map<TaskResourceNeo4jDto>(res, opts => opts.Items.Add("path", path)));
                }
            }
            
            return (formulaTask.Id, task.Id);
        }

        private void RecalculateCron(ProjectTask task, DateTime startFrom)
        {
            //this method should be sync with ~\clientapp\components\cron\parser.js\parsers and
            //~\clientapp\components\cron\parser.js\converters
            if (task.RecurrenceOptions != null)
            {
                var match = _regex.Matches(task.RecurrenceOptions.Cron);
                switch (task.RecurrenceOptions.CronTab)
                {
                    case CronTab.Minutes:
                        task.RecurrenceOptions.Cron = $"{startFrom.Minute}/{match[1].Value} * 1/1 * ? *";
                        break;
                    case CronTab.Hours:
                        if (match.Count == 5)
                            task.RecurrenceOptions.Cron = $"0 {startFrom.Hour}/{match[2].Value} 1/1 * ? *";
                        break;
                    case CronTab.Days:
                        if (match.Count == 4)
                            task.RecurrenceOptions.Cron = $"{match[0].Value} {match[1].Value} {startFrom.Day - task.RecurrenceOptions.DayDiff}/{match[3].Value} * ? *";
                        break;
                }
            }
        }
        public async Task MapDependencyAsync(ITransactionScope trx, Project project, FormulaTask formulaTask,
            Dictionary<int, int> tasksMaps)
        {
            foreach (var childFormulaTaskDependency in formulaTask.ChildTasks)
            {
                if (!tasksMaps.ContainsValue(childFormulaTaskDependency.ParentTaskId))
                    continue;

                var taskDependency = new ProjectTaskDependency
                {
                    ParentTaskId = tasksMaps.First(x => x.Value == childFormulaTaskDependency.ParentTaskId).Key,
                    ChildTaskId = tasksMaps.First(x => x.Value == childFormulaTaskDependency.ChildTaskId).Key
                };
                await trx.AddAsync(taskDependency);
                await trx.SaveChangesAsync();
                await _taskNeo4JRepository.AddTaskDependencyAsync(taskDependency.ParentTaskId, taskDependency.ChildTaskId);
            }
        }

        public async Task MapConditionAsync(ITransactionScope trx, Project project, FormulaTask formulaTask,
            KeyValuePair<int, int> pair, Dictionary<int, int> tasksMaps)
        {

            var formulaTaskCondition = await trx.Read<FormulaTaskCondition>()
                .Include(c => c.Options)
                .Where(c => c.Id == formulaTask.TaskConditionId)
                .SingleAsync();

            var projectTaskCondition = CreateCondition(trx, project.Tasks.First(t => t.Id == pair.Key), formulaTaskCondition);
            foreach (var formulaTaskConditionOption in formulaTaskCondition.Options)
            {
                var optionTargetId = formulaTaskConditionOption.AssignedTaskId.HasValue && tasksMaps.ContainsValue(formulaTaskConditionOption.AssignedTaskId.Value)
                    ? tasksMaps.First(x => x.Value == formulaTaskConditionOption.AssignedTaskId.Value).Key : (int?)null;
                var target = project.Tasks.FirstOrDefault(t => t.Id == optionTargetId);
                var option = await CreateConditionOption(trx, formulaTaskConditionOption, target);
                projectTaskCondition.Options.Add(option);
                await trx.SaveChangesAsync();
                if (optionTargetId.HasValue)
                {
                    await _taskNeo4JRepository.AddTaskConditionAsync(option.Id, projectTaskCondition.Task.Id, optionTargetId.Value);
                }
                target?.AssignedConditionOptions.Add(option);
            }
        }

        private ProjectTaskCondition CreateCondition(ITransactionScope trx, ProjectTask projectTask, FormulaTaskCondition condition)
        {
            projectTask.Condition = new ProjectTaskCondition
            {
                Condition = condition.Condition,
            };
            return projectTask.Condition;
        }

        private async Task<ProjectTaskConditionOption> CreateConditionOption(
            ITransactionScope trx,
            FormulaTaskConditionOption formulaTaskConditionOption,
            ProjectTask target)
        {
            var projectTaskConditionOption = new ProjectTaskConditionOption
            {
                AssignedTaskId = target?.Id,
                Option = formulaTaskConditionOption.Option
            };
            await trx.AddAsync(projectTaskConditionOption);
            return projectTaskConditionOption;
        }
    }
}
