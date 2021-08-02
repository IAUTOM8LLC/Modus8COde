using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Service.ProjectTasks.Dto;

namespace IAutoM8.Service.ProjectTasks.Interfaces
{
    public interface ITaskCommentService
    {
        Task<CommentDto> AddCommentAsync(AddCommentDto comment);
        Task<int> DeleteCommenAsynct(int commentId);
        Task<List<CommentDto>> GetCommentsAsync(int taskId);
    }
}
