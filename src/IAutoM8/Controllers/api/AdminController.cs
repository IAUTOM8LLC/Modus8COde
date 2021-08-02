using IAutoM8.Controllers.Abstract;
using IAutoM8.Global;
using IAutoM8.Service.Projects.Dto;
using IAutoM8.Service.Projects.Interfaces;
using IAutoM8.Service.Users.Dto;
using IAutoM8.Service.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Controllers.api
{
    [Authorize(Roles = OwnerOrManagerOrAdmin)]
    public class AdminController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;

        public AdminController(IUserService userService, IProjectService projectService)
        {
            _userService = userService;
            _projectService = projectService;
        }

        [HttpGet]
        [Route("getUsersForAllProjects")]
        public async Task<IActionResult> GetUsersForAllProjects()
        {
            var myProjectIds = await _projectService.GetOwnProjectsIds();
            var users = await _userService.GetUsersForAllProjects(myProjectIds);
            return Ok(users);
        }

        [Route("assignuser")]
        [HttpPost]
        public async Task<IActionResult> AssignUser([FromBody] AssigneeUserDto user)
        {
            await _userService.CreateAssigneeUser(User.Identity.Name, user);
            return Ok();
        }

        [Route("assigned-users")]
        [HttpGet]
        public async Task<List<CompanyUserDto>> GetAssignedUsers()
        {
            return await _userService.GetOwnerUsersWithRoles(User.Identity.Name);
        }

        [Route("assignuserstoproject")]
        [HttpPut]
        public async Task<IActionResult> AssignUsersToProject([FromBody] AssignUsersToProjectDto model)
        {
            await _projectService.AssignToProject(model);
            return Ok();
        }

        [Route("makemanager")]
        [HttpPut]
        public async Task<IActionResult> MakeManager([FromBody] AssigneeUserDto user)
        {
            await _userService.ChangeRoleTo(user, UserRoles.Manager);
            return Ok();
        }

        [Route("makeworker")]
        [HttpPut]
        public async Task<IActionResult> MakeWorker([FromBody] AssigneeUserDto user)
        {
            await _userService.ChangeRoleTo(user, UserRoles.Worker);
            return Ok();
        }

        [Route("resend")]
        [HttpPut]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] Guid userGuid)
        {
            await _userService.ResendConfirmationEmail(userGuid);
            return Ok();
        }

        [Route("lockuser")]
        [HttpPut]
        public async Task<IActionResult> LockUser([FromBody] Guid userGuid)
        {
            var userId = await _userService.SetLockStatusForUser(userGuid, @lock:true);
            return Ok(userId);
        }

        [Route("unlockuser")]
        [HttpPut]
        public async Task<IActionResult> UnlockUser([FromBody] Guid userGuid)
        {
            var userId = await _userService.SetLockStatusForUser(userGuid, @lock: false);
            return Ok(userId);
        }

        [Route("{userGuid:Guid}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(Guid userGuid)
        {
            await _userService.DeleteUser(userGuid);
            return Ok(await _userService.GetOwnerUsersWithRoles(User.Identity.Name));
        }
    }
}
