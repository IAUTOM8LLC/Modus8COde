import axios from 'axios'
import typeToReducer from 'type-to-reducer'

import { getAuthHeaders } from '@infrastructure/auth'

// Types
const LOAD_PROJECTS = 'react/project/LOAD_PROJECTS';
const LOAD_CHILD_PROJECTS = 'react/project/LOAD_CHILD_PROJECTS';
const LOAD_MOST_RECENT_PROJECT = 'react/project/LOAD_MOST_RECENT_PROJECT';
const ADD_PROJECT = 'react/project/ADD_PROJECT';
const UPLOAD_BULK_PROJECTS = 'react/project/UPLOAD_BULK_PROJECTS';
const EDIT_PROJECT = 'react/project/EDIT_PROJECT';
const DELETE_PROJECT = 'react/project/DELETE_PROJECT';
const IMPORT_FORMULA_TASKS = 'react/project/IMPORT_FORMULA_TASKS';
const ADD_CHILD_NODE = 'react/project/ADD_CHILD_NODE';
const UPDATE_CHILD_NODE = 'react/project/UPDATE_CHILD_NODE';

const initialState = {
    loading: false,
    projects: [],
    childProjects: [],
    projectId: 0
};

// Reducer
const reducer = {
    [LOAD_PROJECTS]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            projects: action.payload.data
        })
    },

    [LOAD_CHILD_PROJECTS]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            childProjects: action.payload.data
        })
    },

    [LOAD_MOST_RECENT_PROJECT]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            projectId: action.payload.data
        })
    },

    [ADD_PROJECT]: {
        FULFILLED: (state, action) => {
        return ({
            ...state,
            projects: [...state.projects, action.payload.data]
        })
    }
    },
    [UPLOAD_BULK_PROJECTS]: {
        FULFILLED: (state, action) => {
            console.log('actionreducer', action.payload);
            return {
                ...state,
            }
        }
    },

    [EDIT_PROJECT]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data
        }),
        FULFILLED: (state, action) => {
            const projects = [...state.projects];
            const index = projects.findIndex(p => p.id === action.payload.data.id);
            projects[index] = action.payload.data;

            return ({ ...state, projects });
        }
    },

    [DELETE_PROJECT]: {
        PENDING: (state, action) => ({
            ...state,
            projects: state.projects.filter(p => p.id !== action.payload)
        })
    },

    [IMPORT_FORMULA_TASKS]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state) => ({ ...state, loading: false })
    },

    [ADD_CHILD_NODE]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state, action) => {
            return {...state, 
                loading: false, 
                projects: [...state.projects, action.payload.data]};
        }
    },

    [UPDATE_CHILD_NODE]: {
        FULFILLED: (state, action) => {
            return {
                ...state,
                projects: state.projects.map(project => {
                    if (project.id === action.payload.data.id) {
                        return action.payload.data;
                    } else {
                        return project;
                    }
                })
            }
        }
    }
};

// Actions
const baseUrl = '/api/projects';

export function loadProjects() {
    return {
        type: LOAD_PROJECTS,
        payload: axios.get(baseUrl, getAuthHeaders())
    }
}

export function loadChildProjects(parentProjectId) {
    return {
        type: LOAD_CHILD_PROJECTS,
        payload: axios.get(`${baseUrl}/child-projects/${parentProjectId}`, getAuthHeaders())
    }
}

export function loadMostRecentProject() {
    return {
        type: LOAD_MOST_RECENT_PROJECT,
        payload: axios.get(`${baseUrl}/recent`, getAuthHeaders())
    }
}

export function addProject(project) {
    return {
        type: ADD_PROJECT,
        payload: axios.post(baseUrl, project, getAuthHeaders())
    };
}

export function editProject(project) {
    return {
        type: EDIT_PROJECT,
        payload: axios.put(baseUrl, project, getAuthHeaders())
    };
}
export function uploadBulkProjects(response) {
    console.log('response', response);
    return {
        type: UPLOAD_BULK_PROJECTS,
        payload: axios.get(`${baseUrl}/addBulkProject?path=${response}`,  getAuthHeaders())
    };
}
export function deleteProject(projectId) {
    return {
        type: DELETE_PROJECT,
        payload: {
            data: projectId,
            promise: axios.delete(`${baseUrl}/${projectId}`, getAuthHeaders())
        }
    };
}

export function addChildNode(childProject) {
    return {
        type: ADD_CHILD_NODE,
        payload: axios.post(`${baseUrl}/children`, childProject, getAuthHeaders())
    }
}

export function updateChildNode(childProject) {
    return {
        type: UPDATE_CHILD_NODE,
        payload: axios.put(`${baseUrl}/children`, childProject, getAuthHeaders())
    }
}

export function importFromFormula({
    projectId,
    formulaId,
    selectedTasks,
    dates,
    mappings
}) {
    const rootDates = dates.rootStartDateTime.reduce((map, obj) => {
        let key = obj.key;
        if (obj.idDashed) {
            key = obj.idDashed.split("-")[1];
        }
        map[key] = obj.value;
        return map;
    }, {});

    return {
        type: IMPORT_FORMULA_TASKS,
        payload: axios.put(
            `${baseUrl}/${projectId}/import-tasks`,
            {
                formulaId,
                checkedTasks: selectedTasks,
                projectStartDates: {
                    projectStartDateTime: dates.projectStartDateTime,
                    rootStartDateTime: rootDates
                },
                skillMappings: mappings
            },
            getAuthHeaders())
    }
}

export default typeToReducer(reducer, initialState)
