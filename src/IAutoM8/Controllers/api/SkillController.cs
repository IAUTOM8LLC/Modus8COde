using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IAutoM8.Controllers.Abstract;
using Microsoft.AspNetCore.Authorization;
using IAutoM8.Service.Skills.Interfaces;
using IAutoM8.Service.Skills.Dto;

namespace IAutoM8.Controllers.api
{
    [Authorize(Roles = OwnerOrManagerOrAdmin)]
    [Route("api/skill")]
    public class SkillController : BaseController
    {
        private readonly ISkillService _skillService;

        public SkillController(ISkillService skillService)
        {
            _skillService = skillService;
        }

        #region Skill CRUD

        [HttpGet]
        public async Task<IActionResult> GetSkills()
        {
            var result = await _skillService.GetSkillAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("{skillId:int}")]
        public async Task<IActionResult> GetSkill(int skillId)
        {
            var result = await _skillService.GetSkillAsync(skillId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody]UpdateSkillDetailDto skillDto)
        {
            var task = await _skillService.AddSkillAsync(skillDto);
            return Ok(task);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSkill([FromBody]UpdateSkillDetailDto skillDto)
        {
            var task = await _skillService.UpdateSkillAsync(skillDto);
            return Ok(task);
        }

        [HttpDelete]
        [Route("{skillId:int}")]
        public async Task<IActionResult> DeleteSkill([FromRoute]int skillId)
        {
            await _skillService.DeleteSkillAsync(skillId);
            return Ok();
        }

        #endregion

        [HttpGet]
        [Route("team/{teamId:int}")]
        public async Task<IActionResult> GetSkillsByTeam(int teamId)
        {
            var result = await _skillService.GetSkillByTeam(teamId);
            return Ok(result);
        }

        [HttpGet]
        [Route("revskills")]
        public async Task<IActionResult> GetRevSkills()
        {
            var result = await _skillService.GetRevSkills();
            return Ok(result);
        }

        [HttpPost]
        [Route("custom-skill")]
        public async Task<IActionResult> AddCustomSkill([FromBody]UpdateSkillDetailDto model)
        {
            var result = await _skillService.AddSkillAsync(model);
            return Ok(result);
        }
    }
}
