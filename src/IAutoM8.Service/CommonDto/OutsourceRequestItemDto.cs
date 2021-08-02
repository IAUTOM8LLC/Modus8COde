using System;

namespace IAutoM8.Service.CommonDto
{
    public class OutsourceRequestItemDto
    {
        public Guid Id { get; set; }
        public bool IsSelected { get; set; }
        public string Role { get; set; }
        public string OwnerId { get; set; }        
    }
}
