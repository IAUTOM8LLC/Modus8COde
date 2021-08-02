using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.Team;
using IAutoM8.Domain.Models.User;
using IAutoM8.Global.Enums;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.Skills.Dto;
using IAutoM8.Service.Skills.Interfaces;
using IAutoM8.Service.Teams.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Service.Skills
{
    public class SkillService : ISkillService
    {
        private readonly IRepo _repo;
        private readonly ClaimsPrincipal _principal;
        private readonly INotificationService _notificationService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public SkillService(IRepo repo, ClaimsPrincipal principal,
            INotificationService notificationService, IDateTimeService dateTimeService,
            IMapper mapper,
            IStorageService storageService)
        {
            _repo = repo;
            _principal = principal;
            _notificationService = notificationService;
            _dateTimeService = dateTimeService;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<List<SkillDto>> GetSkillAsync()
        {
            bool isAdmin = _principal.IsAdmin();

            var users = await _repo.Read<User>()
                .Where(u => u.OwnerId == _principal.GetOwnerId() || u.Id == _principal.GetOwnerId())
                .Select(u => u.Id)
                .ToListAsync();

            var query = _repo.Read<Skill>()
                .Include(t => t.UserSkills)
                    .ThenInclude(tu => tu.User)
                    .ThenInclude(u => u.Roles)
                    .ThenInclude(r => r.Role)
                .Include(t => t.UserSkills)
                    .ThenInclude(tu => tu.User)
                    .ThenInclude(tu => tu.Profile)
                .Include(i => i.AssignedTasks)
                .Include(i => i.AssignedFormulaTasks)
                    .ThenInclude(i => i.FormulaProject)
                .Include(i => i.ReviewingTasks)
                    .ThenInclude(i => i.FormulaProject)
                .Include(i => i.ReviewingFormulaTasks)
                .Include(i => i.TeamSkills)
                    .ThenInclude(i => i.Team);

            if (isAdmin)
            {
                var result = await query.Where(w => w.OwnerGuid == _principal.GetOwnerId())
                    .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                    .OrderByDescending(o => o.IsGlobal)
                    .ThenBy(o => o.Name)
                    .ToListAsync();

                return result;
            }
            else
            {
                var result = await query
                    .Where(w => w.OwnerGuid == _principal.GetOwnerId() || (w.Status == (int)FormulaProjectStatus.UserTask && w.IsGlobal == true))
                    .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                    .OrderByDescending(o => o.IsGlobal)
                    .ThenBy(o => o.Name)
                    .ToListAsync();

                result.ForEach(x => x.Users.ForEach(y => y.UserImage = y.UserImage != null
                    ? _storageService.GetFileUri($"{y.UserId}/{y.UserImage}", StorageType.ProfileImage)
                    : null)
                );

                return result
                    .Select(s => new SkillDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        DateCreated = s.DateCreated,
                        LastUpdated = s.LastUpdated,
                        Users = s.Users.Where(w => users.Contains(w.UserId)).ToList(),
                        HasAssignedTasks = s.HasAssignedTasks,
                        IsWorkerSkill = s.IsWorkerSkill,
                        IsGlobal = s.IsGlobal,
                        Status = s.Status,
                        TeamName = s.TeamName,
                        RevFormulas = s.RevFormulas,
                        DevFormulas = s.DevFormulas
                    })
                    .ToList();
            }
        }

        public async Task<List<SkillDto>> GetSkillByTeam(int teamId)
        {
            bool isAdmin = _principal.IsAdmin();

            var query = _repo.Read<Skill>()
                .Include(t => t.UserSkills)
                            .ThenInclude(tu => tu.User)
                            .ThenInclude(u => u.Roles)
                            .ThenInclude(r => r.Role)
                        .Include(t => t.UserSkills)
                            .ThenInclude(tu => tu.User)
                            .ThenInclude(tu => tu.Profile)
                        .Include(i => i.AssignedTasks)
                        .Include(i => i.AssignedFormulaTasks)
                        .Include(i => i.ReviewingTasks)
                        .Include(i => i.ReviewingFormulaTasks)
                        .Include(t => t.TeamSkills)
                        .AsQueryable();

            if (isAdmin)
            {
                query = query
                    .Where(w => w.OwnerGuid == _principal.GetOwnerId() && w.TeamSkills.Any(x => x.TeamId == teamId));

            }
            else
            {
                query = query
                        .Where(w => (w.OwnerGuid == _principal.GetOwnerId()
                            || (w.Status == (int)FormulaProjectStatus.UserTask && w.IsGlobal))
                            && w.TeamSkills.Any(x => x.TeamId == teamId));
            }

            var result = await query.ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                       .OrderBy(o => o.Name)
                       .ToListAsync();

            return result;
        }

        public async Task<List<SkillDto>> GetRevSkills()
        {
            bool isAdmin = _principal.IsAdmin();

            var query = _repo.Read<Skill>();

            if (isAdmin)
            {
                query = query.Include(t => t.UserSkills)
                            .ThenInclude(tu => tu.User)
                            .ThenInclude(u => u.Roles)
                            .ThenInclude(r => r.Role)
                        .Include(t => t.UserSkills)
                            .ThenInclude(tu => tu.User)
                            .ThenInclude(tu => tu.Profile)
                        .Include(i => i.AssignedTasks)
                        .Include(i => i.AssignedFormulaTasks)
                        .Include(i => i.ReviewingTasks)
                        .Include(i => i.ReviewingFormulaTasks)
                        .Include(t => t.TeamSkills)
                        .Where(w => w.OwnerGuid == _principal.GetOwnerId());

            }
            else
            {
                query = query.Include(t => t.UserSkills)
                            .ThenInclude(tu => tu.User)
                            .ThenInclude(u => u.Roles)
                            .ThenInclude(r => r.Role)
                        .Include(t => t.UserSkills)
                            .ThenInclude(tu => tu.User)
                            .ThenInclude(tu => tu.Profile)
                        .Include(i => i.AssignedTasks)
                        .Include(i => i.AssignedFormulaTasks)
                        .Include(i => i.ReviewingTasks)
                        .Include(i => i.ReviewingFormulaTasks)
                        .Include(t => t.TeamSkills)
                        .Where(w => (w.OwnerGuid == _principal.GetOwnerId() || (w.Status == (int)FormulaProjectStatus.UserTask && w.IsGlobal)));
            }

            var result = await query.ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                       .OrderBy(o => o.Name)
                       .ToListAsync();

            return result;
        }

        public async Task<UpdateSkillDetailDto> GetSkillAsync(int skillId)
        {
            var skill = await _repo.Read<Skill>()
                .Include(i => i.UserSkills)
                .FirstOrDefaultAsync(w => w.Id == skillId);

            if (skill.OwnerGuid != _principal.GetOwnerId() && !skill.IsGlobal)
                throw new ValidationException("You have no access to skill.");

            var model = Mapper.Map<UpdateSkillDetailDto>(skill);
            return model;
        }

        public async Task<SkillDto> AddSkillAsync(UpdateSkillDetailDto model)
        {
            var ownerGuid = _principal.GetOwnerId();
            var userRole = await _repo.Track<User>().Where(x => x.Id == ownerGuid)
                   .Include(t => t.Roles)
                       .ThenInclude(t => t.Role).Select(x => new
                       {
                           Role = x.Roles.Select(r => r.Role.Name).FirstOrDefault()
                       }).FirstOrDefaultAsync();
            var skillEntity = Mapper.Map<Skill>(model);
            skillEntity.OwnerGuid = ownerGuid;
            skillEntity.DateCreated = _dateTimeService.NowUtc;
            skillEntity.IsGlobal = (userRole != null && userRole.Role == "Admin") ? true : false;
            skillEntity.Status = (_principal.IsAdmin()) ? (int?)FormulaProjectStatus.UserTask : null;
            if (model.UserSkills != null)
                foreach (var userId in model.UserSkills)
                {
                    skillEntity.UserSkills.Add(new UserSkill { UserId = userId });
                }

            // If the Skill is a CustomSkill, then TeamId would be not null
            if (model.TeamId.HasValue)
                skillEntity.TeamSkills.Add(new TeamSkill { TeamId = model.TeamId.Value });

            await _repo.AddAsync(skillEntity);
            await _repo.SaveChangesAsync();

            // Team Members
            var companyMembers = await _repo.Read<User>()
                .Where(u => u.OwnerId == _principal.GetOwnerId() || u.Id == _principal.GetOwnerId())
                .Select(u => u.Id)
                .ToListAsync();

            var skill = await _repo.Read<Skill>()
                .Include(t => t.UserSkills)
                .ThenInclude(tu => tu.User)
                .ThenInclude(tu => tu.Profile)
                .SingleAsync(t => t.Id == skillEntity.Id);

            var result = Mapper.Map<SkillDto>(skill);

            if (result == null)
                throw new ValidationException("Unable to create the skill");

            return new SkillDto
            {
                Id = result.Id,
                Name = result.Name,
                DateCreated = result.DateCreated,
                LastUpdated = result.LastUpdated,
                Users = result.Users.Where(w => companyMembers.Contains(w.UserId)).ToList(),
                HasAssignedTasks = result.HasAssignedTasks,
                IsWorkerSkill = result.IsWorkerSkill,
                IsGlobal = result.IsGlobal,
                Status = result.Status,
                TeamName = result.TeamName,
                RevFormulas = result.RevFormulas,
                DevFormulas = result.DevFormulas
            };
        }

        public async Task<SkillDto> UpdateSkillAsync(UpdateSkillDetailDto model)
        {
            using (var trx = _repo.Transaction())
            {
                var skill = await trx.Track<Skill>()
                    .Include(c => c.UserSkills)
                    .FirstOrDefaultAsync(p => p.Id == model.Id);

                if (skill == null)
                    throw new ValidationException("Skill is not found.");

                if (skill.OwnerGuid != _principal.GetOwnerId() && !skill.IsGlobal)
                    throw new ValidationException("You have no access to skill.");

                Mapper.Map(model, skill);
                skill.LastUpdated = _dateTimeService.NowUtc;
                foreach (var userSkill in skill.UserSkills.Where(w => !model.UserSkills.Contains(w.UserId)))
                {
                    trx.Remove(userSkill);
                }
                foreach (var userId in model.UserSkills.Where(w => skill.UserSkills.All(a => a.UserId != w)))
                {
                    await trx.AddAsync(new UserSkill
                    {
                        SkillId = model.Id,
                        UserId = userId
                    });
                }
                await trx.SaveAndCommitAsync();

                // Team Members
                var companyMembers = await _repo.Read<User>()
                    .Where(u => u.OwnerId == _principal.GetOwnerId() || u.Id == _principal.GetOwnerId())
                    .Select(u => u.Id)
                    .ToListAsync();

                skill = await trx.Read<Skill>()
                    .Include(c => c.UserSkills)
                        .ThenInclude(c => c.User)
                        .ThenInclude(c => c.Profile)
                    .Include(i => i.TeamSkills)
                        .ThenInclude(i => i.Team)
                    .FirstOrDefaultAsync(p => p.Id == model.Id);

                var result = Mapper.Map<SkillDto>(skill);

                if (result == null)
                    throw new ValidationException("Unable to update the skill");

                return new SkillDto
                {
                    Id = result.Id,
                    Name = result.Name,
                    DateCreated = result.DateCreated,
                    LastUpdated = result.LastUpdated,
                    Users = result.Users.Where(w => companyMembers.Contains(w.UserId)).ToList(),
                    HasAssignedTasks = result.HasAssignedTasks,
                    IsWorkerSkill = result.IsWorkerSkill,
                    IsGlobal = result.IsGlobal,
                    Status = result.Status,
                    TeamName = result.TeamName,
                    RevFormulas = result.RevFormulas,
                    DevFormulas = result.DevFormulas
                };
            }
        }

        public async Task DeleteSkillAsync(int skillId)
        {
            using (var trx = _repo.Transaction())
            {
                var skill = await trx.Track<Skill>()
                    .Include(c => c.ReviewingTasks)
                    .Include(c => c.AssignedTasks)
                    .Include(c => c.ReviewingFormulaTasks)
                    .Include(c => c.AssignedFormulaTasks)
                    .Include(c => c.UserSkills)
                    .FirstOrDefaultAsync(p => p.Id == skillId);

                if (skill == null)
                    throw new ValidationException("Skill is not found.");
                if (skill.OwnerGuid != _principal.GetOwnerId())
                    throw new ValidationException("You have no access to skill.");

                if (skill.AssignedTasks.Count > 0 || skill.AssignedFormulaTasks.Count > 0)
                    throw new ValidationException("You cannot delete skill becouse it is assigned to task");

                if (skill.ReviewingTasks.Count > 0 || skill.ReviewingFormulaTasks.Count > 0)
                    throw new ValidationException("You cannot delete skill becouse it is assigned to task");

                trx.RemoveRange(skill.UserSkills);
                trx.Remove(skill);

                await trx.SaveAndCommitAsync();
            }
        }
    }
}
