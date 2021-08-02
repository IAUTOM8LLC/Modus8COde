import mapStatus from './mapStatus'

export default function updateTasksForState(allTasks, projectTasks) {
    const projectTasksMapped = projectTasks.map(mapStatus);

    projectTasksMapped.forEach((projectTask) => {
        const taskToUpdate = allTasks.find(t => t.id === projectTask.id);
        allTasks[allTasks.indexOf(taskToUpdate)] = projectTask;
    })

    return allTasks;
}
