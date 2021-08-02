namespace IAutoM8.Neo4jRepository.Dto
{
    public class UpdateTaskResourceNeo4jDto
    {
        public long Id { get; set; }
        public bool IsGlobalShared { get; set; }
        public bool IsShared { get; set; }
    }
}
