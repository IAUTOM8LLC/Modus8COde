using IAutoM8.Controllers.Abstract;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IAutoM8.Controllers.api
{
    [Authorize]
    [Route("api/taskComment")]
    public class TaskCommentController : BaseController
    {
        private readonly ITaskCommentService _taskCommentService;

        public TaskCommentController(ITaskCommentService taskCommentService)
        {
            _taskCommentService = taskCommentService;
        }

        #region Task Comment CRUD

        [HttpGet]
        [Route("{taskId:int}")]
        public async Task<IActionResult> GetComments(int taskId)
        {
            var result = await _taskCommentService.GetCommentsAsync(taskId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody]AddCommentDto commentDto)
        {
            var task = await _taskCommentService.AddCommentAsync(commentDto);
            return Ok(task);
        }

        [HttpDelete]
        [Route("{commentId:int}")]
        public async Task<IActionResult> DeleteTask(int commentId)
        {
           var id = await _taskCommentService.DeleteCommenAsynct(commentId);
            return Ok(id);
        }

        #endregion
    }
}
