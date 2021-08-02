import { createSelector } from 'reselect'
import { skillAccessor } from '@utils/sort'

import { selectSearchQueryTrimLowerCase, selectSearchColumn } from './layout'

const getProjectTasks = (state) => state.projectTasks.tasks;

export const getFileResources = (state) => state.resource.fileResources;
export const getVideoResources = (state) => state.resource.videoResources;

export const selectFileResources = createSelector(
    [getProjectTasks, getFileResources],
    (tasks, resorces) => selectResources(tasks, resorces)
);

export const selectVideoResources = createSelector(
    [getProjectTasks, getVideoResources],
    (tasks, resorces) => selectResources(tasks, resorces)
);

const selectResources = (tasks, resorces) => {
    if (resorces.length > 0 && resorces[0].taskIds !== undefined) {
        const allTaskNames = tasks.map(m => m.title);
        return resorces.map(res => {
            if (res.isGlobalShared) {
                return {
                    ...res,
                    taskNames: [ ...allTaskNames ]
                }
            }
            else {
                return {
                    ...res,
                    taskNames: res.taskIds.map(id => {
                        const task = tasks.find(t => t.id === id);
                        if (task)
                            return task.title;
                        else
                            return '';
                    })
                }
            }
        });
    }
    return [...resorces]
}

