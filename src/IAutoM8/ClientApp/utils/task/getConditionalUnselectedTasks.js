export default function (tasks) {
    if (!tasks.length || tasks.some(t => t.parentTasks === undefined))
        return [];

    const roots = tasks
        .filter(t => t.parentTasks.length === 0 && t.conditionalParentTasks.length === 0)
        .map(r => r.id);
    let result = [];
    return processTasks(roots, tasks, result);
}
const processTasks = (ids, tasks, result, grayOut = false) => {
    ids.forEach(id => {
        const task = tasks.find(t => t.id === id);
        if (!task)
            return [];
        if (grayOut) {
            const parentConds = tasks
                .filter(t => task.conditionalParentTasks.some(s => s === t.id));
            if (!task.isCompleted &&
                (!parentConds.some(e => e.isCompleted && e.condition.options
                .some(o => o.assignedTaskId === id && o.isSelected)) ||
                task.parentTasks.some(t => result.some(r => r === t)))) {
                result.push(id);
                if (task.isConditional) {
                    const conditionalUnselectedPath = task.condition.options
                        .filter(o => o.assignedTaskId && !o.isSelected)
                        .map(o => o.assignedTaskId);
                    processTasks(conditionalUnselectedPath, tasks, result, true);
                } else if (task.childTasks.length > 0) {
                    processTasks(task.childTasks, tasks, result, true);
                }
            }

        } else if (task.isConditional) {
            const selected = task.condition.options.filter(o => o.isSelected);

            if (selected.length === 1) {
                processTasks([selected[0].assignedTaskId], tasks, result);

                if (task.condition.options.filter(o => o.assignedTaskId && !o.isSelected)) {
                        processTasks(
                            task.condition.options
                                .filter(o => o.assignedTaskId && !o.isSelected)
                                .map(o => o.assignedTaskId),
                            tasks,
                            result,
                            true);
                }
            } else if (task.condition.options.filter(o => o.assignedTaskId)) {
                processTasks(
                    task.condition.options.filter(o => o.assignedTaskId).map(o => o.assignedTaskId),
                    tasks, result, task.isCompleted);
            }

        } else if (task.childTasks.length > 0) {
            processTasks(task.childTasks, tasks, result);
        }
    });

    return result;
}
