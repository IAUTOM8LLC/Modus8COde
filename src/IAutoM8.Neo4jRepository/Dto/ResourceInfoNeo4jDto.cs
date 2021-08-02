using System.Collections.Generic;

namespace IAutoM8.Neo4jRepository.Dto
{
    public class ResourceInfoNeo4jDto
    {
        public string Name { get; set; }
        public string Mime { get; set; }
        public string Path { get; set; }
        public int Size { get; set; }
        public byte Type { get; set; }
        public IEnumerable<int> TaskIds { get; set; }
    }
}
