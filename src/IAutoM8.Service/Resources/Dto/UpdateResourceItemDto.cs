namespace IAutoM8.Service.Resources.Dto
{
    public class UpdateResourceItemDto
    {
        public long Id { get; set; }
        public bool IsShared { get; set; }
        public bool IsGlobalShared { get; set; }
    }
}
