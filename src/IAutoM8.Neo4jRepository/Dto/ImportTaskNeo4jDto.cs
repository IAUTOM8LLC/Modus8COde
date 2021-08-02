using System.Collections.Generic;

namespace IAutoM8.Neo4jRepository.Dto
{
    public class ImportTaskNeo4jDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public List<ResourceNeo4jDto> Resoruces { get; set; }
        public byte Status { get; set; }
    }
}
