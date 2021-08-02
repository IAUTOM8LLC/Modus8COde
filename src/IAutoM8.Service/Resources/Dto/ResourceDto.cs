using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Service.Resources.Dto
{
    public class ResourceDto
    {
        public int Id { get; set; }
        public bool IsShared { get; set; }
        public bool IsGlobalShared { get; set; }
        public bool IsPublished { get; set; }
        public bool IsSharedFromParent { get; set; }
        public bool CameFromParent { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Mime { get; set; }
        public int Size { get; set; }
        public ResourceType Type { get; set; }
        public ResourceOriginType OriginType { get; set; }
        public string TimeStamp { get; set; }
    }
}
