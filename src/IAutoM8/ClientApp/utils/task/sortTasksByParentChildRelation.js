import sortBy from 'lodash/sortBy'
import moment from 'moment'

export default function (tasks = []) {
    const rootParentTasks = tasks.filter(t => !t.parentTasks.length&& t.childTasks.length);
    const rootConditionTasks = tasks.filter(t => t.condition != null
        && t.condition.options.some(x => x.assignedTaskId !== 0)
        && !t.conditionalParentTasks.length
        && !t.parentTasks.length);

    const tasksWithoutParentsAndChilds = tasks.filter(t => !t.parentTasks.length
        && !t.childTasks.length
        && (t.condition === null || t.condition.options.every(x => x.assignedTaskId === 0)));
    
    const isParentsInTheArray = (parentIds) => {
        const parentTasks = tasks.filter(t => parentIds.includes(t.id));
        return  parentTasks.length;
    }
    const pseudoRootTasks = tasks.filter(t =>
        (t.parentTasks.length > 0 || t.conditionalParentTasks.length > 0)
        && !isParentsInTheArray(t.parentTasks.concat(t.conditionalParentTasks)));

    let rootTasks = rootParentTasks.concat(rootConditionTasks
        .filter(f => !rootParentTasks.some(s => s.id === f.id)));
    rootTasks = rootTasks.concat(pseudoRootTasks
        .filter(f => !rootTasks.some(s => s.id === f.id)));
    rootTasks = rootTasks.concat(tasksWithoutParentsAndChilds
        .filter(f => !rootTasks.some(s => s.id === f.id)));
    const sortedRootTasksImmutable = sortBy(rootTasks, t => moment(t.dueDate).valueOf());
    let sortedRootTasks = sortBy(rootTasks, t => moment(t.dueDate).valueOf());

    if (sortedRootTasks.length === 0) {
        return sortBy(tasks, t => moment(t.dueDate).valueOf());
    } else {
        const findChildsSortAndInsertThemAfterParent = (sortedRootTasksParam) => {
            sortedRootTasksParam.forEach(function (element) {
                if (element.childTasks.length) {
                    const childTasks = tasks.filter(t => element.childTasks.includes(t.id));
                    let sortedChildTasks = sortBy(childTasks, t => moment(t.dueDate).valueOf());
                    sortedChildTasks = sortedChildTasks.filter(t => !sortedRootTasks.includes(t));
                    sortedRootTasks.splice(sortedRootTasks.indexOf(element) + 1,
                        0, ...sortedChildTasks);
                    findChildsSortAndInsertThemAfterParent(sortedChildTasks);
                }
                else if (element.condition != null) {
                    const conditionTasksIds = element.condition.options.map(t => t.assignedTaskId);
                    const conditionTasks = tasks.filter(t => conditionTasksIds.includes(t.id));
                    let sortedConditionTasks = sortBy(conditionTasks, t => moment(t.dueDate).valueOf());
                    sortedConditionTasks = sortedConditionTasks.filter(t => !sortedRootTasks.includes(t));
                    sortedRootTasks.splice(sortedRootTasks.indexOf(element) + 1,
                        0, ...sortedConditionTasks);
                    findChildsSortAndInsertThemAfterParent(sortedConditionTasks);
                }
            });
        }

        findChildsSortAndInsertThemAfterParent(sortedRootTasksImmutable);
        return sortedRootTasks;
    }

}
