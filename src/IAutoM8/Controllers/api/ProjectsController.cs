using IAutoM8.Controllers.Abstract;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Formula.Interfaces;
using IAutoM8.Service.Projects;
using IAutoM8.Service.Projects.Dto;
using IAutoM8.Service.Projects.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace IAutoM8.Controllers.api
{
    [Authorize]
    public class ProjectsController : BaseController
    {
        private readonly IProjectService _projectService;
        private readonly IFormulaService _formulaService;
        private readonly IScheduleService scheduleService;

        public ProjectsController(
            IProjectService projectService,
            IFormulaService formulaService,
            IScheduleService scheduleService)
        {
            _projectService = projectService;
            _formulaService = formulaService;
            this.scheduleService = scheduleService;
        }

        [HttpGet]
        public async Task<List<ProjectDto>> GetProjects()
        {
            return await _projectService.GetOwnProjects();
        }

        [HttpGet("recent")]
        public async Task<int> GetRecentProject()
        {
            return await _projectService.GetMostRecentId();
        }

        [HttpGet]
        [Route("{projectId:int}")]
        public async Task<ProjectDto> GetProject(int projectId)
        {
            return await _projectService.GetProject(projectId);
        }

        //[Authorize(Roles = OwnerOrManager)]
        //[Route("addBulkProject")]
        //[HttpPost]
        //public void ImportBulkProject(string path)
        //{
        //    _projectService.BulkImportProject(path);
        //}

        [Authorize(Roles = OwnerOrManager)]
        [HttpPost]
        public async Task<JsonResult> AddProject([FromBody] AddProjectDto data)
        {
            var project = await _projectService.AddProject(data);
            return Json(project);
        }

        [Authorize(Roles = OwnerOrManager)]
        [HttpGet]
        [Route("child-projects/{parentProjectId:int}")]
        public async Task<IActionResult> GetChildProjects(int parentProjectId)
        {
            var result = await _projectService.GetChildProjets(parentProjectId);
            return Ok(result);
        }

        //[Authorize(Roles = OwnerOrManager)]
        //[HttpGet]
        //[Route("grand-child-projects/{parentProjectId:int}")]
        //public async Task<IActionResult> GetGrandChildProjects(int parentProjectId)
        //{
        //    var result = await _projectService.GetGrandChildProjets(parentProjectId);
        //    return Ok(result);
        //}

        [Authorize(Roles = OwnerOrManager)]
        [HttpPost]
        [Route("children")]
        public async Task<JsonResult> AddChildProject([FromBody] AddChildProjectDto data)
        {
            var project = await _projectService.AddChildProject(data);
            return Json(project);
        }

        [Authorize(Roles = OwnerOrManager)]
        [HttpPut]
        [Route("children")]
        public async Task<JsonResult> UpdateChildProject([FromBody] AddChildProjectDto data)
        {
            var project = await _projectService.UpdateChildProject(data);
            return Json(project);
        }

        [Authorize(Roles = OwnerOrManager)]
        [HttpPut]
        public async Task<IActionResult> UpdateProject([FromBody] ProjectDto data)
        {
            var edited = await _projectService.UpdateProject(data);
            return Ok(edited);
        }



        [Authorize(Roles = OwnerOrManager)]
        [HttpDelete]
        [Route("{projectId:int}")]
        public async Task<IActionResult> DeleteProject([FromRoute] int projectId)
        {
            await _projectService.DeleteProject(projectId);
            return Ok();
        }

        [Authorize(Roles = OwnerOrManager)]
        [HttpDelete]
        [Route("children/{projectId:int}")]
        public async Task<IActionResult> DeleteChildProject([FromRoute] int projectId)
        {
            await _projectService.DeleteChildProject(projectId);
            return Ok();
        }

        [Authorize(Roles = OwnerOrManager)]
        [HttpPut]
        [Route("{projectId:int}/import-tasks")]
        public async Task<IActionResult> ImportTasks(int projectId, [FromBody]ImportTasksDto model)
        {
            await _formulaService.ImportTasksIntoProject(projectId, model);
            return Ok();
        }

        [HttpGet]
        [Route("{projectId:int}/users")]
        public async Task<IEnumerable<AssignedUserDto>> GetAssignedUsers(int? projectId)
        {
            return await _projectService.GetAssignedUsers(projectId);
        }

        ////[Authorize(Roles = OwnerOrManager)]
        //[Route("addBulkProject/{path}")]
        //[HttpPost]
        //public void ImportBulkProject([FromRoute] string path)
        //{
        //    _projectService.BulkImportProject(path);
        //}

        //[Authorize(Roles = OwnerOrManager)]
        [Route("addBulkProject")]
        [HttpGet]
        public void ImportBulkProject(string path)
        {
            _projectService.BulkImportProject(path);
        }
    }
}
