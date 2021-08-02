using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Neo4jRepository.Repositories
{
    public class FormulaTaskNeo4jRepository : IFormulaTaskNeo4jRepository
    {
        private readonly IResourceNeo4jRepository _resourceRepository;
        private readonly IDataAccess _dataAccess;

        public FormulaTaskNeo4jRepository(
            IResourceNeo4jRepository resourceRepository,
            IDataAccess dataAccess)
        {
            _resourceRepository = resourceRepository;
            _dataAccess = dataAccess;
        }

        public ITransaction BeginTransaction() => _dataAccess.BeginTransaction();

        public async Task AddResourceToTaskAsync(int taskId, TaskResourceNeo4jDto resourceDto)
        {
            var resourceResult = await _resourceRepository.CreateResourceAsync(resourceDto);
            if (await resourceResult.FetchAsync())
            {
                await _dataAccess.RunAsync(@"
                    MATCH (a:FormulaTask), (b:Resource)
                    WHERE a.uId = $TaskId AND ID(b) = $ResourceId
                    CREATE (a) <-[r:FormulaTaskResource]-(b)
                    RETURN r",
                    new
                    {
                        TaskId = taskId,
                        ResourceId = (long)resourceResult.Current["Id"]
                    });
            }
        }

        public async Task AddTasksWithResourcesAsync(
            IEnumerable<ImportTaskNeo4jDto> tasks,
            IEnumerable<TaskDependencyNeo4jDto> dependencies,
            IEnumerable<TaskConditionNeo4jDto> conditions)
        {
            using (var transaction = _dataAccess.BeginTransaction())
            {
                foreach (var task in tasks)
                {
                    await AddTaskAsync(task.Id, task.ProjectId);
                    foreach (var resource in task.Resoruces)
                    {
                        var resourceResult = await _resourceRepository.CreateResourceAsync(resource);
                        if (await resourceResult.FetchAsync())
                        {
                            await _dataAccess.RunAsync(@"
                                MATCH (a:FormulaTask), (b:Resource)
                                WHERE a.uId = $TaskId AND ID(b) = $ResourceId
                                CREATE (a)<-[r:FormulaTaskResource]-(b)
                                RETURN r", new { TaskId = task.Id, ResourceId = (long)resourceResult.Current["Id"] });
                        }
                    }
                }

                await _dataAccess.ExecuteAsync(@"
                        MATCH(a:FormulaTask), (b:FormulaTask)
                        WHERE a.uId = $ParentTaskId AND b.uId = $ChildTaskId
                        CREATE (a)-[r:FormulaDependency]->(b)
                        RETURN r", dependencies);

                await _dataAccess.ExecuteAsync(@"
                        MATCH(a:FormulaTask), (b:FormulaTask)
                        WHERE a.uId = $ConditionTaskId AND b.uId = $TaskId
                        CREATE (a)-[r:FormulaCondition{isSelected:false,uId: $Id}]->(b)
                        RETURN r", conditions);

                transaction.Commit();
            }
        }

        public async Task AddTaskAsync(int taskId, int formulaId)
        {
            await _dataAccess.RunAsync(@"
                Create(n:FormulaTask{
                    uId:$taskId,
                    formulaId:$formulaId})",
                new { taskId, formulaId });
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            await _dataAccess.RunAsync(@"
                MATCH (t:FormulaTask)--(r:Resource)
                WHERE t.uId = $taskId
                DETACH DELETE r", new { taskId });
            await _dataAccess.RunAsync(@"
                MATCH (t:FormulaTask)
                WHERE t.uId = $taskId
                DETACH DELETE t", new { taskId });
        }

        public async Task<List<TaskResourceNeo4jDto>> GetTaskAndSharedResourcesAsync(int taskId, int projectId)
        {
            const string baseResourceMap = @"
                ID(r) as Id,
                r.mime as Mime,
                r.name as Name,
                r.path as Path,
                r.size as Size,
                r.type as Type";

            var result = await _dataAccess.RunAsync($@"
                    MATCH (t:FormulaTask)<-[*]-(:FormulaTask)--(r:Resource)
                    WHERE t.uId = $taskId and r.isShared
                    return {baseResourceMap}, 
                        true as isSharedFromParent,
                        true as cameFromParent,
                        false as IsShared,
                        false as IsGlobalShared,
                        false as IsPublished,
                        0 as OriginType,
                        null as TimeStamp
                    UNION
                    MATCH (t:FormulaTask)--(r:Resource)
                    WHERE t.formulaId = $projectId and not t.uId = $taskId and r.isGlobalShared
                    return {baseResourceMap},                         
                        true as isSharedFromParent,
                        false as cameFromParent,
                        false as IsShared,
                        false as IsGlobalShared,
                        false as IsPublished,
                        0 as OriginType,
                        null as TimeStamp
                    UNION
                    MATCH (t:FormulaTask)--(r:Resource)
                    WHERE t.uId = $taskId
                    return {baseResourceMap}, 
                        false as isSharedFromParent,
                        false as cameFromParent,
                        r.isShared as IsShared,
                        r.isGlobalShared as IsGlobalShared,
                        CASE r.isPublished
                            WHEN null THEN false
                            ELSE r.isPublished
                        END as IsPublished,
                        CASE r.originType
                            WHEN null THEN 0
                            ELSE r.originType
                        END as OriginType,
                        CASE r.timeStamp
                            WHEN null THEN null
                            ELSE r.timeStamp
                        END as TimeStamp",
                    new { taskId, projectId });

            return await _resourceRepository.MapTaskResourcesAsync(result);
        }

        public async Task<List<TaskResourceNeo4jDto>> GetTaskResourcesAsync(int taskId)
        {
            try
            {


                var result = await _dataAccess.RunAsync(@"
                    MATCH (t:FormulaTask)--(r:Resource)
                    WHERE t.uId = $taskId
                    return
                        ID(r) as Id, r.mime as Mime, r.name as Name, r.path as Path, r.size as Size, r.type as Type,
                        false as isSharedFromParent,
                        false as cameFromParent,
                        r.isShared as IsShared,
                        r.isGlobalShared as IsGlobalShared,
                        CASE r.isPublished
                            WHEN null THEN false
                            ELSE r.isPublished
                        END as IsPublished,
                        CASE r.originType
                            WHEN null THEN 0
                            ELSE r.originType
                        END as OriginType,
                        CASE r.timeStamp
                            WHEN null THEN null
                            ELSE r.timeStamp
                        END as TimeStamp",
                    new { taskId });

                return await _resourceRepository.MapTaskResourcesAsync(result);
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        #region dependency and condition
        public async Task RemoveTaskDependencyAsync(int parentTaskId, int childTaskId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH (a:FormulaTask)-[r:FormulaDependency]->(b:FormulaTask)
                        WHERE a.uId = $parentTaskId AND b.uId = $childTaskId
                        DELETE r", new { parentTaskId, childTaskId });
        }

        public async Task AddTaskDependencyAsync(int parentTaskId, int childTaskId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH(a:FormulaTask), (b:FormulaTask)
                        WHERE a.uId = $parentTaskId AND b.uId = $childTaskId
                        CREATE (a)-[r:FormulaDependency]->(b)
                        RETURN r", new { parentTaskId, childTaskId });
        }

        public async Task RemoveTaskConditionAsync(int conditionTaskId, int taskId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH (a:FormulaTask)-[r:FormulaCondition]->(b:FormulaTask)
                        WHERE a.uId = $conditionTaskId AND b.uId = $taskId
                        DELETE r", new { conditionTaskId, taskId });
        }

        public async Task AddTaskConditionAsync(int conditionId, int conditionTaskId, int taskId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH (a:FormulaTask), (b:FormulaTask)
                        WHERE a.uId = $conditionTaskId AND b.uId = $taskId
                        CREATE (a)-[r:FormulaCondition{isSelected:false,uId: $conditionId}]->(b)
                        RETURN r", new { conditionTaskId, taskId, conditionId });
        }
        #endregion

        public async Task<bool> HasLoopAsync(int taskId)
        {
            return await _dataAccess.AnyAsync(@"
                MATCH p=(n:FormulaTask)-[*]-(m:FormulaTask)
                WHERE n.uId = $taskId and m.uId = $taskId
                AND NONE (node IN NODES(p) WHERE SIZE(
                            FILTER(x IN NODES(p) WHERE node = x AND x.uId <> $taskId)
                    ) > 1
                )
                RETURN length(p)", new { taskId });
        }
    }
}
