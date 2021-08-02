using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Service.Resources.Dto
{
    public class FileResourceDto
    {
        public bool IsShared { get; set; }
        public bool IsGlobalShared { get; set; }
        public string Name { get; set; }
        public string Mime { get; set; }
        public int Size { get; set; }
        public int OriginType { get; set; }
        public string TimeStamp { get; set; }
    }
}
