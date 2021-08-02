using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using Neo4j.Driver.V1;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace IAutoM8.Neo4jRepository.Repositories
{
    public class ResourceNeo4jRepository : IResourceNeo4jRepository
    {
        private readonly IDataAccess _dataAccess;

        public ResourceNeo4jRepository(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<IStatementResultCursor> CreateResourceAsync(ResourceNeo4jDto resourceDto)
        {
            return await _dataAccess.RunAsync(@"
                Create(n:Resource{
                    name:$Name,
                    path:$Path,
                    mime:$Mime,
                    size:$Size,
                    type:$Type,
                    isShared:$IsShared,
                    isGlobalShared:$IsGlobalShared,
                    isPublished:$IsPublished,
                    originType:$OriginType,
                    timeStamp:$TimeStamp
                }) return ID(n) as Id", resourceDto);
        }

        public async Task DeleteResourceAsync(long resourceId)
        {
            await _dataAccess.RunAsync(@"
                MATCH (r:Resource)
                WHERE ID(r) = $resourceId
                DETACH DELETE r", new { resourceId });
        }

        public async Task UpdateTaskResourceAsync(UpdateTaskResourceNeo4jDto resourceDto)
        {
            await _dataAccess.RunAsync(@"
                MATCH (r:Resource)
                WHERE ID(r) = $Id
                SET r.isShared = $IsShared, r.isGlobalShared = $IsGlobalShared", resourceDto);
        }

        public async Task PublishResourceAsync(PublishTaskResourceNeo4jDto resourceDto)
        {
            await _dataAccess.RunAsync(@"
                MATCH (r:Resource)
                WHERE ID(r) = $Id
                SET r.isPublished = $IsPublished", resourceDto);
        }

        public async Task<List<TaskResourceNeo4jDto>> MapTaskResourcesAsync(IStatementResultCursor resourcesCursor)
        {
            var list = new List<TaskResourceNeo4jDto>();
            while (await resourcesCursor.FetchAsync())
            {
                var resource = new TaskResourceNeo4jDto
                {
                    Id = (long)resourcesCursor.Current["Id"],
                    IsSharedFromParent = (bool)resourcesCursor.Current["isSharedFromParent"],
                    CameFromParent = (bool)resourcesCursor.Current["cameFromParent"],
                    IsShared = (bool)resourcesCursor.Current["IsShared"],
                    IsGlobalShared = (bool)resourcesCursor.Current["IsGlobalShared"],
                    IsPublished = (bool)resourcesCursor.Current["IsPublished"],
                    OriginType = (int)(long)resourcesCursor.Current["OriginType"],
                    TimeStamp = (string)resourcesCursor.Current["TimeStamp"],
                    Mime = (string)resourcesCursor.Current["Name"],
                    Path = (string)resourcesCursor.Current["Path"],
                    Name = (string)resourcesCursor.Current["Name"],
                    Size = (int)(long)resourcesCursor.Current["Size"],
                    Type = (byte)(long)resourcesCursor.Current["Type"]
                };
                if (resource.IsSharedFromParent)
                {
                    var mappedResource = list.FirstOrDefault(w => w.Id == resource.Id);
                    if (mappedResource == null)
                        list.Add(resource);
                    else
                        mappedResource.CameFromParent = true;
                }
                else
                {
                    list.Add(resource);
                }
            }
            return list;
        }

        public async Task<IEnumerable<TaskResourceInfoNeo4jDto>> MapTaskResourceInfosAsync(IStatementResultCursor resourcesCursor)
        {
            var list = new List<TaskResourceInfoNeo4jDto>();
            while (await resourcesCursor.FetchAsync())
            {
                var id = (int)(long)resourcesCursor.Current["Id"];
                var mappedResource = list.FirstOrDefault(w => w.Id == id);
                if (mappedResource == null)
                {
                    mappedResource = new TaskResourceInfoNeo4jDto
                    {
                        Id = id,
                        IsGlobalShared = (bool)resourcesCursor.Current["IsGlobalShared"],
                        Mime = (string)resourcesCursor.Current["Name"],
                        Path = (string)resourcesCursor.Current["Path"],
                        Name = (string)resourcesCursor.Current["Name"],
                        Size = (int)(long)resourcesCursor.Current["Size"],
                        Type = (byte)(long)resourcesCursor.Current["Type"],
                        TaskIds = new List<int> { (int)(long)resourcesCursor.Current["TaskId"] }
                    };
                    list.Add(mappedResource);
                }
                else
                {
                    mappedResource.TaskIds.Append((int)(long)resourcesCursor.Current["TaskId"]);
                    if (!mappedResource.IsGlobalShared && (bool)resourcesCursor.Current["IsGlobalShared"])
                        mappedResource.IsGlobalShared = true;
                }
            }
            return list;
        }
    }
}
