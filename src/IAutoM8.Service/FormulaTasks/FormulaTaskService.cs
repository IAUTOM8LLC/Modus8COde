using AutoMapper;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Exceptions;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.FormulaTasks.Dto;
using IAutoM8.Service.FormulaTasks.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Resources.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IAutoM8.Domain.Models;
using IAutoM8.Global.Utils;
using IAutoM8.Domain.Models.User;

namespace IAutoM8.Service.FormulaTasks
{
    public class FormulaTaskService : IFormulaTaskService
    {
        private readonly IRepo _repo;
        private readonly ClaimsPrincipal _principal;
        private readonly IStorageService _storageService;
        private readonly IFormulaTaskNeo4jRepository _formulaTaskNeo4jRepository;
        private readonly IFormulaNeo4jRepository _formulaNeo4JRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IMapper _mapper;

        public FormulaTaskService(
            IRepo repo,
            ClaimsPrincipal principal,
            IStorageService storageService,
            IFormulaTaskNeo4jRepository formulaTaskNeo4jRepository,
            IFormulaNeo4jRepository formulaNeo4JRepository,
            IDateTimeService dateTimeService,
            IMapper mapper)
        {
            _repo = repo;
            _principal = principal;
            _storageService = storageService;
            _formulaTaskNeo4jRepository = formulaTaskNeo4jRepository;
            _formulaNeo4JRepository = formulaNeo4JRepository;
            _dateTimeService = dateTimeService;
            _mapper = mapper;
        }

        public async Task<bool> GetFormulaLockStatus(int formulaId)
        {
            return !await _repo.Read<FormulaProject>()
                .Where(c => c.Id == formulaId && c.IsLocked)
                .AnyAsync();
        }

        public async Task<List<FormulaTaskDto>> GetTasksAsync(int formulaProjectId)
        {
            //var ownerId = _principal.GetOwnerId();
            //var hasAccess = await _repo
            //    .Read<FormulaProject>()
            //    .AnyAsync(f =>
            //        f.Id == formulaProjectId
            //        && (f.OwnerGuid == ownerId || f.Owner.OwnerId == ownerId)
            //    );

            //if (!hasAccess)
            //    throw new ForbiddenException(shouldRedirect: true);

            var tasks = await _repo.Read<FormulaTask>()
                .Include(c => c.ChildTasks)
                .Include(c => c.ParentTasks)
                .Include(c => c.RecurrenceOptions)
                .Include(c => c.Condition)
                .ThenInclude(c => c.Options)
                .Where(c => c.FormulaProjectId == formulaProjectId)
                .ToListAsync();

            return tasks.Select(Mapper.Map<FormulaTaskDto>).ToList();
        }

        public async Task<int> GetGroupTaskCount(int groupTaskId)
        {
            return await _repo.Read<FormulaTask>()
                .Include(t => t.InternalFormulaProject)
                .ThenInclude(t => t.FormulaTasks)
                .Where(t => t.Id == groupTaskId)
                .Select(t => t.InternalFormulaProject.FormulaTasks.Count)
                .FirstOrDefaultAsync();
        }

