export default function (task) {
    if (!task)
        return false;

    if (task.childTasks.length > 0) {
        return true;
    }

    if (task.condition && task.condition.options) {
        return task.condition.options.filter(o => o.assignedTaskId > 0).length > 0;
    }

    return false;
}