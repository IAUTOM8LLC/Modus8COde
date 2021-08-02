using System;
using IAutoM8.Controllers.Abstract;
using IAutoM8.Global.Enums;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Service.Projects.Interfaces;
using Newtonsoft.Json.Linq;

namespace IAutoM8.Controllers.api
{
    [Authorize]
    public class TasksController : BaseController
    {
        private readonly ITaskService _taskService;
        private readonly ITaskHistoryService _taskHistoryService;
        private readonly ITaskFormulaService _taskFormulaService;
        private readonly IProjectService _projectService;
        private readonly IProjectTaskOutsourcesService _projectTaskOutsourcesService;

        public TasksController(ITaskService taskService,
            ITaskHistoryService taskHistoryService,
            ITaskFormulaService taskFormulaService,
            IProjectService projectService,
            IProjectTaskOutsourcesService projectTaskOutsourcesServic)
        {
            _taskService = taskService;
            _taskHistoryService = taskHistoryService;
            _taskFormulaService = taskFormulaService;
            _projectService = projectService;
            _projectTaskOutsourcesService = projectTaskOutsourcesServic;
        }

        #region Task CRUD

        [HttpGet]
        [Route("{taskId:int}")]
        public async Task<IActionResult> GetTask(int taskId)
        {
            var result = await _taskService.GetTaskAsync(taskId);
            return Ok(result);
        }

        [HttpGet]
        [Route("projects/{projectId:int}")]
        public async Task<IActionResult> GetTasks(int projectId)
        {
            var result = await _taskService.GetTasksAsync(new List<int> { projectId });
            return Ok(result);
        }

        [HttpGet]
        [Route("projects/resources/{projectId:int}")]
        public async Task<IActionResult> GetProjectTaskResources(int projectId)
        {
            var result = await _taskService.GetProjectTaskResourcesAsync(projectId);
            return Ok(result);
        }

        [HttpGet]
        [Route("{statuses}/{skip:int}/{take:int}")]
        public async Task<IActionResult> GetTasksInStatus(string statuses, int skip, int take)
        {
            var myProjectIds = await _projectService.GetOwnProjectsIds();
            return Ok(new
            {
                status = statuses,
                items = await _taskService.GetTasksInStatusAsync(myProjectIds, statuses, skip, take)
            });
        }

        [HttpGet]
        [Route("{userId}/{statuses}/{skip:int}/{take:int}")]
        public async Task<IActionResult> GetOwnerUserTasksInStatus(Guid userId, string statuses, int skip, int take)
        {
            var myProjectIds = await _projectService.GetOwnProjectsIds(userId);
            return Ok(new
            {
                status = statuses,
                items = await _taskService.GetTasksInStatusAsync(myProjectIds, statuses, skip, take, userIdToLoad: userId)
            });
        }

        [HttpGet]
        [Route("alltasks")]
        public async Task<IActionResult> GetUserTasksInStatus()
        {
            var result = await _taskService.GetTasksInStatusAsync(UserGuid);
            return Ok(result);
        }

        [HttpGet]
        [Route("vendor-tasks")]
        public async Task<IActionResult> GetVendorsInStatus()
        {
            var result = await _taskService.GetVendorTasksInStatusAsync(UserGuid);
            return Ok(result);
        }

        [HttpGet]
        [Route("projects/getTasksFromProjects")]
        public async Task<IActionResult> GetTasksFromAllProjects(IEnumerable<int> projectIds)
        {
            var result = await _taskService.GetTasksAsync(projectIds, false);
            return Ok(result);
        }

