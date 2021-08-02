using AutoMapper;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Global;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks
{
    public class TaskImportService : ITaskImportService
    {
        private readonly IScheduleService _scheduleService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IProjectTaskEntityImportService _projectTaskEntityImportService;
        private readonly IRepo _repo;

        public TaskImportService(
            IScheduleService scheduleService,
            IDateTimeService dateTimeService,
            IRepo repo,
            IProjectTaskEntityImportService projectTaskEntityImportService)
        {
            _scheduleService = scheduleService;
            _dateTimeService = dateTimeService;
            _projectTaskEntityImportService = projectTaskEntityImportService;
            _repo = repo;
        }

        public async Task<Dictionary<int, int>> ImportTasksIntoProjectAsync(ITransactionScope trx,
            Project project,
            IEnumerable<FormulaTask> formulaTasks,
            DateTime startFrom,
            IEnumerable<int> outsourcesSkills,
            IEnumerable<SkillMapDto> skillMappings,
            int? parentTaskId = null,
            (int x, int y) positionOffset = default((int, int)))
        {
            var tasksMaps = new Dictionary<int, int>();
            foreach (var formulaTask in formulaTasks)
            {
                if (formulaTask.InternalFormulaProjectId.HasValue)
                {
                    tasksMaps = tasksMaps.Concat(await _projectTaskEntityImportService.MapFormulaTaskAsync(trx, project, formulaTask,
                                        startFrom, outsourcesSkills, skillMappings, parentTaskId, ImportTasksIntoProjectAsync))
                                    .ToDictionary(x => x.Key, x => x.Value);
                }
                else
                {
                    (int x, int y) map = await _projectTaskEntityImportService.MapTaskAsync(trx, project,
                        formulaTask, parentTaskId, positionOffset, startFrom, outsourcesSkills, skillMappings);
                    tasksMaps.Add(map.y, map.x);
                }
            }

            if (parentTaskId.HasValue)
            {
                var importedTasks = await trx.Track<ProjectTask>().Where(w => w.ParentTaskId == parentTaskId)
                    .ToListAsync();
                if (importedTasks.Any())
                {
                    var minX = importedTasks.Min(m => m.PosX);
                    var minY = importedTasks.Min(m => m.PosY);
                    foreach (var task in importedTasks)
                    {
                        task.PosY = task.PosY - minY + PositionMapConstants.PaddingTop;
                        task.PosX = task.PosX - minX + PositionMapConstants.PaddingLeft;
                    }
                }
            }

            ArrayList taskListArray = new ArrayList();

            //Create parent to child dependencies
            foreach (var pair in tasksMaps)
            {
                var formulaTask = formulaTasks.FirstOrDefault(x => x.Id == pair.Value);
                if (formulaTask == null) continue;
                if (!formulaTask.InternalFormulaProjectId.HasValue &&
                    formulaTask.TaskConditionId.HasValue)
                    await _projectTaskEntityImportService.MapConditionAsync(trx, project, formulaTask, pair, tasksMaps);
                else
                    await _projectTaskEntityImportService.MapDependencyAsync(trx, project, formulaTask, tasksMaps);

                // Add checklist from FormulaTaskChecklist to ProjectTaskChecklist
                var toDoChecklist = await _repo.Read<FormulaTaskChecklist>().Where(t => t.FormulaTaskId == pair.Value).ToListAsync();

                // In below foreach loop, it will add all FormulaTaskChecklist to ProjectTaskChecklist 
                foreach (var todo in toDoChecklist)
                {
                    await trx.AddAsync(new ProjectTaskChecklist()
                    {
                        Name = todo.Name,
                        ProjectTaskId = pair.Key,
                        Type = todo.Type,
                        DateCreated = _dateTimeService.NowUtc
                    });
                }

                taskListArray.Add(pair.Key);
            }


            // added feature to add Notes from formula to Project.
            for (int i = 0; i < taskListArray.Count; i++)
            {
                var formulaNotes = await _repo.Read<FormulaNote>().Where(t => t.FormulaId == formulaTasks.FirstOrDefault().FormulaProjectId).ToListAsync();
                foreach (var note in formulaNotes)
                {
                    await trx.AddAsync(new ProjectNote()
                    {
                        Text = note.Text,
                        DateCreated = _dateTimeService.NowUtc,
                        FormulaId = note.FormulaId,
                        ProjectId = project.Id,
                        ProjectTaskId = (int?)taskListArray[i],
                        IsPublished = false,//by default it will be false                    
                    });
                }
            }

            project.LastUpdated = _dateTimeService.NowUtc;

            await trx.SaveChangesAsync();
            return tasksMaps;
        }

        public async Task ScheduleJobsAsync(ITransactionScope trx, IEnumerable<ProjectTask> tasks)
        {
            foreach (var task in tasks)
            {
                await (task.IsAutomated || task.IsInterval
                    ? _scheduleService.CreateAutomatedJob(trx, task)
                    : _scheduleService.CreateJobBegin(trx, task));
            }
        }

    }
}
