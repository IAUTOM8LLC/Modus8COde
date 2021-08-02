using System.Collections.Generic;

namespace IAutoM8.Neo4jRepository.Dto
{
    public class TaskResourceInfoNeo4jDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mime { get; set; }
        public string Path { get; set; }
        public int Size { get; set; }
        public byte Type { get; set; }
        public bool IsGlobalShared { get; set; }
        public IEnumerable<int> TaskIds { get; set; }
    }
}
