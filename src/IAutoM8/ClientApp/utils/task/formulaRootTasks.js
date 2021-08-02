export default function (tasks, ids = []) {
    if (!tasks || tasks.length < 1)
        return [];

    const assignedOptions = tasks.map(t => t.condition
        ? t.condition.options
            .filter(co => co.assignedTaskId > 0)
            .map(x => x.assignedTaskId)
        : []);

    const merged = [].concat.apply([], assignedOptions);
    let assignedTasks = Array.from(new Set(merged));

    if (ids.length > 0)
        assignedTasks = assignedTasks.filter(t => ids.indexOf(t) !== -1);

    return tasks.filter(t => {
        const isRoot = ids.length > 0
            ? t.parentTasks.filter(pt => ids.includes(pt)).length === 0
            : t.parentTasks.length === 0;

        return isRoot && assignedTasks.indexOf(t.id) === -1;
    });
}
