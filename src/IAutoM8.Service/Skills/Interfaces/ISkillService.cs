using IAutoM8.Service.Skills.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Service.Skills.Interfaces
{
    public interface ISkillService
    {
        Task<List<SkillDto>> GetSkillAsync();
        Task<List<SkillDto>> GetSkillByTeam(int teamId);
        Task<List<SkillDto>> GetRevSkills();
        Task<UpdateSkillDetailDto> GetSkillAsync(int skillId);
        Task<SkillDto> AddSkillAsync(UpdateSkillDetailDto model);
        Task<SkillDto> UpdateSkillAsync(UpdateSkillDetailDto model);
        Task DeleteSkillAsync(int skillId);
    }
}
