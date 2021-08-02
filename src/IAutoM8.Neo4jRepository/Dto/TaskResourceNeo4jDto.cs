namespace IAutoM8.Neo4jRepository.Dto
{
    public class TaskResourceNeo4jDto: ResourceNeo4jDto
    {
        public long Id { get; set; }
        public bool IsSharedFromParent { get; set; }
        public bool CameFromParent { get; set; } //Commented
    }
}
