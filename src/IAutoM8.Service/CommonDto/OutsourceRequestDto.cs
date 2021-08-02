using System.Collections.Generic;

namespace IAutoM8.Service.CommonDto
{
    public class OutsourceRequestDto
    {
        public int TaskId { get; set; }
        public List<OutsourceRequestItemDto> Outsources { get; set; }
    }
}
