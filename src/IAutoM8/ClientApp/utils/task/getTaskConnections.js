export default function (tasks, grayOutIds) {
    const connections = [];

    for (const task of tasks) {
        if (task.isConditional && task.condition) {
            const opts = task.condition.options.filter(opt => opt.assignedTaskId > 0);
            opts.forEach(opt =>
                connections.push({
                    from: opt.id,
                    to: opt.assignedTaskId,
                    option: opt.option,
                    isConditional: true,
                    isSelected: opt.isSelected,
                    isGroup: task.hasOwnProperty('parentTaskId') && task.parentTaskId !== null,
                    isGrayed: grayOutIds &&
                        (grayOutIds.includes(Number(opt.assignedTaskId)) || grayOutIds.includes(task.id))
                }));
        }

        task.childTasks.forEach(childId => {
            connections.push({
                from: task.id.toString(),
                to: childId.toString(),
                isConditional: false,
                isGroup: task.hasOwnProperty('parentTaskId') && task.parentTaskId !== null,
                isGrayed: grayOutIds &&
                    (grayOutIds.includes(Number(childId)) || grayOutIds.includes(Number(task.id)))
            });
        });
    }

    return connections;
}