        public async Task<FormulaTaskDto> GetTaskAsync(int taskId, int formulaId)
        {
            var task = await _repo.Read<FormulaTask>()
                .Include(i => i.AssignedConditionOptions)
                .Include(c => c.ChildTasks)
                .Include(c => c.ParentTasks)
                .Include(c => c.RecurrenceOptions)
                .Include(c => c.Condition).ThenInclude(t => t.Options)
                .Where(c => c.Id == taskId)
                .FirstOrDefaultAsync();

            if (task == null)
                return null;

            var mappedTask = _mapper.Map<FormulaTaskDto>(task);
            mappedTask.Resources = _mapper.Map<List<ResourceDto>>(await _formulaTaskNeo4jRepository
                .GetTaskAndSharedResourcesAsync(task.Id, formulaId),
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.FormulaTask)));
                });

            var projectResources = await _repo.Read<ResourceFormula>()
                .Include(c => c.Resource)
                .Where(c => c.FormulaId == mappedTask.FormulaProjectId)
                .Select(s => s.Resource)
                .ToListAsync();
            var mappedProjectResources = _mapper.Map<List<ResourceDto>>(projectResources,
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.Formula)));
                    opts.Items.Add("isShared", true);
                });

            mappedTask.Resources.AddRange(mappedProjectResources);

            // Commenting the check, as Client wants the Training tab to be shown in all cases, irrespective of the Public or Custom Formula
            // Keep the check commented and ShowTrainingTab = true till further update from client.
            // Code changes done after discussion with the manager, Dated: Nov 30, 2020
            var isVendor = _principal.IsVendor();
            //if (!isVendor && task.PublicVaultFormulaTaskID != null)
            //{
            //    mappedTask.ShowTrainingTab = false;
            //}
            //else
            //{
            //    mappedTask.ShowTrainingTab = true;
            //}

            if (task.IsTrainingLocked && !isVendor && !_principal.IsAdmin())
            {
                mappedTask.ShowTrainingTab = false;
            }
            else
            {
                mappedTask.ShowTrainingTab = true;
            }

            mappedTask.FormulaTaskChecklists = await GetFormulaTaskCheckListAsync(taskId);

            return mappedTask;
        }

        public async Task<FormulaTaskDto> AddTaskAsync(UpdateFormulaTaskDto model)
        {
            if (model == null)
                throw new ArgumentException(nameof(model));
            var ownerGuid = _principal.GetUserId();
            var userRole = await _repo.Track<User>().Where(x => x.Id == ownerGuid)
                    .Include(t => t.Roles)
                        .ThenInclude(t => t.Role).Select(x => new
                        {
                            Role = x.Roles.Select(r => r.Role.Name).FirstOrDefault()
                        }).FirstOrDefaultAsync();
            using (var transaction = _formulaTaskNeo4jRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                if (await trx.Read<FormulaProject>().Where(w => w.Id == model.FormulaProjectId && w.IsLocked)
                    .AnyAsync())
                    throw new ValidationException("Formula is locked.");
                var task = Mapper.Map<FormulaTask>(model);
                task.OwnerGuid = _principal.GetUserId();
                task.DateCreated = _dateTimeService.NowUtc;
                task.IsGlobal = (userRole != null && userRole.Role == "Admin") ? true : false;
                await trx.AddAsync(task);
                await trx.SaveChangesAsync();
                await _formulaTaskNeo4jRepository.AddTaskAsync(task.Id, task.FormulaProjectId);
                //await trx.SaveAndCommitAsync();

                await AddCheckListItemsAsync(trx, model, task);

                //Change FormulaProject Status
                var formulaproject = await trx.Track<FormulaProject>()
                    .FirstOrDefaultAsync(x => x.Id == model.FormulaProjectId);
                formulaproject.Status = (int)FormulaProjectStatus.Draft;

                await trx.SaveAndCommitAsync();

                var res = await trx.Track<FormulaTask>()
                    .Include(c => c.ChildTasks)
                    .Include(c => c.ParentTasks)
                    .Include(c => c.RecurrenceOptions)
                    .Include(c => c.Condition).ThenInclude(t => t.Options)
                    .FirstOrDefaultAsync(x => x.Id == task.Id);

                if (res.TaskConditionId.HasValue)
                {
                    var tasks = from condition in res.Condition.Options.Where(w => w.AssignedTaskId.HasValue)
                                select _formulaTaskNeo4jRepository.AddTaskConditionAsync(condition.Id, task.Id, condition.AssignedTaskId.Value);
                    await Task.WhenAll(tasks);
                }

                transaction.Commit();

                var result = Mapper.Map<FormulaTaskDto>(res);
                // This can be removed, after adding the navigation properties in domain classes
                result.FormulaTaskChecklists = await GetFormulaTaskCheckListAsync(task.Id);

                return result;
            }
        }

        public async Task<FormulaTaskDto> AddFormulaTaskAsync(AddFormulaTaskDto model)
        {
            if (model == null)
                throw new ArgumentException(nameof(model));

            var taskId = 0;
            using (var transaction = _formulaTaskNeo4jRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                if (await trx.Read<FormulaProject>().Where(w => w.Id == model.FormulaProjectId && w.IsLocked)
                    .AnyAsync())
                    throw new ValidationException("Formula is locked.");
                var task = Mapper.Map<FormulaTask>(model);
                task.OwnerGuid = _principal.GetUserId();
                task.DateCreated = _dateTimeService.NowUtc;
                task.Title = await trx.Read<FormulaProject>()
                    .Where(w => w.Id == model.InternalFormulaProjectId)
                    .Select(s => s.Name)
                    .FirstAsync();

                await trx.AddAsync(task);
                await trx.SaveChangesAsync();

                await _formulaTaskNeo4jRepository.AddTaskAsync(task.Id, task.FormulaProjectId);
                await _formulaNeo4JRepository.AddRelationAsync(model.FormulaProjectId, model.InternalFormulaProjectId);

                if (await _formulaNeo4JRepository.HasLoopAsync(model.FormulaProjectId))
                    throw new ValidationException("Try to add recursive formula.");

                await trx.SaveAndCommitAsync();
                taskId = task.Id;
                transaction.Commit();
            }

            var res = await _repo.Read<FormulaTask>().FirstOrDefaultAsync(x => x.Id == taskId);
            return Mapper.Map<FormulaTaskDto>(res);
        }

        public async Task<FormulaTaskDto> UpdateTaskAsync(UpdateFormulaTaskDto model)
        {

            var formula = _repo.Read<FormulaProject>()
                .Where(t => t.Id == model.FormulaProjectId)
                .FirstOrDefault();

            if (formula != null && !_principal.IsAdmin() && formula.IsGlobal)
            {
                return null;
            }
            else
            {
                using (var trx = _repo.Transaction())
                {
                    if (await trx.Read<FormulaProject>().Where(w => w.Id == model.FormulaProjectId && w.IsLocked)
                        .AnyAsync())
                        throw new ValidationException("Formula is locked.");
                    var task = await trx.Track<FormulaTask>()
                        .Include(c => c.ChildTasks)
                        .Include(c => c.ParentTasks)
                        .Include(c => c.RecurrenceOptions)
                        .Include(c => c.Condition).ThenInclude(t => t.Options)
                        .FirstOrDefaultAsync(x => x.Id == model.Id);

                    if (task == null)
                        throw new ValidationException("Formula task not found.");
                    if (task.Description != null)
                    {
                        int diffCount = Difference.DiffText(task.Description, model.Description).Length;
                        if (diffCount > 0)
                            task.DescNotificationFlag = true;
                    }
                    if (task.Description == null && model.Description != null) task.DescNotificationFlag = true;


                    // Deleting all the checklist based on the FormulaTaskId
                    var todos = await trx.Track<FormulaTaskChecklist>()
                            .Where(p => p.FormulaTaskId == task.Id)
                            .ToListAsync();

                    trx.RemoveRange(todos);

                    // Insert the CheckList items
                    await AddCheckListItemsAsync(trx, model, task);

                    //Change FormulaProject Status
                    var formulaproject = await trx.Track<FormulaProject>()
                        .FirstOrDefaultAsync(x => x.Id == model.FormulaProjectId);
                    formulaproject.Status = (int)FormulaProjectStatus.Draft;

                    Mapper.Map(model, task);
                    task.LastUpdated = _dateTimeService.NowUtc;
                    await trx.SaveAndCommitAsync();

                    var result = Mapper.Map<FormulaTaskDto>(task);
                    // This can be removed, after adding the navigation properties in domain classes
                    result.FormulaTaskChecklists = await GetFormulaTaskCheckListAsync(task.Id);

                    return result;
                }
            }
        }

        public async Task UpdateTasksPositionAsync(List<TaskPositionDto> list)
        {
            if (list?.Count == 0)
                return;

            using (var trx = _repo.Transaction())
            {
                var taskIds = list.Select(s => s.Id).ToList();
                if (await trx.Read<FormulaTask>()
                    .Include(i => i.FormulaProject)
                    .Where(w => taskIds.Contains(w.Id) && w.FormulaProject.IsLocked)
                    .AnyAsync())
                    throw new ValidationException("Formula is locked.");
                var tasks = trx.Track<FormulaTask>().Where(c => taskIds.Contains(c.Id));
                foreach (var task in tasks)
                {
                    var model = list.FirstOrDefault(c => c.Id == task.Id);
                    if (model == null)
                        continue;
                    task.PosX = model.PosX;
                    task.PosY = model.PosY;
                }

                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task DeleteTask(int taskId)
        {
            using (var transaction = _formulaTaskNeo4jRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                if (await trx.Read<FormulaTask>()
                    .Include(i => i.FormulaProject)
                    .Where(w => w.Id == taskId && w.FormulaProject.IsLocked)
                    .AnyAsync())
                    throw new ValidationException("Formula is locked.");
                var task = trx.Track<FormulaTask>()
                    .Include(c => c.ProjectTasks)
                    .Include(c => c.ChildTasks)
                    .Include(c => c.ChildFormulaTasks)
                    .Include(c => c.ParentTasks)
                    .Include(c => c.ResourceFormulaTask)
                        .ThenInclude(c => c.Resource)
                    .FirstOrDefault(p => p.Id == taskId);

                if (task == null)
                    throw new ValidationException("Formula is not found.");

                if (task.ChildFormulaTasks.Any())
                    throw new ValidationException("Formula task is used in another formula.");

                if (task.InternalFormulaProjectId.HasValue &&
                    !await trx.Track<FormulaTask>()
                    .AnyAsync(p => p.InternalFormulaProjectId == task.InternalFormulaProjectId && p.Id != task.Id))
                {
                    await _formulaNeo4JRepository.RemoveRelationAsync(task.FormulaProjectId,
                        task.InternalFormulaProjectId.Value);
                }
                var resources = task.ResourceFormulaTask.Select(s => s.Resource).ToList();
                await _storageService.DeleteFileAsync(taskId.ToString(), StorageType.FormulaTask);
                foreach (var projectTask in task.ProjectTasks)
                {
                    projectTask.FormulaTaskId = null;
                }
                var todos = await trx.Track<FormulaTaskChecklist>()
                    .Where(t => t.FormulaTaskId == taskId)
                    .ToListAsync();
                trx.RemoveRange(task.ResourceFormulaTask);
                await _formulaTaskNeo4jRepository.DeleteTaskAsync(taskId);
                await trx.SaveChangesAsync();
                trx.RemoveRange(todos);
                await trx.SaveChangesAsync();
                trx.RemoveRange(resources);
                trx.RemoveRange(task.ChildTasks);
                trx.RemoveRange(task.ParentTasks);
                trx.Remove(task);
                await trx.SaveAndCommitAsync();
                transaction.Commit();
            }
        }

        public async Task DeleteTasks(ITransactionScope trx, IEnumerable<int> taskIds)
        {
            var tasks = await trx.Track<FormulaTask>()
                    .Include(c => c.ProjectTasks)
                    .Include(c => c.ChildTasks)
                    .Include(c => c.ParentTasks)
                    .Include(c => c.ResourceFormulaTask)
                        .ThenInclude(c => c.Resource)
                    .Where(p => taskIds.Contains(p.Id))
                    .ToListAsync();
            foreach (var projectTask in tasks.SelectMany(s => s.ProjectTasks))
            {
                projectTask.FormulaTaskId = null;
            }

            await trx.SaveChangesAsync();
            var resources = tasks.SelectMany(sm => sm.ResourceFormulaTask.Select(s => s.Resource).ToList()).ToList();
            trx.RemoveRange(tasks.SelectMany(sm => sm.ResourceFormulaTask));
            await trx.SaveChangesAsync();
            trx.RemoveRange(resources);
            foreach (var task in tasks)
            {
                if (task.InternalFormulaProjectId.HasValue)
                {
                    await _formulaNeo4JRepository.RemoveRelationAsync(task.FormulaProjectId,
                        task.InternalFormulaProjectId.Value);
                }
            }
            foreach (var taskId in taskIds)
            {
                await _storageService.DeleteFileAsync(taskId.ToString(), StorageType.FormulaTask);
            }

            var todos = await trx.Track<FormulaTaskChecklist>()
                .Where(p => taskIds.Contains(p.FormulaTaskId.Value))
                .ToListAsync();

            trx.RemoveRange(todos);
            await trx.SaveChangesAsync();

            trx.RemoveRange(tasks.SelectMany(c => c.ChildTasks));
            trx.RemoveRange(tasks.SelectMany(c => c.ParentTasks));
            trx.RemoveRange(tasks);
        }

        public async Task CreateTaskDependency(
            ITransactionScope trx,
            FormulaTask parentTask,
            FormulaTask childTask,
            bool isRequired)
        {
            var taskDependency = new FormulaTaskDependency
            {
                Required = isRequired,
                ParentTask = parentTask,
                ChildTask = childTask
            };
            await trx.AddAsync(taskDependency);
        }

        public async Task AssignTaskToConditionOption(int conditionOptionId, int? taskId)
        {
            using (var trx = _repo.Transaction())
            {
                var opt = trx.Track<FormulaTaskConditionOption>()
                    .Include(i => i.Condition)
                    .ThenInclude(i => i.Task)
                    .FirstOrDefault(t => t.Id == conditionOptionId);

                if (opt != null)
                {
                    var assignedTaskId = opt.AssignedTaskId;
                    opt.AssignedTaskId = taskId;
                    if (taskId.HasValue)
                    {
                        if (await trx.Read<FormulaTask>()
                            .Include(i => i.FormulaProject)
                            .Where(w => w.Id == taskId && w.FormulaProject.IsLocked)
                            .AnyAsync())
                            throw new ValidationException("Formula is locked.");
                        if (opt.Condition.Task.FormulaProjectId !=
                            await trx.Read<FormulaTask>().Where(w => w.Id == taskId.Value)
                            .Select(s => s.FormulaProjectId).FirstOrDefaultAsync())
                        {
                            throw new ValidationException("Connection between this tasks is not allowed.");
                        }
                        await _formulaTaskNeo4jRepository.AddTaskConditionAsync(opt.Id, opt.Condition.Task.Id, taskId.Value);
                    }
                    else if (assignedTaskId.HasValue)
                    {
                        if (await trx.Read<FormulaTask>()
                            .Include(i => i.FormulaProject)
                            .Where(w => w.Id == assignedTaskId && w.FormulaProject.IsLocked)
                            .AnyAsync())
                            throw new ValidationException("Formula is locked.");
                        await _formulaTaskNeo4jRepository.RemoveTaskConditionAsync(opt.Condition.Task.Id, assignedTaskId.Value);
                    }
                    await trx.SaveAndCommitAsync();
                }
            }
        }

        public async Task<FormulaTask> CreateTask(
            ITransactionScope trx,
            FormulaTask formulaTask,
            Guid userGuid,
            List<Skill> bussinessSkills,
            int formulaId,
            int? internalFormulaId)
        {
            var task = _mapper.Map<FormulaTask>(formulaTask, opts =>
             {
                 opts.Items.Add("OwnerGuid", userGuid);
                 opts.Items.Add("DateCreated", _dateTimeService.NowUtc);
                 opts.Items.Add("FormulaProjectId", formulaId);
                 opts.Items.Add("InternalFormulaProjectId", internalFormulaId);
             });

            task.FormulaTaskVendors = task.FormulaTaskVendors.Where(w => w.Status == FormulaRequestStatus.Accepted).ToList();

            var assignedSkill = CopyOrGetSkill(formulaTask.AssignedSkill, bussinessSkills);
            if (assignedSkill != null)
            {
                if (assignedSkill.Id > 0)
                {
                    task.AssignedSkillId = assignedSkill.Id;
                }
                else
                {
                    bussinessSkills.Add(assignedSkill);
                    task.AssignedSkill = assignedSkill;
                }
            }

            var reviewSkill = CopyOrGetSkill(formulaTask.ReviewingSkill, bussinessSkills);
            if (reviewSkill != null)
            {
                if (reviewSkill.Id > 0)
                {
                    task.ReviewingSkillId = reviewSkill.Id;
                }
                else
                {
                    bussinessSkills.Add(reviewSkill);
                    task.ReviewingSkill = reviewSkill;
                }
            }

            if (formulaTask.RecurrenceOptions != null)
            {
                task.RecurrenceOptions = _mapper.Map<RecurrenceOptions>(formulaTask.RecurrenceOptions);
            }

            task.OriginalFormulaTaskId = formulaTask.OriginalFormulaTaskId ?? formulaTask.Id;

            await trx.AddAsync(task);
            await trx.SaveChangesAsync();
            await _formulaTaskNeo4jRepository.AddTaskAsync(task.Id, task.FormulaProjectId);
            return task;
        }

        public async Task<FormulaTaskCondition> CreateCondition(
            ITransactionScope trx,
            FormulaTask formulaTask,
            FormulaTaskCondition formulaTaskCondition)
        {
            var condition = new FormulaTaskCondition
            {
                Condition = formulaTaskCondition.Condition
            };
            await trx.AddAsync(condition);
            formulaTask.Condition = condition;
            return condition;
        }

        public async Task<FormulaTaskConditionOption> CreateConditionOption(
            ITransactionScope trx,
            FormulaTaskConditionOption formulaTaskConditionOption,
            FormulaTask optionTarget)
        {
            var conditionOption = new FormulaTaskConditionOption
            {
                AssignedTask = optionTarget,
                Option = formulaTaskConditionOption.Option
            };
            await trx.AddAsync(conditionOption);
            return conditionOption;
        }

        public async Task AddTaskDependency(FormulaTaskDependencyDto model)
        {
            using (var trx = _repo.Transaction())
            {
                if (await trx.Read<FormulaTask>()
                    .Include(i => i.FormulaProject)
                    .Where(w => w.Id == model.ParentTaskId && w.FormulaProject.IsLocked)
                    .AnyAsync())
                    throw new ValidationException("Formula is locked.");
                var dependency = trx.Track<FormulaTaskDependency>()
                    .FirstOrDefault(p =>
                        p.ChildTaskId == model.ChildTaskId
                        && p.ParentTaskId == model.ParentTaskId);

                if (dependency == null)
                {
                    dependency = Mapper.Map<FormulaTaskDependency>(model);
                    await trx.AddAsync(dependency);

                    if (await trx.Read<FormulaTask>().Where(w => w.Id == model.ChildTaskId)
                        .Select(s => s.FormulaProjectId).FirstOrDefaultAsync() !=
                        await trx.Read<FormulaTask>().Where(w => w.Id == model.ParentTaskId)
                        .Select(s => s.FormulaProjectId).FirstOrDefaultAsync())
                    {
                        throw new ValidationException("Connection between this tasks is not allowed.");
                    }
                }
                else
                {
                    Mapper.Map(model, dependency);
                }
                await _formulaTaskNeo4jRepository.AddTaskDependencyAsync(model.ParentTaskId, model.ChildTaskId);
                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task RemoveTaskDependency(FormulaTaskDependencyDto model)
        {
            using (var trx = _repo.Transaction())
            {
                if (await trx.Read<FormulaTask>()
                    .Include(i => i.FormulaProject)
                    .Where(w => w.Id == model.ParentTaskId && w.FormulaProject.IsLocked)
                    .AnyAsync())
                    throw new ValidationException("Formula is locked.");
                var dependency = trx.Track<FormulaTaskDependency>()
                    .FirstOrDefault(p =>
                        p.ChildTaskId == model.ChildTaskId
                        && p.ParentTaskId == model.ParentTaskId);

                trx.Remove(dependency);
                await _formulaTaskNeo4jRepository.RemoveTaskDependencyAsync(model.ParentTaskId, model.ChildTaskId);
                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        private Skill CopyOrGetSkill(Skill skill, IEnumerable<Skill> existingSkills)
        {
            if (skill == null)
                return null;

            var existingSkill = existingSkills.FirstOrDefault(t =>
                string.Equals(t.Name, skill.Name, StringComparison.CurrentCultureIgnoreCase));

            if (existingSkill != null)
                return existingSkill;

            var newSkill = _mapper.Map<Skill, Skill>(skill);
            newSkill.OwnerGuid = _principal.GetOwnerId();
            return newSkill;
        }

        public async Task CopyResources(KeyValuePair<FormulaTask, FormulaTask> pair)
        {

            foreach (var res in await _formulaTaskNeo4jRepository.GetTaskResourcesAsync(pair.Key.Id))
            {
                var path = res.Type == (byte)ResourceType.Link ? res.Name : $"{pair.Value.Id}/{res.Name}";
                if ((byte)ResourceType.File == res.Type)
                    await _storageService.CopyFileAsync(res.Path, path, StorageType.FormulaTask, StorageType.FormulaTask);
                await _formulaTaskNeo4jRepository.AddResourceToTaskAsync(pair.Value.Id,
                    _mapper.Map<TaskResourceNeo4jDto>(res, opts => opts.Items.Add("path", path)));
            }
        }

        private async Task<List<FormulaTaskChecklistDto>> GetFormulaTaskCheckListAsync(int taskId)
        {
            var todos = await _repo.Read<FormulaTaskChecklist>()
                .Where(t => t.FormulaTaskId == taskId)
                .ToListAsync();

            return todos.Select(Mapper.Map<FormulaTaskChecklistDto>).ToList();
        }

        private async Task AddCheckListItemsAsync(ITransactionScope trx, UpdateFormulaTaskDto model, FormulaTask task)
        {
            // Add the Todo Checklist
            if (model.AddTodoCheckList != null && model.AddTodoCheckList.Milestones.Count > 0)
            {
                foreach (var milestone in model.AddTodoCheckList.Milestones)
                {
                    await trx.AddAsync(new FormulaTaskChecklist()
                    {
                        FormulaTaskId = task.Id,
                        Type = TodoType.Resource,
                        Name = milestone,
                        DateCreated = _dateTimeService.NowUtc
                    });
                }
            }

            // Add the Reviewer Checklist
            if (model.AddReviewerCheckList != null && model.AddReviewerCheckList.Milestones.Count > 0)
            {
                foreach (var milestone in model.AddReviewerCheckList.Milestones)
                {
                    await trx.AddAsync(new FormulaTaskChecklist()
                    {
                        FormulaTaskId = task.Id,
                        Type = TodoType.Reviewer,
                        Name = milestone,
                        DateCreated = _dateTimeService.NowUtc
                    });
                }
            }
        }

        // Get Formula Notes
        public async Task<List<FormulaNotesDto>> GetFormulaNotesAsync(int formulaId)
        {
            var notes = await _repo.Read<FormulaNote>()
                .Where(x => x.FormulaId == formulaId)
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();

            return notes.Select(Mapper.Map<FormulaNotesDto>).ToList();
        }

        public async Task<FormulaNotesDto> AddFormulaNotesAsync(AddFormulaNotesDto model)
        {
            if (model == null)
                throw new ArgumentException(nameof(model));


            var formula = _repo.Read<FormulaProject>()
                .Where(t => t.Id == model.FormulaId)
                .FirstOrDefault();

            if (formula != null && !_principal.IsAdmin() && formula.IsGlobal)
            {
                throw new ValidationException("Formula is locked.");
            }
            else
            {
                using (var trx = _repo.Transaction())
                {
                    if (await trx.Read<FormulaProject>().Where(w => w.Id == model.FormulaId && w.IsLocked).AnyAsync())
                        throw new ValidationException("Formula is locked.");

                    var note = Mapper.Map<FormulaNote>(model);
                    note.DateCreated = _dateTimeService.NowUtc;

                    await trx.AddAsync(note);
                    await trx.SaveChangesAsync();

                    await trx.SaveAndCommitAsync();


                    var res = await trx.Track<FormulaNote>()
                        .SingleOrDefaultAsync(x => x.Id == note.Id);

                    var result = Mapper.Map<FormulaNotesDto>(res);

                    return result;
                }
            }
        }

        public async Task DeleteFormulaNotesAsync(int noteId)
        {
            using (var trx = _repo.Transaction())
            {
                var note = trx.Track<FormulaNote>()
                    .FirstOrDefault(p => p.Id == noteId);

                if (note == null)
                    throw new ValidationException("Formula note is not found.");

                if (await trx.Read<FormulaProject>()
                    .Where(w => w.Id == note.FormulaId && w.IsLocked)
                    .AnyAsync())
                    throw new ValidationException("Formula is locked.");

                trx.Remove(note);
                await trx.SaveAndCommitAsync();
            }
        }

        public async Task SetTrainingLockStatus(int taskId, bool lockStatus)
        {
            using (var trx = _repo.Transaction())
            {
                var guid = _principal.GetUserId();
                var formulaTask = await trx.Track<FormulaTask>()
                    .Include(i => i.Owner)
                    .SingleOrDefaultAsync(p => p.Id == taskId);

                if (formulaTask == null)
                    throw new ValidationException("Formula task is not found.");

                if (formulaTask.OwnerGuid != guid &&
                    !(formulaTask.Owner.OwnerId.HasValue && formulaTask.Owner.OwnerId.Value == guid))
                    throw new ValidationException("Formula is owned by other owner.");

                formulaTask.IsTrainingLocked = lockStatus;

                // Get the Published Formula Task
                var publishedFormulaTask = await trx.Track<FormulaTask>()
                    .Include(i => i.Owner)
                    .SingleOrDefaultAsync(t => t.PublicVaultFormulaTaskID == taskId);

                if (publishedFormulaTask != null)
                    publishedFormulaTask.IsTrainingLocked = lockStatus;

                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task DisableFormulaTask(AddFormulaTaskDisableStatusDto data)
        {
            using (var trx = _repo.Transaction())
            {
                var entity = _mapper.Map<FormulaTaskDisableStatus>(data);
                entity.DateCreated = _dateTimeService.NowUtc;
                entity.IsDisabled = true;

                var formulaProject = await trx.Track<FormulaProject>()
                    .SingleOrDefaultAsync(x => x.Id == data.ParentFormulaId);

                formulaProject.Status = (int)FormulaProjectStatus.Draft;

                await trx.AddAsync(entity);
                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task EnableFormulaTask(AddFormulaTaskDisableStatusDto model)
        {
            using (var trx = _repo.Transaction())
            {
                var entity = await trx.Track<FormulaTaskDisableStatus>()
                    .SingleOrDefaultAsync(x => x.ChildFormulaId == model.ChildFormulaId
                        && x.ParentFormulaId == model.ParentFormulaId
                        && x.InternalChildFormulaId == model.InternalChildFormulaId
                        && x.InternalChildFormulaTaskId == model.InternalChildFormulaTaskId);

                var formulaProject = await trx.Track<FormulaProject>()
                    .SingleOrDefaultAsync(x => x.Id == model.ParentFormulaId);

                formulaProject.Status = (int)FormulaProjectStatus.Draft;

                trx.Remove(entity);
                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task<List<AddFormulaTaskDisableStatusDto>> GetDisabledTasksAsync(int internalChildFormulaId, int parentFormulaId, int childFormulaId)
        {
            var tasks = await _repo.Read<FormulaTaskDisableStatus>()
                .Where(t => t.ParentFormulaId == parentFormulaId
                    && t.InternalChildFormulaId == internalChildFormulaId
                    && t.ChildFormulaId == childFormulaId)
                .ToListAsync();

            return tasks.Select(Mapper.Map<AddFormulaTaskDisableStatusDto>).ToList();
        }

        public async Task AddPublishedTaskToNeo4j(int formulaId)
        {
            using (var transaction = _formulaTaskNeo4jRepository.BeginTransaction())
            {
                var formulaProjectId = await _repo.Read<FormulaProject>()
                    .Where(w => w.PublicVaultFormulaProjectID == formulaId)
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();

                var formulaTasks = await _repo.Read<FormulaTask>()
                    .Include(i => i.FormulaProject)
                    .Where(w => w.FormulaProject.PublicVaultFormulaProjectID == formulaId)
                    .ToListAsync();

                await _formulaNeo4JRepository.AddFormulaAsync(formulaProjectId);

                foreach (var task in formulaTasks)
                {
                    if (task.InternalFormulaProjectId.HasValue)
                    {
                        await _formulaTaskNeo4jRepository.AddTaskAsync(task.Id, task.FormulaProjectId);
                        await _formulaNeo4JRepository.AddRelationAsync(task.FormulaProjectId, task.InternalFormulaProjectId.Value);
                    }
                    else
                    {
                        await _formulaTaskNeo4jRepository.AddTaskAsync(task.Id, task.FormulaProjectId);
                    }
                }

                transaction.Commit();
            }
        }
    }
}
