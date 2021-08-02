using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.Team;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Formula.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.Skills.Dto;
using IAutoM8.Service.Teams.Dto;
using IAutoM8.Service.Teams.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Service.Teams
{
    public class TeamService : ITeamService
    {

        private readonly IRepo _repo;
        private readonly IMapper _mapper;
        private readonly ClaimsPrincipal _principal;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationService _notificationService;
        private readonly IFormulaService _formulaService;

        public TeamService(IRepo repo,
            IMapper mapper,
             ClaimsPrincipal principal,
             IDateTimeService dateTimeService,
             INotificationService notificationService,
             IFormulaService formulaService
            )
        {
            _repo = repo;
            _mapper = mapper;
            _principal = principal;
            _dateTimeService = dateTimeService;
            _notificationService = notificationService;
            _formulaService = formulaService;
        }


        public async Task<IList<TeamDto>> GetTeamsData()
        {
            Guid ownerID = _principal.GetOwnerId();
            bool isAdmin = _principal.IsAdmin();
            //string whereQuery = isAdmin ? " and fp.Status in (1,2,3,4)" : "and fp.Status = 5";
            var list = await _repo.ExecuteSql<TeamDto>(_mapper, "usp_GetTeamSkillFormulaList @ownerID, @isAdmin",
            new List<SqlParameter> {
                new SqlParameter { ParameterName = "@ownerID",SqlDbType = SqlDbType.UniqueIdentifier, Value = ownerID } ,
                new SqlParameter
                    {
                        ParameterName = "@isAdmin",
                        SqlDbType = SqlDbType.Bit,
                        Value = isAdmin
                    }}
            ).ToListAsync();

            return list;
        }

        public async Task<IList<TeamDto>> GetTeamsAsync()
        {
            Guid ownerID = _principal.GetOwnerId();
            bool isAdmin = _principal.IsAdmin();

            string whereQuery = isAdmin ? " and fp.Status in (1,2,3,4)" : "and fp.Status = 5";

            var query = @"
                        	-- Global ( Admin query & Global Owner)
	SELECT  DISTINCT
                        t.Id AS Id,
                        t.Name AS TeamName,
                        t.IsGlobal AS TeamIsGlobal,
                        t.Status AS TeamStatus,
                        s.Id AS SkillId,
                        s.Name AS SkillName,
                        s.IsGlobal AS SkillIsGlobal,
                        s.Status AS SkillStatus,
                       	FormulaID,
						FormulaName,
						FormulaIsGlobal,
						FormulaCreatedDate,
                        FormulaUpdatedDate,
                        FormulaStatus,
                        ft.Title as TaskName,
						ft.Id as TaskID,
						ft.IsGlobal as TaskIsGlobal,
						STUFF((
							select ',' + up2.FullName
							from FormulaTaskVendor FTV2
							LEFT JOIN UserProfile up2 ON FTV2.VendorGuid = up2.UserId
							where FTV2.FormulaTaskId = FTV.FormulaTaskId
							order by up2.FullName
							for xml path('')
						),1,1,'') as OutsourcerName
                        FROM 
                        Team t
                        LEFT JOIN teamskill ts ON t.Id=ts.teamid
                        LEFT JOIN Skill s ON s.Id=ts.skillid

						LEFT JOIN 
							(
								SELECT 
								AssignedSkillId , ft.IsGlobal, FT.TITLE, FT.ID,
								fp.id as FormulaID,
								fp.Name AS FormulaName,
								fp.IsGlobal AS FormulaIsGlobal,
								fp.DateCreated AS FormulaCreatedDate,
								fp.LastUpdated as FormulaUpdatedDate,
								fp.Status as FormulaStatus
								FROM FormulaTask ft join FormulaProject fp on ft.FormulaProjectId = fp.Id  and fp.IsGlobal=1 " + whereQuery +
                                @" ) ft
							on ft.AssignedSkillId = s.id and ft.IsGlobal=1  						
						LEFT JOIN FormulaTaskVendor FTV on ft.Id = ftv.FormulaTaskId and ftv.Status = 3   -- status 3 means certified vendor
						LEFT JOIN UserProfile up ON ftv.VendorGuid = up.UserId
					  WHERE
					  t.ISGLOBAL =1 AND t.STATUS = 5
					  and s.IsGlobal = 1 and s.status =5

                       UNION ALL-- Only Owner query: Global Team,Skills used to create custom Formulas.SELECT  DISTINCT                        t.Id AS Id,                        t.Name AS TeamName,                        t.IsGlobal AS TeamIsGlobal,                        t.Status AS TeamStatus,                        s.Id AS SkillId,                        s.Name AS SkillName,                        s.IsGlobal AS SkillIsGlobal,                        s.Status AS SkillStatus,                       	fp.id as FormulaID,						fp.Name AS FormulaName,						fp.IsGlobal AS FormulaIsGlobal,						fp.DateCreated AS FormulaCreatedDate,                        fp.LastUpdated as FormulaUpdatedDate,                        fp.Status as FormulaStatus,                        ft.Title as TaskName,						ft.Id as TaskID,						ft.IsGlobal as TaskIsGlobal,						--up.UserId as OutsourcerId,                         --up.FullName AS OutsourcerName 						stuff((							select ',' + up2.FullName							from FormulaTaskVendor FTV2							LEFT JOIN UserProfile up2 ON FTV2.VendorGuid = up2.UserId							where FTV2.FormulaTaskId = FTV.FormulaTaskId							order by up2.FullName							for xml path('')						),1,1,'') as OutsourcerName                   				FROM TEAMSKILL TS					JOIN TEAM T ON TS.TeamId = T.ID 					JOIN SKILL S ON TS.SkillId = S.ID and  s.IsGlobal = 1 and s.status =5					JOIN FormulaTask ft on ft.AssignedSkillId = s.id and ft.ownerguid = @ownerID					join FormulaProject fp on fp.id = ft.FormulaProjectId and fp.ownerguid = @ownerID				                       LEFT JOIN FormulaTaskVendor FTV on ft.Id = ftv.FormulaTaskId and ftv.Status = 3 				   				    LEFT JOIN UserProfile up ON ftv.VendorGuid = up.UserId				   where @ownerID <> '2A3B99E6-0A9F-42DD-15B5-08D7A66504B7'				   UNION ALL-- Only Owner query: Custom Team or Custome skills used to create custom formulasSELECT  DISTINCT                        t.Id AS Id,                        t.Name AS TeamName,                        t.IsGlobal AS TeamIsGlobal,                        t.Status AS TeamStatus,                        s.Id AS SkillId,                        s.Name AS SkillName,                        s.IsGlobal AS SkillIsGlobal,                        s.Status AS SkillStatus,                       	fp.id as FormulaID,						fp.Name AS FormulaName,						fp.IsGlobal AS FormulaIsGlobal,						fp.DateCreated AS FormulaCreatedDate,                        fp.LastUpdated as FormulaUpdatedDate,                        fp.Status as FormulaStatus,                        ft.Title as TaskName,						ft.Id as TaskID,						ft.IsGlobal as TaskIsGlobal,						--up.UserId as OutsourcerId,                         --up.FullName AS OutsourcerName 						stuff((							select ',' + up2.FullName							from FormulaTaskVendor FTV2							LEFT JOIN UserProfile up2 ON FTV2.VendorGuid = up2.UserId							where FTV2.FormulaTaskId = FTV.FormulaTaskId							order by up2.FullName							for xml path('')						),1,1,'') as OutsourcerName                   					FROM                    Team t                    LEFT JOIN teamskill ts ON t.Id=ts.teamid                    LEFT JOIN Skill s ON s.Id=ts.skillid					LEFT JOIN FormulaTask ft on ft.AssignedSkillId = s.id and ft.ownerguid = @ownerID					LEFT join FormulaProject fp on fp.id = ft.FormulaProjectId and fp.ownerguid = @ownerID			                       LEFT JOIN FormulaTaskVendor FTV on ft.Id = ftv.FormulaTaskId and ftv.Status = 3 				   				    LEFT JOIN UserProfile up ON ftv.VendorGuid = up.UserId				    where 					@ownerID <> '2A3B99E6-0A9F-42DD-15B5-08D7A66504B7'					AND  s.ownerguid = @ownerID ORDER BY  t.IsGlobal desc,S.IsGlobal DESC, t.Id";

            try
            {

                return await _repo.ExecuteSql<TeamDto>(_mapper, query,
                    new List<SqlParameter> {
                    new SqlParameter{ ParameterName = "@ownerID", SqlDbType = SqlDbType.UniqueIdentifier, Value = ownerID },
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<TeamSkillDto>> GetAllTeamsAsync()
        {
            bool isAdmin = _principal.IsAdmin();
            var query = _repo.Read<Team>();
            if (isAdmin)
            {
                query = query.Where(x => x.OwnerGuid == _principal.GetOwnerId());
            }
            else
            {
                query = query.Where(w => w.OwnerGuid == _principal.GetOwnerId() || (w.Status == (int)FormulaProjectStatus.UserTask && w.IsGlobal));

            }
            var result = await query.ProjectTo<TeamSkillDto>(_mapper.ConfigurationProvider)
                .OrderBy(o => o.Name)
                .ToListAsync();

            return result;
        }

        public async Task<IList<SkillDto>> GetUnusedSkills(int teamId)
        {
            try
            {

                Guid ownerID = _principal.GetOwnerId();

                var query = @"
                SELECT 
			    Id, DateCreated, LastUpdated, Name
			    FROM 
                Skill s
                LEFT JOIN 
                teamskill ts on ts.SkillId=s.id
                WHERE 
				ts.SkillId is NULL AND 
				s.OwnerGuid =@ownerID
                UNION
			    SELECT 
			    Id, DateCreated, LastUpdated, Name
			    FROM 
                Skill s
                LEFT JOIN 
                teamskill ts on ts.SkillId=s.id
                WHERE 
			    ts.TeamId = @teamID
                AND s.OwnerGuid =@ownerID order by 4";

                var unUsedSkillsData = await _repo.ExecuteSql<SkillDto>(_mapper, query,
                   new List<SqlParameter> {
                    new SqlParameter{ ParameterName = "@ownerID", SqlDbType = SqlDbType.UniqueIdentifier, Value = ownerID },
                    new SqlParameter{ ParameterName = "@teamID", SqlDbType = SqlDbType.Int, Value = teamId },
                    }).ToListAsync();



                //var unUsedSkillsData = await _repo.Read<Skill>()
                // .GroupJoin(_repo.Read<TeamSkill>(), inner => inner.Id, outer => outer.SkillId,
                // (s, ts) => new { skills = s, teamskills = ts.DefaultIfEmpty() })
                // .Select(x => new { Id = x.skills.Id, Name = x.skills.Name, TeamSkillId = x.teamskills.DefaultIfEmpty() })
                // .Where(x => x.TeamSkillId == null)
                // .ToListAsync();



                return Mapper.Map<List<SkillDto>>(unUsedSkillsData);
                // return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<TeamDto> AddTeamAsync(UpdateTeamDetailDto model)
        {
            if (model.Id > 0)
            {
                List<TeamSkill> tSkill = new List<TeamSkill>();

                if (model.TeamSkills != null)
                    foreach (var skillId in model.TeamSkills)
                    {
                        tSkill.Add(new TeamSkill { SkillId = skillId, TeamId = model.Id });
                    }
                _repo.AddRange(tSkill);

                await _repo.SaveChangesAsync();

                var team = await _repo.Read<Team>()
                    .Include(t => t.TeamSkills)
                    .ThenInclude(tu => tu.Skill)
                    .SingleAsync(t => t.Id == model.Id);

                return Mapper.Map<TeamDto>(team);
            }
            else
            {
                var teamEntity = Mapper.Map<Team>(model);
                teamEntity.OwnerGuid = _principal.GetOwnerId();
                teamEntity.DateCreated = _dateTimeService.NowUtc;
                teamEntity.IsGlobal = _principal.IsAdmin(); // here we will add condition for Admin
                teamEntity.Status = (_principal.IsAdmin()) ? (int?)FormulaProjectStatus.UserTask : null;
                if (model.TeamSkills != null)
                    foreach (var skillId in model.TeamSkills)
                    {
                        teamEntity.TeamSkills.Add(new TeamSkill { SkillId = skillId });
                    }

                await _repo.AddAsync(teamEntity);

                await _repo.SaveChangesAsync();
                var team = await _repo.Read<Team>()
                    .Include(t => t.TeamSkills)
                    .ThenInclude(tu => tu.Skill)
                    .SingleAsync(t => t.Id == teamEntity.Id);

                return Mapper.Map<TeamDto>(team);
            }
        }

        public async Task<UpdateTeamDetailDto> GetTeamAsync(int teamId)
        {
            var team = await _repo.Read<Team>()
                .Include(i => i.TeamSkills)
                .FirstOrDefaultAsync(w => w.Id == teamId);

            //if (team.OwnerGuid != _principal.GetOwnerId())
            //    throw new ValidationException("You have no access to skill.");

            var model = Mapper.Map<UpdateTeamDetailDto>(team);
            return model;
        }

        public async Task<TeamDto> UpdateTeamAsync(UpdateTeamDetailDto model)
        {
            using (var trx = _repo.Transaction())
            {
                var team = await trx.Track<Team>()
                    .Include(c => c.TeamSkills)
                    .FirstOrDefaultAsync(p => p.Id == model.Id);

                if (team == null)
                    throw new ValidationException("Team is not found.");
                if (team.OwnerGuid != _principal.GetOwnerId())
                    throw new ValidationException("You have no access to Team.");

                Mapper.Map(model, team);
                team.LastUpdated = _dateTimeService.NowUtc;

                foreach (var teamSkill in team.TeamSkills.Where(w => !model.TeamSkills.Contains(w.SkillId)))
                {
                    trx.Remove(teamSkill);
                }

                foreach (var skillId in model.TeamSkills.Where(w => team.TeamSkills.All(a => a.SkillId != w)))
                {
                    await trx.AddAsync(new TeamSkill
                    {
                        SkillId = skillId,
                        TeamId = model.Id
                    });
                }

                await trx.SaveAndCommitAsync();
                team = await trx.Read<Team>()
                    .Include(c => c.TeamSkills)
                    .FirstOrDefaultAsync(p => p.Id == model.Id);
                return Mapper.Map<TeamDto>(team);
            }
        }


        public async Task DeleteTeamAsync(int teamId)
        {
            using (var trx = _repo.Transaction())
            {
                var team = await trx.Track<Team>()
                    .Include(c => c.ReviewingTasks)
                    .Include(c => c.AssignedTasks)
                    .FirstOrDefaultAsync(p => p.Id == teamId);

                if (team == null)
                    throw new ValidationException("Team is not found.");
                if (team.OwnerGuid != _principal.GetOwnerId())
                    throw new ValidationException("You have no access to team.");

                if (team.AssignedTasks.Count > 0)
                    throw new ValidationException("You cannot delete team becouse its skill assigned to task");

                trx.RemoveRange(team.TeamSkills);
                trx.Remove(team);

                await trx.SaveAndCommitAsync();
            }
        }

        public async Task<PublishStatus> PublishFormula(int formulaId)
        {
            PublishStatus result = new PublishStatus();
            var formulaStatus = await _repo.ExecuteSql<PublishFormula>(_mapper, "usp_PublishFormula @FormulaID",
        new List<SqlParameter> { new SqlParameter { ParameterName = "@FormulaID",
                SqlDbType = SqlDbType.Int, Value = formulaId } }).ToListAsync();


            var tasksList = formulaStatus.Where(x => x.Type == "Formula").ToList();

            foreach (var list in tasksList)
            {
                await _formulaService.CopyResources((int)list.OldFormulaTaskId, list.NewFormulaTaskId, list.NewFormulaId);
            }

            PublishFormulaList obj = new PublishFormulaList();

            string skillList = "";
            string teamList = "";
            int newFormulaId = 0;

            bool notificationFlag = false;

            if (formulaStatus.Count == 0)
            {
                throw new ValidationException("Error in publishing the formula!");
            }

            foreach (var item in formulaStatus)
            {
                var itemType = item.Type;

                if (itemType == "Formula" && !notificationFlag)
                {
                    notificationFlag = true;
                    newFormulaId = item.NewFormulaId;
                    obj.Formula = "Formula <b>" + item.Name + "</b> has been " + item.Status + " to your Account";
                }

                else if (itemType == "Skill")
                {
                    skillList += "Skill <b>" + item.Name + "</b> has been " + item.Status + " to your Account<br/>";
                }

                else if (itemType == "Team")
                {
                    teamList += "Team <b>" + item.Name + "</b> has been " + item.Status + " to your Account<br/>";
                }
            }
            obj.SkillList = skillList;
            obj.TeamList = teamList;

            await _notificationService.SendPublishFormulaNotification(obj);

            result.Result = 1;
            return result;

        }

        public async Task<IList<PublishStatus>> PublishTeam(int teamId)
        {
            var team = await _repo.Read<Team>()
              .FirstOrDefaultAsync(p => p.Id == teamId);

            var formulastatus = await _repo.ExecuteSql<PublishStatus>(_mapper, "usp_PublishTeam @TeamID",
            new List<SqlParameter> { new SqlParameter { ParameterName = "@TeamID",
                SqlDbType = SqlDbType.Int, Value = teamId } }).ToListAsync();


            if (formulastatus.FirstOrDefault().Result == 1)
            {

                PublishFormulaList obj = new PublishFormulaList();

                obj.Formula = "";

                obj.SkillList += "";

                string txt = team.LastUpdated > team.DateCreated ? "Updated" : "Added";

                obj.TeamList += "Team <b>" + team.Name + "</b> has been " + txt + " to your Account</li>";

                await _notificationService.SendPublishFormulaNotification(obj);
            }

            return formulastatus;

        }

        public async Task<IList<PublishStatus>> PublishSkill(int skillId)
        {
            try
            {
                var skill = await _repo.Read<Skill>()
                    .Include(i => i.TeamSkills).ThenInclude(i => i.Team)
              .FirstOrDefaultAsync(p => p.Id == skillId);

                var formulastatus = await _repo.ExecuteSql<PublishStatus>(_mapper, "usp_PublishSkill @SkillID",
                new List<SqlParameter> { new SqlParameter { ParameterName = "@SkillID",
                SqlDbType = SqlDbType.Int, Value = skillId } }).ToListAsync();

                if (formulastatus.FirstOrDefault().Result == 1)
                {
                    bool addTeam = skill.TeamSkills.FirstOrDefault().Team.Status == 3 ? false : true;

                    PublishFormulaList obj = new PublishFormulaList();

                    obj.Formula = "";

                    string skillTxt = skill.LastUpdated > skill.DateCreated ? "Updated" : "Added";

                    obj.SkillList += "Skill <b>" + skill.Name + "</b> has been " + skillTxt + " to your Account";

                    string teamTxt = skill.TeamSkills.FirstOrDefault().Team.LastUpdated >
                        skill.TeamSkills.FirstOrDefault().Team.DateCreated ? " Updated" : " Added";

                    obj.TeamList += addTeam ? "Team <b>" + skill.TeamSkills.FirstOrDefault().Team.Name + "</b> has been " + teamTxt + " to your Account"
                        : "";

                    await _notificationService.SendPublishFormulaNotification(obj);
                }

                return formulastatus;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
