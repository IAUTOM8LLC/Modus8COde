using IAutoM8.Domain.Models.Project.Task;

namespace IAutoM8.Domain.Models.Resource
{
    public class ResourceProjectTask : ResourceBase
    {
        public int ProjectTaskId { get; set; }
        
        public virtual ProjectTask ProjectTask { get; set; }
    }
}
