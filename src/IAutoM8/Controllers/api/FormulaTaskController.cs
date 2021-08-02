using IAutoM8.Controllers.Abstract;
using IAutoM8.Global;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.FormulaTasks.Dto;
using IAutoM8.Service.FormulaTasks.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Sockets.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Controllers.api
{
    [Authorize]
    public class FormulaTaskController : BaseController
    {
        private readonly IFormulaTaskService _taskService;
        private readonly IFormulaTaskOutsourcesService _formulaTaskOutsourcesService;

        public FormulaTaskController(IFormulaTaskService taskService,
            IFormulaTaskOutsourcesService formulaTaskOutsourcesService)
        {
            _taskService = taskService;
            _formulaTaskOutsourcesService = formulaTaskOutsourcesService;
        }

        #region CRUD

        [HttpGet]
        [Route("formula/{formulaId:int}")]
        public async Task<IActionResult> GetTasks(int formulaId)
        {
            var result = await _taskService.GetTasksAsync(formulaId);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-disabled-tasks")]
        public async Task<IActionResult> GetDisabledTasksAsync([FromQuery] DisabledFormulaTaskDto queryParams)
        {
            if (!queryParams.ParentFormulaId.HasValue || !queryParams.ChildFormulaId.HasValue || !queryParams.ParentFormulaTaskId.HasValue)
            {
                return Ok();
            }

            var result = await _taskService
                .GetDisabledTasksAsync(queryParams.ParentFormulaId.Value, queryParams.ChildFormulaId.Value, queryParams.ParentFormulaTaskId.Value);

            return Ok(result);
        }

        [HttpGet]
        [Route("formula-status/{formulaId:int}")]
        public async Task<IActionResult> GetFormulaLockStatus(int formulaId)
        {
            var result = await _taskService.GetFormulaLockStatus(formulaId);
            return Ok(result);
        }

        [HttpGet]
        [Route("{taskId:int}/{formulaId:int}")]
        public async Task<IActionResult> GetTask(int taskId, int formulaId)
        {
            var result = await _taskService.GetTaskAsync(taskId, formulaId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddTask([FromBody] UpdateFormulaTaskDto model)
        {
            var task = await _taskService.AddTaskAsync(model);
            return Ok(task);
        }

        [HttpPost]
        [Route("add-formula-task")]
        public async Task<IActionResult> AddFormulaTask([FromBody]AddFormulaTaskDto model)
        {
            var task = await _taskService.AddFormulaTaskAsync(model);
            return Ok(task);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTask([FromBody]UpdateFormulaTaskDto model)
        {
            var task = await _taskService.UpdateTaskAsync(model);
            return Ok(task);
        }

        [HttpDelete]
        [Route("{taskId:int}")]
        public async Task<IActionResult> DeleteTask([FromRoute] int taskId)
        {
            await _taskService.DeleteTask(taskId);
            return Ok();
        }

        #endregion

        [HttpGet]
        [Route("{taskId:int}/group-members-count")]
        public async Task<IActionResult> GetGroupTaskCount(int taskId)
        {
            var result = await _taskService.GetGroupTaskCount(taskId);
            return Ok(result);
        }

        [HttpPut]
        [Route("position")]
        public async Task<IActionResult> UpdateTasksPosition([FromBody]List<TaskPositionDto> list)
        {
            await _taskService.UpdateTasksPositionAsync(list);
            return Ok();
        }

        [HttpPut]
        [Route("connect")]
        public async Task<IActionResult> AddTaskDependency([FromBody]FormulaTaskDependencyDto model)
        {
            await _taskService.AddTaskDependency(model);
            return Ok();
        }

        [HttpPut]
        [Route("disconnect")]
        public async Task<IActionResult> RemoveTaskDependency([FromBody]FormulaTaskDependencyDto model)
        {
            await _taskService.RemoveTaskDependency(model);
            return Ok();
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

        [HttpGet]
        [Route("outsources/{id:int}/{skip:int}/{count:int}")]
        public async Task<IActionResult> Outsources(int id, int skip, int count)
        {
            return Ok(await _formulaTaskOutsourcesService.GetOutsources(id, (short)skip, (byte)count));
        }

        [HttpPut]
        [Route("outsources")]
        public async Task<IActionResult> Outsources([FromBody]OutsourceRequestDto requestDto)
        {
            await _formulaTaskOutsourcesService.CreateRequest(requestDto);
            return Ok();
        }

        [HttpGet]
        [Route("notes/{formulaId:int}")]
        public async Task<IActionResult> GetNotes(int formulaId)
        {
            var result = await _taskService.GetFormulaNotesAsync(formulaId);
            return Ok(result);
        }

        [HttpPost]
        [Route("notes")]
        public async Task<IActionResult> AddNotes([FromBody] AddFormulaNotesDto model)
        {
            var result = await _taskService.AddFormulaNotesAsync(model);
            return Ok(result);
        }

        [HttpDelete]
        [Route("notes/{noteId:int}")]
        public async Task<IActionResult> DeleteNotes(int noteId)
        {
            await _taskService.DeleteFormulaNotesAsync(noteId);
            return Ok();
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut]
        [Route("lock-training")]
        public async Task<IActionResult> LockTraining([FromBody]int taskId)
        {
            await _taskService.SetTrainingLockStatus(taskId, true);
            return Ok(taskId);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut]
        [Route("unlock-training")]
        public async Task<IActionResult> UnlockTraining([FromBody]int taskId)
        {
            await _taskService.SetTrainingLockStatus(taskId, false);
            return Ok(taskId);
        }

        [HttpPost]
        [Route("disable-formula-task")]
        public async Task<IActionResult> DisableFormulaTask([FromBody] AddFormulaTaskDisableStatusDto model)
        {
            await _taskService.DisableFormulaTask(model);
            return Ok();
        }

        [HttpDelete]
        [Route("enable-formula-task")]
        public async Task<IActionResult> EnableFormulaTask([FromBody] AddFormulaTaskDisableStatusDto model)
        {
            await _taskService.EnableFormulaTask(model);
            return Ok();
        }

        [HttpPost]
        [Route("add-neo-tasks")]
        public async Task<IActionResult> AddNeoFormulaTasks([FromBody]int formulaId)
        {
            await _taskService.AddPublishedTaskToNeo4j(formulaId);
            return Ok();
        }
    }
}
