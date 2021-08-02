using AutoMapper;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Formula.Interfaces;
using IAutoM8.Service.FormulaTasks.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IAutoM8.Service.Formula
{
    public class FormulaShareService : IFormulaShareService
    {
        private readonly IRepo _repo;
        private readonly IFormulaTaskService _formulaTaskService;
        private readonly IFormulaNeo4jRepository _formulaNeo4JRepository;
        private readonly IFormulaTaskNeo4jRepository _formulaTaskNeo4jRepository;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public FormulaShareService(IRepo repo, IFormulaTaskService formulaTaskService,
            IFormulaNeo4jRepository formulaNeo4JRepository,
            IFormulaTaskNeo4jRepository formulaTaskNeo4jRepository,
            IStorageService storageService,
            IMapper mapper)
        {
            _repo = repo;
            _formulaTaskService = formulaTaskService;
            _formulaNeo4JRepository = formulaNeo4JRepository;
            _formulaTaskNeo4jRepository = formulaTaskNeo4jRepository;
            _storageService = storageService;
            _mapper = mapper;
        }

        public async Task<FormulaDto> UpdateFormulaShareStatus(int formulaId, FormulaShareStatusDto formulaShareStatusDto)
        {
            using (var trx = _repo.Transaction())
            {
                var formulaProject = await trx.Track<FormulaProject>()
                    .Include(f => f.OriginalFormulaProject)
                    .Include(f => f.Owner)
                    .FirstOrDefaultAsync(f => f.Id == formulaId);

                if (formulaProject == null)
                    throw new ValidationException("Formula is not found.");

                if (formulaProject.OriginalFormulaProject != null && !formulaProject.OriginalFormulaProject.IsResharingAllowed)
                    throw new ValidationException("Cannot share formula. Resharing is disabled.");

                _mapper.Map(formulaShareStatusDto, formulaProject);
                await trx.SaveAndCommitAsync(CancellationToken.None);

                return _mapper.Map<FormulaDto>(formulaProject);
            }
        }

        public async Task<IList<FormulaUserShareDto>> GetFormulaShareList(int formulaId)
        {
            var shares = await _repo.Read<FormulaShare>()
                .Include(c => c.AccessHolder)
                .Include(c => c.FormulaProject)
                .Where(c => c.FormulaProjectId == formulaId).ToListAsync();
            return shares.Select(_mapper.Map<FormulaUserShareDto>).ToList();
        }

        public async Task<FormulaUserShareDto> ShareFormulaToUser(FormulaUserShareDto model)
        {
            if (model == null)
                throw new ArgumentException(nameof(model));
            using (var trx = _repo.Transaction())
            {
                var share = _mapper.Map<FormulaShare>(model);

                await trx.AddAsync(share);
                await trx.SaveAndCommitAsync(CancellationToken.None);

                var res = await _repo.Read<FormulaShare>()
                    .FirstOrDefaultAsync(x => x.FormulaProjectId == share.FormulaProjectId
                                            && x.UserId == share.UserId);
                return _mapper.Map<FormulaUserShareDto>(res);
            }
        }

        public async Task RemoveUserFromShareList(FormulaUserShareDto model)
        {
            using (var trx = _repo.Transaction())
            {
                var share = trx.Track<FormulaShare>()
                    .FirstOrDefault(p => p.FormulaProjectId == model.FormulaProjectId && p.UserId == model.UserId);
                if (share == null)
                    throw new ValidationException("Formula is not found.");

                trx.Remove(share);
                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task CopyFormulaToUser(int formulaId, Guid userGuid)
        {
            using (var trx = _repo.Transaction())
            {
                using (var transaction = _formulaNeo4JRepository.BeginTransaction())
                {
                    var formulaMappings = new Dictionary<int, int>();
                    await CopyFormula(trx, formulaId, userGuid, formulaMappings);
                    await trx.SaveAndCommitAsync(CancellationToken.None);
                    transaction.Commit();
                }
            }
        }

        private async Task<int> CopyFormula(ITransactionScope trx, int formulaId, Guid userGuid, Dictionary<int, int> formulaMappings)
        {
            var formula = trx.Track<FormulaProject>()
                        .Include(x => x.FormulaTasks)
                            .ThenInclude(task => task.ParentTasks)
                        .Include(x => x.FormulaTasks)
                            .ThenInclude(task => task.AssignedSkill)
                        .Include(x => x.FormulaTasks)
                            .ThenInclude(task => task.ReviewingSkill)
                        .Include(x => x.FormulaTasks)
                            .ThenInclude(task => task.RecurrenceOptions)
                        .Include(x => x.ResourceFormula)
                            .ThenInclude(res => res.Resource)
                        .Include(x => x.FormulaTasks)
                            .ThenInclude(x => x.FormulaTaskVendors)
                        .FirstOrDefault(f => f.Id == formulaId && !f.IsDeleted);

            if (formula == null)
                throw new ValidationException("Formula is not found.");

            if (formula.IsDeleted)
                throw new ValidationException("Formula is deleted.");

            var bussinessSkills = await trx.Read<Skill>()
                .Where(w => w.OwnerGuid == userGuid)
                .ToListAsync();

            var formulaCopy = new FormulaProject(formula, userGuid);
            await trx.AddAsync(formulaCopy);
            await trx.SaveChangesAsync();

            foreach (var res in formula.ResourceFormula.Select(s => s.Resource))
            {
                var path = res.Type == ResourceType.Link ? res.Name : $"{formulaCopy.Id}/{res.Name}";
                if ((byte)ResourceType.File == res.Type)
                    await _storageService.CopyFileAsync(res.Path, path, StorageType.Formula, StorageType.Formula);
                formulaCopy.ResourceFormula.Add(new ResourceFormula
                {
                    Resource = _mapper.Map<Resource>(res, opts => opts.Items.Add("path", path))
                });
            }
            await _formulaNeo4JRepository.AddFormulaAsync(formulaCopy.Id);

            var tasksMapping = new Dictionary<FormulaTask, FormulaTask>();
            foreach (var formulaTask in formula.FormulaTasks)
            {
                int internalFormulaProjectId = 0;
                if (formulaTask.InternalFormulaProjectId != null)
                {
                    internalFormulaProjectId = trx.Track<FormulaProject>()
                        .FirstOrDefault(t => t.OriginalFormulaProjectId == formulaTask.InternalFormulaProjectId
                        && !t.IsDeleted && t.OwnerGuid == userGuid)?.Id ?? 0;
                    if (internalFormulaProjectId == 0 && !formulaMappings.TryGetValue(formulaTask.InternalFormulaProjectId.Value, out internalFormulaProjectId))
                    {
                        internalFormulaProjectId = await CopyFormula(trx, formulaTask.InternalFormulaProjectId.Value, userGuid, formulaMappings);
                        formulaMappings.Add(formulaTask.InternalFormulaProjectId.Value, internalFormulaProjectId);
                    }
                }

                // NOTE: businessSkills list is mutated inside!
                var task = await _formulaTaskService.CreateTask(trx, formulaTask, userGuid, bussinessSkills,
                    formulaCopy.Id, internalFormulaProjectId == 0 ? (int?)null : internalFormulaProjectId);

                tasksMapping.Add(formulaTask, task);
            }

            foreach (var pair in tasksMapping)
            {
                await _formulaTaskService.CopyResources(pair);
            }

            var dictionary = tasksMapping.ToDictionary(k => k.Key.Id, v => v.Value.Id);

            //Copy dependencies
            foreach (var pair in dictionary)
            {
                var originalTask = formula.FormulaTasks.First(x => x.Id == pair.Key);
                foreach (var childFormulaTaskDependency in originalTask.ParentTasks)
                {
                    if (!dictionary.ContainsKey(childFormulaTaskDependency.ParentTask.Id))
                        continue;
                    var parentTaskId = dictionary[childFormulaTaskDependency.ParentTask.Id];
                    var parentTask = formulaCopy.FormulaTasks.First(t => t.Id == parentTaskId);
                    var childTask = formulaCopy.FormulaTasks.First(t => t.Id == pair.Value);
                    var isRequired = childFormulaTaskDependency.Required;
                    await _formulaTaskService.CreateTaskDependency(trx, parentTask, childTask, isRequired);
                    await trx.SaveChangesAsync();
                    await _formulaTaskNeo4jRepository.AddTaskDependencyAsync(parentTaskId, childTask.Id);
                }
            }

            //Create conditions with options
            foreach (var pair in dictionary)
            {
                var formulaTask = formula.FormulaTasks.First(f => f.Id == pair.Key);
                if (!formulaTask.TaskConditionId.HasValue)
                    continue;

                var taskCondition = await trx.Read<FormulaTaskCondition>()
                    .Include(c => c.Options)
                    .Where(c => c.Id == formulaTask.TaskConditionId)
                    .SingleAsync();

                var formulaTaskCondition = await _formulaTaskService.CreateCondition(trx, formulaCopy.FormulaTasks.First(f => f.Id == pair.Value), taskCondition);
                foreach (var formulaTaskConditionOption in taskCondition.Options)
                {
                    var optionTargetId = formulaTaskConditionOption.AssignedTaskId.HasValue ? dictionary[formulaTaskConditionOption.AssignedTaskId.Value] : (int?)null;
                    var optionTarget = formulaCopy.FormulaTasks.FirstOrDefault(f => f.Id == optionTargetId);

                    var option = await _formulaTaskService.CreateConditionOption(trx, formulaTaskConditionOption, optionTarget);
                    formulaTaskCondition.Options.Add(option);
                    await trx.SaveChangesAsync();
                    optionTarget?.AssignedConditionOptions.Add(option);

                    if (optionTargetId.HasValue)
                    {
                        await _formulaTaskNeo4jRepository.AddTaskConditionAsync(option.Id, formulaTaskCondition.Task.Id, optionTargetId.Value);
                    }
                }
            }
            foreach (var task in formulaCopy.FormulaTasks)
            {
                if (task.InternalFormulaProjectId.HasValue)
                    await _formulaNeo4JRepository.AddRelationAsync(formulaCopy.Id,
                        task.InternalFormulaProjectId.Value);
            }

            return formulaCopy.Id;
        }

        public async Task<FormulaShareType> GetFormulaShareStatus(int formulaId)
        {
            var formula = await _repo.Read<FormulaProject>()
                .Include(f => f.OriginalFormulaProject)
                .FirstOrDefaultAsync(p => p.Id == formulaId);

            if (formula == null)
                throw new ValidationException("Formula is not found.");
            if (formula.OriginalFormulaProject != null && !formula.OriginalFormulaProject.IsResharingAllowed)
                throw new ValidationException("You cannot reshare formula.");

            return formula.ShareType;
        }

        public async Task<List<string>> CheckFormulaAndSubformulasForSharing(int formulaId)
        {
            var internalFormulaIds = await _formulaNeo4JRepository.GetAllInternalFormulaIds(formulaId);
            var formulas = await _repo.Read<FormulaProject>()
                .Include(f => f.OriginalFormulaProject)
                .Include(f => f.ChildFormulaProjects)
                .Where(t => internalFormulaIds.Contains(t.Id))
                .ToListAsync();

            List<string> validationMessages = new List<string>();

            foreach (var formula in formulas)
            {
                if (formula.ShareType != FormulaShareType.PublicLink)
                {
                    validationMessages.Add($"{formula.Name} is not allowed for sharing");
                }

                if (formula.OriginalFormulaProject != null && !formula.OriginalFormulaProject.IsResharingAllowed)
                {
                    validationMessages.Add($"{formula.Name} is not allowed for resharing");
                }
            }

            return validationMessages;
        }

        public async Task<IList<int>> GetUniqueFormulas(IList<int> formulaIds)
        {
            var internalFormulas = new HashSet<int>();
            foreach (var formulaId in formulaIds)
            {
                foreach(var internalFormulaId in await _formulaNeo4JRepository.GetAllInternalFormulaIds(formulaId))
                {
                    if (internalFormulaId != formulaId)
                        internalFormulas.Add(internalFormulaId);
                }
            }
            return formulaIds.Where(id => !internalFormulas.Contains(id)).ToList();
        }
    }
}
