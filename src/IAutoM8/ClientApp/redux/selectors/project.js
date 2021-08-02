import { createSelector } from 'reselect'

import { projectAccessor } from '@utils/sort'

import { ALL_PROJECTS_ID } from '@constants/projectCostants'

import { selectSearchQueryTrimLowerCase, selectSearchColumn } from './layout'

const getProjects = (state) => state.project.projects;

const getChildProjects = (state) => state.project.childProjects;

export const getUsers = (state) => state.users.users;

export const getProjectId = (state, props) => {
    if (props.match.params.projectId === 'getTasksFromAllProjects'
        || props.match.params.projectId === 'allProjects'
        || props.projectTaskNotification
        || props.formulaTaskNotification
        || props.transferRequest) {
        return ALL_PROJECTS_ID;
    }
    return Number(props.match.params.projectId) || 0;
}

export const selectProjectName = createSelector(
    [getProjectId, getProjects],
    (projectId, projects) => {
        if (projectId === ALL_PROJECTS_ID) {
            return 'All Projects';
        }
        const project = projects.find(p => p.id === projectId);
        return project ? project.name : '';
    }
);

export const filterProjectsByQuery = createSelector(
    [selectSearchQueryTrimLowerCase, selectSearchColumn, getProjects],
    (query, column, projects) => {
        if (query) {
            const accessor = projectAccessor(column);
            return projects.filter(p =>
                accessor
                && accessor(p)
                && accessor(p).includes(query)
            ) || [];
        }

        return projects;
    }
);

export const getProjectOptions = createSelector(
    [getProjects],
    (projects = []) => {
        return projects.map((p) => ({
            key: p.id,
            value: p.id,
            text: p.name
        }));
    }
);

export const getChildProjectOptions = createSelector(
    [getChildProjects],
    (childProjects = []) => {
        return childProjects.map((p) => ({
            key: p.id,
            value: p.id,
            text: p.name
        }));
    }
);
