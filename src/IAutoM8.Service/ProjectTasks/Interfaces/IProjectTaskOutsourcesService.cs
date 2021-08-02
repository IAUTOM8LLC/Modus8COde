using IAutoM8.Service.CommonDto;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks.Interfaces
{
    public interface IProjectTaskOutsourcesService
    {
        Task CreateRequest(OutsourceRequestDto requestDto);
    }
}
