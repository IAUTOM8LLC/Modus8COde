import scc from 'strongly-connected-components'

export default function (tasks, edge) {
    const vertexes = tasks.map(t => t.id);

    const taskDirectConnections = tasks
        .filter(t => t.childTasks.length > 0)
        .map(t => t.childTasks.map(ct => [t.id, ct]));

    const directConnections = [].concat.apply([], taskDirectConnections);

    const taskConditionalConnections = tasks
        .filter(t => t.isConditional)
        .map(t => t.condition.options
            .filter(co => co.assignedTaskId)
            .map(co => [t.id, co.assignedTaskId]));

    const conditionalConnections = [].concat.apply([], taskConditionalConnections);

    const edges = [edge, ...directConnections, ...conditionalConnections];

    const result = scc(getAdjacencyList(vertexes, edges));
    return result.components.length !== tasks.length;
}

function getAdjacencyList(vertexes, edges) {
    const vertexOrder = vertexes.sort((a, b) => a - b);
    return vertexOrder.map(v => edges
        .filter(e => e[0] === v)
        .map(e => vertexOrder.indexOf(e[1])));
}
