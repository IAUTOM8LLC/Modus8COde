namespace IAutoM8.Domain.Models.Resource
{
    public class ResourceProject : ResourceBase
    {
        public int ProjectId { get; set; }
        
        public virtual Project.Project Project { get; set; }
    }
}
