using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Neo4jRepository.Dto
{
    public class ResourceNeo4jDto
    {
        public string Name { get; set; }
        public string Mime { get; set; }
        public string Path { get; set; }
        public int Size { get; set; }
        public byte Type { get; set; }
        public bool IsShared { get; set; }
        public bool IsGlobalShared { get; set; }
        public bool IsPublished { get; set; }
        public int OriginType { get; set; }
        public string TimeStamp { get; set; }
    }
}
