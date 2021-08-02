using IAutoM8.Neo4jRepository.Dto;
using Neo4j.Driver.V1;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Neo4jRepository.Interfaces
{
    public interface IResourceNeo4jRepository
    {
        Task<IStatementResultCursor> CreateResourceAsync(ResourceNeo4jDto resourceDto);
        Task DeleteResourceAsync(long resourceId);
        Task UpdateTaskResourceAsync(UpdateTaskResourceNeo4jDto resourceDto);
        Task PublishResourceAsync(PublishTaskResourceNeo4jDto resourceDto);
        Task<List<TaskResourceNeo4jDto>> MapTaskResourcesAsync(IStatementResultCursor resourcesCursor);
        Task<IEnumerable<TaskResourceInfoNeo4jDto>> MapTaskResourceInfosAsync(IStatementResultCursor resourcesCursor);
    }
}
