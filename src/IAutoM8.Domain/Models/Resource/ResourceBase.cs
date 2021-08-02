namespace IAutoM8.Domain.Models.Resource
{
    public class ResourceBase
    {
        public int ResourceId { get; set; }
        public virtual Resource Resource { get; set; }
    }
}