        [HttpPost]
        [Route("{statuses}/{skip:int}/{take:int}")]
        public async Task<IActionResult> GetSelectedUserTasksInStatus(string statuses, int skip, int take, [FromBody] List<Guid> userIds)
        {
            var myProjectIds = await _projectService.GetOwnProjectsIds();
            return Ok(new
            {
                status = statuses,
                items = await _taskService.GetTasksInStatusAsync(myProjectIds, statuses, skip, take)
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddTask([FromBody]UpdateTaskDto taskDto)
        {
            var task = await _taskService.AddTaskAsync(taskDto);
            return Ok(task);
        }

        [HttpPost]
        [Route("add-formula-task")]
        public async Task<IActionResult> AddFormulaTask([FromBody]TaskFormulaDto taskDto)
        {
            var tasks = await _taskFormulaService.CreateFormulaTaskAsync(taskDto);
            return Ok(tasks);
        }

        [HttpPut]
        [Route("{taskId:int}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody]UpdateTaskDto taskDto)
        {
            var task = await _taskService.UpdateTaskAsync(taskId, taskDto);
            return Ok(task);
        }

        [HttpDelete]
        [Route("{taskId:int}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            await _taskService.DeleteTask(taskId);
            return Ok();
        }

        #endregion

        [HttpPut]
        [Route("{taskId:int}/publish-resource")]
        public async Task<IActionResult> PublishTaskResource(int taskId, [FromBody] PublishTaskResourceDto model)
        {
            var result = await _taskService.PublishTaskResourceAsync(taskId, model);
            return Ok(result);
        }

        [HttpPut]
        [Route("{taskId:int}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, [FromBody] StatusDto model)
        {
            var task = await _taskService.UpdateTaskStatusAsync(taskId, model);
            return Ok(task);
        }

        [HttpPut]
        [Route("{taskId:int}/notification")]
        public async Task<IActionResult> UpdateTaskNotification(int taskId, [FromBody] NotificationDto model)
        {
            var task = await _taskService.UpdateTaskNotificationAsync(taskId, model);
            return Ok(task);
        }

        [HttpPut]
        [Route("stopOutsource")]
        public async Task<IActionResult> StopOutsource(int taskId)
        {
            var task = await _taskService.StopOutsource(taskId);
            return Ok(task);
        }

        [HttpPut]
        [Route("position")]
        public async Task<IActionResult> UpdateTasksPosition([FromBody]List<TaskPositionDto> taskPositionList)
        {
            await _taskService.UpdateTasksPositionAsync(taskPositionList);
            return Ok();
        }

        [HttpPut]
        [Route("connect")]
        public async Task<IActionResult> ConnectTasks([FromBody]TaskDependencyDto model)
        {
            await _taskService.AddTaskDependency(model);
            return Ok();
        }

        [HttpPut]
        [Route("disconnect")]
        public async Task<IActionResult> DisconnectTasks([FromBody]TaskDependencyDto model)
        {
            await _taskService.RemoveTaskDependency(model);
            return Ok();
        }

        [HttpPost]
        [Route("processingUser")]
        public async Task<IActionResult> UpdateProcessingUser([FromBody]ProcessingUserDto model)
        {
            var task = await _taskService.ChangeProcessingUser(model);
            return Ok(task);
        }

        [HttpPost]
        [Route("changeProcessingUser")]
        public async Task<IActionResult> ChangeProcessingUser([FromBody] ProcessingUserDto model)
        {
            var task = await _taskService.ChangeNewProcessingUser(model);
            return Ok(task);
        }


        [HttpPut]
        [Route("{taskId:int}/attach-option")]
        public async Task<IActionResult> AttachTaskToOption(int taskId, [FromBody] int optionId)
        {
            await _taskService.AssignTaskToConditionOption(optionId, taskId);
            return Ok(taskId);
        }

        [HttpPut]
        [Route("{taskId:int}/detach-option")]
        public async Task<IActionResult> DettachTaskFromOption(int taskId, [FromBody]int optionId)
        {
            await _taskService.AssignTaskToConditionOption(optionId, null);
            return Ok(taskId);
        }

        [HttpPost]
        [Route("conditional/{taskId:int}/complete")]
        public async Task<IActionResult> SelectConditionOption(int taskId, [FromBody] ConditionOptionDto model)
        {
            await _taskService.CompleteConditionalTask(taskId, model);
            return Ok(taskId);
        }

        [HttpGet]
        [Route("history/{projectId:int}")]
        public async Task<IActionResult> GetHistory(int projectId)
        {
            return Ok(await _taskHistoryService.GetTaskHistoryByProjectId(projectId));
        }

        [HttpGet]
        [Route("getAllProjectsHistory")]
        public async Task<IActionResult> GetAllProjectsHistory()
        {
            var myProjectIds = await _projectService.GetOwnProjectsIds();
            var result = await _taskHistoryService.GetTasksHistory(myProjectIds);
            return Ok(result);
        }

        [HttpPut]
        [Route("do-task/{taskId:int}")]
        public async Task<IActionResult> DoTask(int taskId)
        {
            var task = await _taskService.DoTask(taskId);
            return Ok(task);
        }

        [HttpPut]
        [Route("review-task/{taskId:int}")]
        public async Task<IActionResult> ReviewTask(int taskId)
        {
            var task = await _taskService.ReviewTask(taskId);
            return Ok(task);
        }

        [HttpPut]
        [Route("do-vendor-task/{taskId:int}")]
        public async Task<IActionResult> DoVendorTask(int taskId)
        {
            var task = await _taskService.DoVendorTask(taskId);
            return Ok(task);
        }


        [HttpPut]
        [Route("outsources")]
        public async Task<IActionResult> Outsources([FromBody]OutsourceRequestDto requestDto)
        {
            await _projectTaskOutsourcesService.CreateRequest(requestDto);
            return Ok();
        }

        [HttpGet]
        [Route("notes/{taskId:int?}")]
        public async Task<IActionResult> GetNotes(int? taskId)
        {
            if (taskId == null)
            {
                return Ok(new List<ProjectNotesDto>());
            }

            var result = await _taskService.GetTaskNotesAsync(taskId);
            return Ok(result);
        }

        [HttpPost]
        [Route("notes")]
        public async Task<IActionResult> AddNote([FromBody]JObject requestData)
        {
            var projectNote = requestData["projectNote"].ToObject<AddProjectNotesDto>();
            var sharedTaskIds = requestData["sharedTaskIds"].ToObject<List<int>>();

            var result = await _taskService.AddTaskNotesAsync(projectNote);

            if (result.FormulaId != 0)
            {
                await _taskService.ShareProjectNote(projectNote, sharedTaskIds);
            }

            return Ok(result);
        }

        [HttpPut]
        [Route("notes/{noteId:int}/{isPublished:bool}")]
        public async Task<IActionResult> UpdateNote(int noteId, bool isPublished)
        {
            var result = await _taskService.UpdateTaskNotesAsync(noteId, isPublished);
            return Ok(result);
        }

        [HttpDelete]
        [Route("notes/{noteId:int}")]
        public async Task<IActionResult> DeleteNote(int noteId)
        {
            await _taskService.DeleteTaskNotesAsync(noteId);
            return Ok();
        }

        [HttpGet]
        [Route("vendorjobinvites")]
        public async Task<IActionResult> GetVendorInvites()
        {
            IList<VendorJobInvitesDto> result = await _taskService.GetVendorJobInvites(UserGuid);
            return Ok(result);

        }

        [HttpPost]
        [Route("cancelOutsource")]
        public async Task<IActionResult> StopVendorTaskOnCancelNudge([FromBody]int projectTaskId)
        {
            await _taskService.StopVendorTaskOnCancelNudge(projectTaskId);
            return Ok();
        }

        [HttpPost]
        [Route("expired-job-invites")]
        public async Task<IActionResult> UpdateExpiredJobInvitesToOwner([FromBody] int taskId)
        {
            await _taskService.UpdateExpiredJobInvitesToOwner(taskId);
            return Ok();
        }

        [HttpGet]
        [Route("getTaskInStatusById/{taskId:int}")]
        public async Task<IActionResult> GetTaskInStatusById(int taskId)
        {
            var result = await _taskService.GetTaskInStatusById(taskId);
            return Ok(result);
        }

        [HttpPatch]
        [Route("update-task-checklist")]
        public async Task<IActionResult> UpdateTaskChecklist([FromBody] List<UpdateTaskChecklistDto> todos)
        {
            await _taskService.UpdateTaskChecklist(todos);
            return Ok();
        }

        [HttpGet]
        [Route("getdownstreamsharedtasks/{taskId:int}")]
        public async Task<IActionResult> GetDownstreamSharedTasks(int taskId)
        {
            var result = await _taskService.GetDownstreamShareIds(taskId);
            return Ok(result);
        }

        [HttpGet]
        [Route("getformulasharedtasks/{taskId:int}")]
        public async Task<IActionResult> GetFormulaSharedTasks(int taskId)
        {
            var result = await _taskService.GetFormulaShareIds(taskId);
            return Ok(result);
        }
    }
}
