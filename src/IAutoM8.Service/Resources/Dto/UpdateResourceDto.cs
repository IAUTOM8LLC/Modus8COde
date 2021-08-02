using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace IAutoM8.Service.Resources.Dto
{
    public class UpdateResourceDto
    {
        public int Id { get; set; }
        public List<int> ToDeleteList { get; set; }
        public List<FileResourceDto> ToAddFileList { get; set; }
        public List<string> ToAddUrlList { get; set; }
    }
}
