using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Service.Skills.Dto;
using IAutoM8.Service.Teams.Dto;

namespace IAutoM8.Service.Teams.Interfaces
{
    public interface ITeamService
    {
        Task<IList<TeamDto>> GetTeamsAsync();
        Task<IList<SkillDto>> GetUnusedSkills(int teamId);
        Task<TeamDto> AddTeamAsync(UpdateTeamDetailDto model);
        Task<TeamDto> UpdateTeamAsync(UpdateTeamDetailDto model);
        Task<UpdateTeamDetailDto> GetTeamAsync(int teamId);
        Task DeleteTeamAsync(int teamId);
        Task<List<TeamSkillDto>> GetAllTeamsAsync();
        Task<PublishStatus> PublishFormula(int formulaId);
        Task<IList<PublishStatus>> PublishTeam(int teamId);
        Task<IList<PublishStatus>> PublishSkill(int skillId);
        Task<IList<TeamDto>> GetTeamsData();
        
    }
}
