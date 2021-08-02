using IAutoM8.Controllers.Abstract;
using IAutoM8.Service.Teams.Dto;
using IAutoM8.Service.Teams.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Global.Enums;

namespace IAutoM8.Controllers.api
{
    [Authorize(Roles = OwnerOrManagerOrAdmin)]
    [Route("api/Teams")]
    public class TeamController : BaseController
    {

        private readonly ITeamService _teamService;
        private readonly System.Security.Claims.ClaimsPrincipal _principal;
        public TeamController(ITeamService teamService, System.Security.Claims.ClaimsPrincipal principal)
        {
            _teamService = teamService;
            _principal = principal;
        }

        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {

            //var tets= _principal.get
            // Here we will have all teams
            //var result = await _teamService.GetTeamsAsync();           
            var result = await _teamService.GetTeamsData();           

            List<TeamData> teamData = new List<TeamData>();

            bool IsAdmin = _principal.IsAdmin();

            //Getting Unique Team Id's
            var TeamsList = result.GroupBy(x => x.Id).Select(grp => grp.ToList()).ToList();

            //if (IsAdmin)
            //{
            //    TeamsList = TeamsList.Where(x => x.FirstOrDefault().TeamStatus != (int)FormulaProjectStatus.UserTask).ToList();
            //}


            if (TeamsList.Count == 0)
            {
                teamData.Add(new TeamData { TeamName = "", IsAdmin = IsAdmin });
            }

            foreach (var list in TeamsList)
            {
                TeamData team = new TeamData()
                {
                    Id = list.FirstOrDefault().Id,
                    TeamName = list.FirstOrDefault().TeamName,
                    isGlobal = list.FirstOrDefault().TeamIsGlobal,
                    TeamStatus = list.FirstOrDefault().TeamStatus,
                    IsAdmin = _principal.IsAdmin()
                };


                //Getting Unique Skill Id's here
                var skillData = list.GroupBy(x => x.SkillId).Select(grp => grp.ToList()).ToList();
                #region Preparing Skills Data
                List<SkillRef> skills = new List<SkillRef>();
                foreach (var skill in skillData)
                {

                    if (skill.FirstOrDefault().SkillId > 0)
                    {
                        SkillRef skillRef = new SkillRef();
                        skillRef.SkillId = skill.FirstOrDefault().SkillId;
                        skillRef.SkillName = skill.FirstOrDefault().SkillName;
                        skillRef.SkillIsGlobal = skill.FirstOrDefault().SkillIsGlobal;
                        skillRef.SkillStatus = skill.FirstOrDefault().SkillStatus;

                        #region Preparing Users List

                        //Getting unique users of this skill
                        //var uniqueUsers = skill.GroupBy(c => new
                        //{
                        //    c.OutsourcerId,
                        //    c.OutsourcerName
                        //}).Select(gcs => new UsersRef()
                        //{
                        //    OutsourcerId = gcs.Key.OutsourcerId,
                        //    OutsourcerName = gcs.Key.OutsourcerName
                        //});

                        //List<UsersRef> users = new List<UsersRef>();

                        //foreach (var u in uniqueUsers)
                        //{
                        //    users.Add(new UsersRef { OutsourcerId = u.OutsourcerId, OutsourcerName = u.OutsourcerName });
                        //}

                        //skillRef.Users = users;

                        #endregion

                        #region Preparing Formula List
                        List<FormulasTaskRef> formulaList = new List<FormulasTaskRef>();

                        // var uniqueFormulaList = skill.GroupBy(x => x.FormulaID).Select(grp => grp.ToList()).ToList();

                        foreach (var u in skill)
                        {
                            if (u.FormulaID > 0)
                            {
                                var showFormula = skill.Where(x => x.FormulaID == u.FormulaID && x.OutsourcerName == null).Count();

                                formulaList.Add(new FormulasTaskRef
                                {
                                    Id = u.FormulaID,
                                    FormulaName = u.FormulaName,
                                    FormulaCreatedDate = u.FormulaCreatedDate,
                                    FormulaUpdatedDate = u.FormulaUpdatedDate,
                                    FormulaIsGlobal = u.FormulaIsGlobal,
                                    FormulaStatus = u.FormulaStatus,
                                    TaskID = u.TaskID,
                                    TaskName = u.TaskName,
                                    TaskType = u.TaskIsGlobal,
                                    OutsourcerName = u.OutsourcerName,
                                    ShowFormulaPublish = showFormula > 0 ? false : true
                                });
                            }
                        }

                        skillRef.Formulas = formulaList;
                        skills.Add(skillRef);
                        #endregion

                    }


                }
                team.Skill = skills;
                #endregion

                teamData.Add(team);
            }





            return Ok(teamData);
        }

        [HttpGet]
        [Route("GetUnusedSkills/{teamId:int}")]
        public async Task<IActionResult> GetUnusedSkills(int teamId)
        {
            var result = await _teamService.GetUnusedSkills(teamId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddTeam([FromBody] UpdateTeamDetailDto teamDto)
        {
            var task = await _teamService.AddTeamAsync(teamDto);
            return Ok(task);
        }

        [HttpGet]
        [Route("{TeamId:int}")]
        public async Task<IActionResult> GetTeam(int teamId)
        {
            var result = await _teamService.GetTeamAsync(teamId);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTeam([FromBody] UpdateTeamDetailDto teamDto)
        {
            var task = await _teamService.UpdateTeamAsync(teamDto);
            return Ok(task);
        }

        [HttpDelete]
        [Route("{teamId:int}")]
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            await _teamService.DeleteTeamAsync(teamId);
            return Ok();
        }


        [HttpGet]
        [Route("allteams")]
        public async Task<IActionResult> GetAllTeams()
        {
            var result = await _teamService.GetAllTeamsAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("publishformula/{formulaId:int}")]
        public async Task<IActionResult> PublishFormula(int formulaId)
        {
            var result = await _teamService.PublishFormula(formulaId);
            return Ok(result);
        }

        [HttpGet]
        [Route("publishteam/{teamId:int}")]
        public async Task<IActionResult> PublishTeam(int teamId)
        {
            var result = await _teamService.PublishTeam(teamId);
            return Ok(result);
        }

        [HttpGet]
        [Route("publishskill/{skillId:int}")]
        public async Task<IActionResult> PublishSkill(int skillId)
        {
            var result = await _teamService.PublishSkill(skillId);
            return Ok(result);
        }
    }
}
