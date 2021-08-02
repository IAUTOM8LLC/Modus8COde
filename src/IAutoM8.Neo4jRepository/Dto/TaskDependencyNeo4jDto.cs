namespace IAutoM8.Neo4jRepository.Dto
{
    public class TaskDependencyNeo4jDto
    {
        public int ParentTaskId { get; set; }
        public int ChildTaskId { get; set; }
    }
}
