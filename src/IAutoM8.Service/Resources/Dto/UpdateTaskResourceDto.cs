using System.Collections.Generic;

namespace IAutoM8.Service.Resources.Dto
{
    public class UpdateTaskResourceDto
    {
        public int Id { get; set; }
        public List<long> ToDeleteList { get; set; }
        public List<FileResourceDto> ToAddFileList { get; set; }
        public List<UrlResourceDto> ToAddUrlList { get; set; }
        public List<UpdateResourceItemDto> UpdateResourceList { get; set; }
    }
}
