export default function (task, tasks) {
    if (!task.isUpcoming && !task.isCompleted) {
        return {
            title: 'Cannot delete task',
            message: 'Task is running',
            canDelete: false
        }
    }

    if (task.parentTasks.length > 0 && tasks.length > 0) {
        const parents = tasks.filter(t => task.parentTasks.includes(t.id));

        if (parents.some(t => !t.isUpcoming && !t.isCompleted)) {
            return {
                title: 'Cannot delete task',
                message: 'One of its parents is running',
                canDelete: false
            }
        }
    }

    return { canDelete: true };
}
