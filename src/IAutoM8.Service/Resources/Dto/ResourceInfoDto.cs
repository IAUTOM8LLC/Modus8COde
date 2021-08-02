using System.Collections.Generic;

namespace IAutoM8.Service.Resources.Dto
{
    public class ResourceInfoDto
    {
        public string Name { get; set; }
        public string Mime { get; set; }
        public string Url { get; set; }
        public int Size { get; set; }
        public byte Type { get; set; }
        public bool IsGlobalShared { get; set; }
        public IEnumerable<int> TaskIds { get; set; }
    }
}
