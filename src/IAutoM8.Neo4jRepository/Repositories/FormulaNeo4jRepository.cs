using IAutoM8.Neo4jRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IAutoM8.Neo4jRepository.Repositories
{
    public class FormulaNeo4jRepository: IFormulaNeo4jRepository
    {
        private readonly IDataAccess _dataAccess;

        public FormulaNeo4jRepository(
            IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public ITransaction BeginTransaction() => _dataAccess.BeginTransaction();

        public async Task AddFormulaAsync(int formulaId)
        {
            await _dataAccess.RunAsync(@"
                Create(n:Formula{
                    uId:$formulaId})",
                new { formulaId });
        }

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
        public async Task AddRelationAsync(int id, int internalFormulaId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH (a:Formula), (b:Formula)
                        WHERE a.uId = $id AND b.uId = $internalFormulaId
                        CREATE (a)-[r:Relation]->(b)
                        RETURN r", new { id, internalFormulaId });
        }

        public async Task RemoveRelationAsync(int id, int internalFormulaId)
        {
            await _dataAccess.RunAsync(@"
                        MATCH (a:Formula)-[r:Relation]->(b:Formula)
                        WHERE a.uId = $id AND b.uId = $internalFormulaId
                        DELETE r", new { id, internalFormulaId });
        }

        public async Task<bool> HasLoopAsync(int id)
        {
            return await _dataAccess.AnyAsync(@"
                MATCH p=(n:Formula)-[*]->(m:Formula)
                WHERE n.uId = $id and m.uId = $id
                AND NONE (node IN NODES(p) WHERE SIZE(
                            FILTER(x IN NODES(p) WHERE node = x AND x.uId <> $id)
                    ) > 1
                )
                RETURN length(p)", new { id });
        }

        public async Task<IEnumerable<int>> GetAllInternalFormulaIds(int id)
        {
            var list = new List<int>();
            var cursor = await _dataAccess.RunAsync(@"MATCH(a:Formula)-[:Relation*]->(b:Formula)
                        WHERE a.uId = $id
                        RETURN distinct b.uId as uId", new { id });
            while (await cursor.FetchAsync())
            {
                list.Add((int)(long)cursor.Current["uId"]);
            }
            list.Add(id);
            return list;
        }
    }
}
