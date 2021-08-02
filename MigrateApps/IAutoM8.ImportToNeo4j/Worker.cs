using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.ImportToNeo4j
{
    public class Worker : IWorker
    {
        private readonly IRepo _repo;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IFormulaTaskNeo4jRepository _formulaTaskNeo4JRepository;
        private readonly IFormulaNeo4jRepository _formulaNeo4JRepository;

        public Worker(
            IRepo repo,
            ITaskNeo4jRepository taskNeo4JRepository,
            IFormulaTaskNeo4jRepository formulaTaskNeo4JRepository,
            IFormulaNeo4jRepository formulaNeo4JRepository)
        {
            _repo = repo;
            _taskNeo4JRepository = taskNeo4JRepository;
            _formulaTaskNeo4JRepository = formulaTaskNeo4JRepository;
            _formulaNeo4JRepository = formulaNeo4JRepository;
        }

        public Task Do()
        {
            foreach (var projectId in _repo.Read<Project>().Select(s => s.Id).ToList())
            {
                var tasks = GetProjectTasks(projectId);
                var dependencies = GetProjectTasksDependencies(projectId);
                var conditions = GetProjectTasksConditions(projectId);
                _taskNeo4JRepository.AddTasksWithResourcesAsync(tasks, dependencies, conditions).Wait();
            }
            foreach (var formulaId in _repo.Read<FormulaProject>().Select(s => s.Id).ToList())
            {
                var tasks = GetFormulaTasks(formulaId);
                var dependencies = GetFormulaTasksDependencies(formulaId);
                var conditions = GetFormulaTasksConditions(formulaId);
                _formulaTaskNeo4JRepository.AddTasksWithResourcesAsync(tasks, dependencies, conditions).Wait();
                _formulaNeo4JRepository.AddFormulaAsync(formulaId).Wait();
            }
            return Task.CompletedTask;
        }

        private List<TaskConditionNeo4jDto> GetFormulaTasksConditions(int formulaId)
        {
            return _repo.Read<FormulaTask>()
                .Include(i => i.Condition)
                .ThenInclude(i => i.Options)
                .Where(w => w.FormulaProjectId == formulaId)
                .SelectMany(sm => sm.Condition.Options
                    .Where(w => w.AssignedTaskId.HasValue)
                    .Select(s => new TaskConditionNeo4jDto
                    {
                        Id = s.Id,
                        ConditionTaskId = sm.Id,
                        TaskId = s.AssignedTaskId.Value
                    })
                ).ToList();
        }

        private List<TaskDependencyNeo4jDto> GetFormulaTasksDependencies(int formulaId)
        {
            return _repo.Read<FormulaTaskDependency>()
                .Include(i => i.ParentTask)
                .Where(w => w.ParentTask.FormulaProjectId == formulaId)
                .Select(s => new TaskDependencyNeo4jDto
                {
                    ParentTaskId = s.ParentTaskId,
                    ChildTaskId = s.ChildTaskId
                }).ToList();
        }

        private List<ImportTaskNeo4jDto> GetFormulaTasks(int formulaId)
        {
            return _repo.Read<FormulaTask>()
                .Include(i => i.ResourceFormulaTask)
                .ThenInclude(i => i.Resource)
                .Where(w => w.FormulaProjectId == formulaId)
                .Select(s => new ImportTaskNeo4jDto
                {
                    Id = s.Id,
                    ProjectId = s.FormulaProjectId,
                    Resoruces = s.ResourceFormulaTask.Select(sr => new ResourceNeo4jDto
                    {
                        Type = (byte)sr.Resource.Type,
                        Size = sr.Resource.Size,
                        IsShared = s.IsShareResources,
                        Mime = sr.Resource.Mime,
                        Name = sr.Resource.Name,
                        Path = sr.Resource.Path
                    }).ToList()
                }).ToList();
        }

        private List<TaskConditionNeo4jDto> GetProjectTasksConditions(int projectId)
        {
            return _repo.Read<ProjectTask>()
                .Include(i => i.Condition)
                .ThenInclude(i => i.Options)
                .Where(w => w.ProjectId == projectId)
                .SelectMany(sm => sm.Condition.Options
                    .Where(w => w.AssignedTaskId.HasValue)
                    .Select(s => new TaskConditionNeo4jDto
                    {
                        Id = s.Id,
                        ConditionTaskId = sm.Id,
                        TaskId = s.AssignedTaskId.Value
                    })
                ).ToList();
        }

        private List<TaskDependencyNeo4jDto> GetProjectTasksDependencies(int projectId)
        {
            return _repo.Read<ProjectTaskDependency>()
                .Include(i => i.ParentTask)
                .Where(w => w.ParentTask.ProjectId == projectId)
                .Select(s => new TaskDependencyNeo4jDto
                {
                    ParentTaskId = s.ParentTaskId,
                    ChildTaskId = s.ChildTaskId
                }).ToList();
        }

        private List<ImportTaskNeo4jDto> GetProjectTasks(int projectId)
        {
            return _repo.Read<ProjectTask>()
                .Include(i => i.ResourceProjectTask)
                .ThenInclude(i => i.Resource)
                .Where(w => w.ProjectId == projectId)
                .Select(s => new ImportTaskNeo4jDto
                {
                    Id = s.Id,
                    ProjectId = s.ProjectId,
                    Status = (byte)s.Status,
                    Resoruces = s.ResourceProjectTask.Select(sr => new ResourceNeo4jDto
                    {
                        Type = (byte)sr.Resource.Type,
                        Size = sr.Resource.Size,
                        IsShared = s.IsShareResources,
                        Mime = sr.Resource.Mime,
                        Name = sr.Resource.Name,
                        Path = sr.Resource.Path
                    }).ToList()
                }).ToList();
        }
    }
}
