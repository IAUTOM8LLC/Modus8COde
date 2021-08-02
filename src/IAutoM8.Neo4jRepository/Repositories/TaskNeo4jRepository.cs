using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITransaction = IAutoM8.Neo4jRepository.Interfaces.ITransaction;

namespace IAutoM8.Neo4jRepository.Repositories
{
    public class TaskNeo4jRepository : ITaskNeo4jRepository
    {
        private readonly IDataAccess _dataAccess;
        private readonly IResourceNeo4jRepository _resourceRepository;

        public TaskNeo4jRepository(
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
                    MATCH (a:Task), (b:Resource)
                    WHERE a.uId = $TaskId AND ID(b) = $ResourceId
                    CREATE (a) <-[r:TaskResource]-(b)",
                    new
                    {
                        TaskId = taskId,
                        ResourceId = (long)resourceResult.Current["Id"]
                    });
            }
        }

        public async Task AddResourceToFormulaTaskAsync(int taskId, TaskResourceNeo4jDto resourceDto)
        {
            var resourceResult = await _resourceRepository.CreateResourceAsync(resourceDto);
            if (await resourceResult.FetchAsync())
            {
                await _dataAccess.RunAsync(@"
                    MATCH (a:FormulaTask), (b:Resource)
                    WHERE a.uId = $TaskId AND ID(b) = $ResourceId
                    CREATE (a) <-[r:FormulaTaskResource]-(b)",
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
                    await _dataAccess.RunAsync(@"
                        Create(n:Task{
                            uId:$taskId,
                            projectId:$projectId,
                            status:$status})",
                            new { taskId = task.Id, projectId = task.ProjectId, status = task.Status });
                    foreach (var resource in task.Resoruces)
                    {
                        var resourceResult = await _resourceRepository.CreateResourceAsync(resource);
                        if (await resourceResult.FetchAsync())
                        {
                            await _dataAccess.RunAsync(@"
                                MATCH (a:Task), (b:Resource)
                                WHERE a.uId = $TaskId AND ID(b) = $ResourceId
                                CREATE (a)<-[r:TaskResource]-(b)
                                RETURN r", new
                            {
                                TaskId = task.Id,
                                ResourceId = (long)resourceResult.Current["Id"]
                            });
                        }
                    }
                }

                await _dataAccess.ExecuteAsync(@"
                    MATCH(a:Task), (b:Task)
                    WHERE a.uId = $ParentTaskId AND b.uId = $ChildTaskId
                    CREATE (a)-[r:Dependency]->(b)
                    RETURN r", dependencies);

                await _dataAccess.RunAsync(@"
                    MATCH(a:Task), (b:Task)
                    WHERE a.uId = $ConditionTaskId AND b.uId = $TaskId
                    CREATE (a)-[r:Condition{isSelected:false,uId: $Id}]->(b)
                    RETURN r", conditions);

                transaction.Commit();
            }
        }

        public async Task AddTaskAsync(int taskId, int projectId)
        {
            await _dataAccess.RunAsync(@"
                Create(n:Task{
                    uId:$taskId,
                    projectId:$projectId,
                    status:$status})",
                new { taskId, projectId, status = (byte)TaskStatusType.New });
        }

        public async Task AddFormulaTaskAsync(int taskId, int projectId, int formulaTaskId)
        {
            await _dataAccess.RunAsync(@"
                Create(n:Task{
                    uId:$taskId,
                    projectId:$projectId,
                    status:$status,
                    formulaTaskId:$formulaTaskId})",
                new
                {
                    taskId,
                    projectId,
                    status = (byte)TaskStatusType.New,
                    formulaTaskId
                });
        }

        public async Task ChangeTaskStatusAsync(int taskId, TaskStatusType type)
        {
            await _dataAccess.RunAsync(@"
                MATCH(n:Task{uId:$taskId})
                SET n.status=$status",
                new { taskId, status = (byte)type });
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            await _dataAccess.RunAsync(@"
                MATCH (t:Task)--(r:Resource)
                WHERE t.uId = $taskId
                DETACH DELETE r", new { taskId });
            await _dataAccess.RunAsync(@"
                MATCH (t:Task)
                WHERE t.uId = $taskId
                DETACH DELETE t", new { taskId });
        }

        public async Task DeleteAllProjectTasksAsync(int projectId)
        {
            await _dataAccess.RunAsync(@"
                MATCH (t:Task)--(r:Resource)
                WHERE t.projectId = $projectId
                DETACH DELETE r", new { projectId });
            await _dataAccess.RunAsync(@"
                MATCH (t:Task)
                WHERE t.projectId = $projectId
                DETACH DELETE t", new { projectId });
        }

        public async Task<IEnumerable<int>> GetAllTaskWithResourcesAsync(int projectId)
        {
            var resourcesCursor = await _dataAccess.RunAsync(@"
                MATCH (t:Task)--(r:Resource)
                WHERE t.projectId = $projectId
                RETURN DISTINCT t.uId as uId", new { projectId });
            var list = new List<int>();
            while (await resourcesCursor.FetchAsync())
            {
                list.Add((int)(long)resourcesCursor.Current["uId"]);
            }
            return list;
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
                    MATCH (t:Task)<-[*]-(:Task)--(r:Resource)
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
                    MATCH (t:Task)--(r:Resource)
                    WHERE t.projectId = $projectId and not t.uId = $taskId and r.isGlobalShared
                    return {baseResourceMap},                         
                        true as isSharedFromParent,
                        false as cameFromParent,
                        false as IsShared,
                        false as IsGlobalShared,
                        false as IsPublished,
                        0 as OriginType,
                        null as TimeStamp
                    UNION
                    MATCH (t:Task)--(r:Resource)
                    WHERE t.projectId = $projectId and r.isPublished
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
                        END as TimeStamp
                    UNION
                    MATCH (t:Task)--(r:Resource)
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

        public async Task<List<TaskResourceNeo4jDto>> GetTaskAndAllSharedResourcesAsync(int taskId, int projectId)
        {
            const string baseResourceMap = @"
                    ID(r) as Id,
                    r.mime as Mime,
                    r.name as Name,
                    r.path as Path,
                    r.size as Size,
                    r.type as Type";

            var result = await _dataAccess.RunAsync($@"
                    MATCH (t:Task)<-[*]-(:Task)--(r:Resource)
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
                    MATCH (t:Task)--(r:Resource)
                    WHERE t.projectId = $projectId and not t.uId = $taskId and r.isGlobalShared
                    return {baseResourceMap},                         
                        true as isSharedFromParent,
                        false as cameFromParent,
                        false as IsShared,
                        false as IsGlobalShared,
                        false as IsPublished,
                        0 as OriginType,
                        null as TimeStamp
                    UNION
                    MATCH (t:Task)--(r:Resource)
                    WHERE t.projectId = $projectId
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
            var result = await _dataAccess.RunAsync(@"
                MATCH (t:Task)--(r:Resource)
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
                        END as TimeStamp", new { taskId });

            return await _resourceRepository.MapTaskResourcesAsync(result);
        }

        public async Task<IEnumerable<TaskResourceInfoNeo4jDto>> GetProjectResourcesAsync(int projectId)
        {
            var result = await _dataAccess.RunAsync(@"
                MATCH (t:Task)--(r:Resource)
                WHERE t.projectId = $projectId
                return
                    ID(r) as Id, r.mime as Mime, r.name as Name, r.path as Path, r.size as Size, r.type as Type,
                    false as IsGlobalShared,
                    t.uId as TaskId", new { projectId });

            return await _resourceRepository.MapTaskResourceInfosAsync(result);
        }

        public async Task<IEnumerable<TaskResourceInfoNeo4jDto>> GetProjectResourcesAsync(int projectId, IEnumerable<int> taskIds)
        {
            const string baseResourceMap = @"
                    ID(r) as Id,
                    r.mime as Mime,
                    r.name as Name,
                    r.path as Path,
                    r.size as Size,
                    r.type as Type";
            var tasks = string.Join(",", taskIds);

            var result = await _dataAccess.RunAsync($@"
                    MATCH (t:Task)<-[*]-(:Task)--(r:Resource)
                    WHERE t.uId in [{tasks}] and r.isShared
                    return {baseResourceMap},
                        false as IsGlobalShared,
                        t.uId as TaskId
                    UNION
                    MATCH (t:Task)--(r:Resource)
                    WHERE t.projectId = $projectId and not t.uId in [{tasks}] and r.isGlobalShared
                    return {baseResourceMap},
                        true as IsGlobalShared,
                        t.uId as TaskId
                    UNION
                    MATCH (t:Task)--(r:Resource)
                    WHERE t.uId in [{tasks}]
                    return {baseResourceMap},
                        false as IsGlobalShared,
                        t.uId as TaskId",
                new { projectId });

            return await _resourceRepository.MapTaskResourceInfosAsync(result);
        }

        #region dependency and condition
        public async Task RemoveTaskDependencyAsync(int parentTaskId, int childTaskId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH (a:Task)-[r:Dependency]->(b:Task)
                        WHERE a.uId = $parentTaskId AND b.uId = $childTaskId
                        DELETE r", new { parentTaskId, childTaskId });
        }

        public async Task AddTaskDependencyAsync(int parentTaskId, int childTaskId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH(a:Task), (b:Task)
                        WHERE a.uId = $parentTaskId AND b.uId = $childTaskId
                        CREATE (a)-[r:Dependency]->(b)", new { parentTaskId, childTaskId });
        }

        public async Task RemoveTaskConditionAsync(int conditionTaskId, int taskId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH (a:Task)-[r:Condition]->(b:Task)
                        WHERE a.uId = $conditionTaskId AND b.uId = $taskId
                        DELETE r", new { conditionTaskId, taskId });
        }

        public async Task AddTaskConditionAsync(int conditionId, int conditionTaskId, int taskId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH (a:Task), (b:Task)
                        WHERE a.uId = $conditionTaskId AND b.uId = $taskId
                        CREATE (a)-[r:Condition{isSelected:false,uId: $conditionId}]->(b)",
                        new { conditionId, conditionTaskId, taskId });
        }

        public async Task SetTaskConditionSelectedAsync(int conditionId, bool isSelected)
        {
            await _dataAccess.RunAsync(@"
                        MATCH ()-[r:Condition{uId:$conditionId}]-()
                        SET r.isSelected=$isSelected",
                        new { conditionId, isSelected });
        }

        public async Task SetTaskConditionSelectedAsync(int taskCondId, int taskChildId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH (:Task{uId:$taskCondId})-[r]-(:Task{uId:$taskChildId})
                        SET r.isSelected=true",
                        new { taskCondId, taskChildId });
        }

        public async Task DeselectTaskConditionsAsync(int taskId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH(n:Task{ uId: $taskId})-[r:Condition]->()
                        SET r.isSelected=false",
                        new { taskId });
        }
        #endregion

        public async Task<bool> HasLoopAsync(int taskId)
        {
            return await _dataAccess.AnyAsync(@"
                MATCH p=(n:Task)-[*]-(m:Task)
                WHERE n.uId = $taskId and m.uId = $taskId
                AND NONE (node IN NODES(p) WHERE SIZE(
                            FILTER(x IN NODES(p) WHERE node = x AND x.uId <> $taskId)
                    ) > 1
                )
                RETURN length(p)", new { taskId });
        }

        public async Task<bool> IsLeafAsync(int taskId)
        {
            return !await _dataAccess.AnyAsync(@"
                MATCH (n:Task{uId:$taskId})-[*]->(leaf:Task)
                WHERE NOT (leaf)-[]->(:Task)
                RETURN n", new { taskId });
        }

        public async Task<bool> IsRootAsync(int taskId)
        {
            return !await _dataAccess.AnyAsync(@"
                MATCH (n:Task{uId:$taskId})<-[*]-(root:Task)
                WHERE NOT (root)<-[]-(:Task)
                RETURN n", new { taskId });
        }

        public async Task<IEnumerable<int>> GetChildTaskIdsAsync(int taskId)
        {
            var result = await _dataAccess.RunAsync(@"
                MATCH (n:Task{uId:$taskId})-[]->(child:Task)
                RETURN child.uId as uId", new { taskId });

            return await result.ToListAsync(record => (int)(long)record["uId"]);
        }

        public async Task<IEnumerable<int>> GetParentTaskIdsAsync(int taskId)
        {
            var result = await _dataAccess.RunAsync(@"
                MATCH (n:Task{uId:$taskId})<-[]-(parent:Task)
                RETURN parent.uId as uId", new { taskId });

            return await result.ToListAsync(record => (int)(long)record["uId"]);
        }

        public async Task<IEnumerable<int>> GetReadyToStartTaskIdsAsync(int taskId)
        {
            var statusNew = (byte)TaskStatusType.New;
            var statusComplete = (byte)TaskStatusType.Completed;
            var result = await _dataAccess.RunAsync(@"
                MATCH (:Task{uId:$taskId})-->(child:Task)
                WHERE child.status=$statusNew
                MATCH p=(child)<--(parent:Task)
                WITH child.uId as uId,
                    collect(distinct filter(rel in relationships(p) where endNode(rel).uId=child.uId and type(rel)=""Dependency"")) as noConditionalConns,
                    collect(distinct filter(rel in relationships(p) where endNode(rel).uId=child.uId and type(rel)=""Condition"")) as conditionalConns,
                    collect(EXTRACT(rel in relationships(p) | [startNode(rel), rel, endNode(rel)])) as detailedPath
                WITH uId,
	                collect(size(filter(nodeAndRel in filter(nodeAndRel in detailedPath where any(rel in nodeAndRel where (rel[2]).uId = uId and 
    	                type(rel[1])=""Condition"")) where all(gr in nodeAndRel where (gr[0]).status={statusComplete} and ((type(gr[1])=""Condition"" and (gr[1]).isSelected)
        	                or type(gr[1])=""Dependency""))))) as countOfCompletedCondPaths,
                    collect(size(filter(nodeAndRel in filter(nodeAndRel in detailedPath where any(rel in nodeAndRel where (rel[2]).uId = uId and 
    	                type(rel[1])=""Dependency"")) where all(gr in nodeAndRel where (gr[0]).status={statusComplete} and ((type(gr[1])=""Condition"" and (gr[1]).isSelected)
                            or type(gr[1])=""Dependency""))))) as countOfCompletedNoCondPaths,
                    collect(size(filter(list in noConditionalConns where size(list)>0))) as noConditionalConnsCount,
                    collect(size(filter(list in conditionalConns where size(list)>0))) as conditionalConnsCount
                WHERE countOfCompletedNoCondPaths[0]=noConditionalConnsCount[0] and (conditionalConnsCount[0]=0 or countOfCompletedCondPaths[0]>0)
                RETURN uId", new { taskId, statusNew, statusComplete });

            return await result.ToListAsync(record => (int)(long)record["uId"]);
        }

        public async Task<IEnumerable<int>> GetRootTaskIdsAsync(int taskId)
        {
            var result = await _dataAccess.RunAsync(@"
                OPTIONAL MATCH (selfParrent:Task{uId:$taskId})
                WHERE NOT (selfParrent)<-[]-(:Task)
                OPTIONAL MATCH (current:Task{uId:$taskId})<-[*]-(parrent:Task)
                WHERE NOT (parrent)<-[]-(:Task)
                WITH EXTRACT(node in collect(selfParrent)|node.uId)+EXTRACT(node in collect(parrent)|node.uId) as nodeIds
                UNWIND nodeIds as uId
                Return uId", new { taskId });

            return await result.ToListAsync(record => (int)(long)record["uId"]);
        }

        public async Task<IEnumerable<int>> GetProjectRootTaskIdsAsync(int projectId)
        {
            var result = await _dataAccess.RunAsync(@"
                MATCH p=(task:Task{projectId:$projectId})
                WHERE NOT (task)<-[]-(:Task) AND NOT EXISTS(task.formulaTaskId)
                Return task.uId as uId", new { projectId });

            return await result.ToListAsync(record => (int)(long)record["uId"]);
        }

        public async Task<IEnumerable<int>> GetFormulaRootTaskIdsAsync(int formulaTaskId)
        {
            var result = await _dataAccess.RunAsync(@"
                MATCH (root:Task{formulaTaskId:$formulaTaskId})
                WHERE NOT (root)<-[]-(:Task)
                RETURN root.uId as uId", new { formulaTaskId });

            return await result.ToListAsync(record => (int)(long)record["uId"]);
        }
        public async Task<IEnumerable<int>> GetFormulaRootAllTaskIdsAsync(int formulaTaskId)
        {
            var result = await _dataAccess.RunAsync(@"
                MATCH (root:Task{formulaTaskId:$formulaTaskId})
                RETURN root.uId as uId", new { formulaTaskId });

            return await result.ToListAsync(record => (int)(long)record["uId"]);
        }

        public async Task<bool> IsGraphCompleted(int taskId)
        {
            var statusComplete = (byte)TaskStatusType.Completed;

            return !await _dataAccess.AnyAsync($@"
                OPTIONAL MATCH (selfParrent:Task)
                WHERE selfParrent.uId = $taskId and not (selfParrent:Task)<-[]-(:Task)
                OPTIONAL MATCH (parent:Task)-[*]-(current:Task)
                WHERE current.uId = $taskId and not (parent:Task)<-[]-(:Task)
                WITH  EXTRACT(node in collect(selfParrent) | node.uId) + EXTRACT(node in collect(parent) | node.uId) as nodeIds
                UNWIND nodeIds as nodeId
                WITH collect(distinct nodeId) as parrentNodeIds
                MATCH p=(parrentTask:Task)-[*]->(task:Task)
                WHERE parrentTask.uId in parrentNodeIds and task.status<>{statusComplete}
                WITH task.uId as uId,
                    collect(distinct filter(rel in relationships(p) where endNode(rel).uId=task.uId and type(rel)=""Dependency"")) as noConditionalConns,
                    collect(distinct filter(rel in relationships(p) where endNode(rel).uId=task.uId and type(rel)=""Condition"")) as conditionalConns,
                    collect(EXTRACT(rel in relationships(p) | [startNode(rel), rel, endNode(rel)])) as detailedPath
                WITH uId,
                    collect(filter(nodeAndRel in filter(nodeAndRel in detailedPath where any(rel in nodeAndRel where (rel[2]).uId = uId and 
                        type(rel[1])=""Condition"")) where all(gr in nodeAndRel where (gr[0]).status={statusComplete} and ((type(gr[1])=""Condition"" and (gr[1]).isSelected)
                            or type(gr[1])=""Dependency"")))) as completedCondPaths,
                    collect(filter(nodeAndRel in filter(nodeAndRel in detailedPath where any(rel in nodeAndRel where (rel[2]).uId = uId and 
                        type(rel[1])=""Dependency"")) where all(gr in nodeAndRel where (gr[0]).status={statusComplete} and ((type(gr[1])=""Condition"" and (gr[1]).isSelected)
                            or type(gr[1])=""Dependency"")))) as completedNoCondPaths,
                    collect(size(filter(list in noConditionalConns where size(list)>0))) as noConditionalConnsCount,
                    collect(size(filter(list in conditionalConns where size(list)>0))) as conditionalConnsCount
                WITH uId,
                    EXTRACT(e in completedNoCondPaths | EXTRACT(e2 in e | EXTRACT(e3 in filter(f in e2 where (f[2]).uId = uId) |(e3[0]).uId)[0])) as completedNoCondParrentIds, 
                    EXTRACT(e in completedCondPaths | EXTRACT(e2 in e | EXTRACT(e3 in filter(f in e2 where (f[2]).uId = uId) |(e3[0]).uId)[0])) as completedCondParrentIds,
                    noConditionalConnsCount, conditionalConnsCount
                    UNWIND CASE
                        WHEN completedNoCondParrentIds[0] = []
                            THEN [null]
                            ELSE completedNoCondParrentIds[0]
                        END as completedNoCondParrentId
                    UNWIND CASE
                        WHEN completedCondParrentIds[0] = []
                            THEN [null]
                            ELSE completedCondParrentIds
                        END as completedCondParrentId
                WITH uId, size(COLLECT(DISTINCT completedNoCondParrentId)) as countOfCompletedNoCondPaths,
                    size(COLLECT(DISTINCT completedCondParrentId)) as countOfCompletedCondPaths,
                    noConditionalConnsCount, conditionalConnsCount
                WHERE countOfCompletedNoCondPaths=noConditionalConnsCount[0] and (conditionalConnsCount[0]=0 or countOfCompletedCondPaths>0)
                Return uId
                union
                MATCH(parent:Task)
                WHERE parent.uId=$taskId and not(parent)<--(:Task) and parent.status <> {statusComplete}
                Return parent.uId as uId
                union
                MATCH (parent:Task)-[*]-(current:Task)
                WHERE current.uId = $taskId and not (parent)<--(:Task) and parent.status <> {statusComplete}
                Return parent.uId as uId",
                new { taskId }
            );
        }

        public async Task<bool> IsFormulaRootCompletedAsync(int forumlaTaskId)
        {
            var statusComplete = (byte)TaskStatusType.Completed;
            return !await _dataAccess.AnyAsync($@"
                MATCH (leaf:Task)
                WHERE leaf.formulaTaskId = $forumlaTaskId and leaf.status <> {statusComplete} and NOT (leaf)<-[]-(:Task)
                Return leaf.uId as uId", new { forumlaTaskId });
        }

        public async Task<bool> IsFormulaGraphCompletedAsync(int forumlaTaskId)
        {
            var rootIds = await GetFormulaRootTaskIdsAsync(forumlaTaskId);
            foreach (var id in rootIds)
            {
                if (!await IsGraphCompleted(id))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> HasRelationsAsync(int taskId1, int taskId2)
        {
            return await _dataAccess.AnyAsync(@"
                MATCH p =(:Task{uId:$taskId1})-[*]-(:Task{uId:$taskId2})
                WITH relationships(p) AS r
                RETURN r", new { taskId1, taskId2 });
        }

        public async Task<bool> IsAvailableToStartAsync(int taskId)
        {
            var statusComplete = (byte)TaskStatusType.Completed;
            return await _dataAccess.AnyAsync($@"
                MATCH p=(t:Task)<-[]-(parent:Task)
                WHERE t.uId = $taskId and (t)<-[]-(:Task)
                MATCH (t)
                WITH t.uId as uId, collect(ALL(relation in relationships(p) where type(relation)=""Dependency"" or relation.isSelected)) as relations,
                    collect(ALL(node in nodes(p) where t.uId = $taskId or node.status={statusComplete})) as nodes
                where ALL(rel in relations where rel) and ALL(node in nodes where node)
                Return uId", new { taskId });
        }

        public async Task PublishTaskResourceAsync(PublishTaskResourceNeo4jDto resourceDto)
        {
            await _resourceRepository.PublishResourceAsync(resourceDto);
        }
    }
}
