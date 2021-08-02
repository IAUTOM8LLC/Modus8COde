import axios from 'axios'
import typeToReducer from 'type-to-reducer'
import update from 'immutability-helper'

import { getAuthHeaders } from '@infrastructure/auth'
import { isOverdue, mapStatus, updateTasksForState } from '@utils/task'
import { isNull } from 'lodash'

// Types
const LOAD_PROJECT_TASKS = 'react/project-tasks/LOAD_PROJECT_TASKS';
const LOAD_ALL_PROJECTS_TASKS = 'react/project-tasks/LOAD_ALL_PROJECTS_TASKS';
const UPDATE_TASKS_FOR_PROJECT = 'react/project-tasks/UPDATE_TASKS_FOR_PROJECT';
const UPDATE_PROCESSING_USER = 'react/project-tasks/UPDATE_PROCESSING_USER';
const UPDATE_NEW_PROCESSING_USER = 'react/project-tasks/UPDATE_NEW_PROCESSING_USER';
const CHECK_OVERDUE = 'react/project-tasks/CHECK_OVERDUE';
const ADD = 'react/project-tasks/ADD';
const EDIT = 'react/project-tasks/EDIT';
const DELETE = 'react/project-tasks/DELETE';
const UPDATE_STATUS = 'react/project-tasks/UPDATE_STATUS';
const UPDATE_STATUS_LIST = 'react/project-tasks/UPDATE_STATUS_LIST';
const UPDATE_POSITIONS = 'react/project-tasks/UPDATE_POSITIONS';
const ADD_CONNECTION = 'react/project-tasks/ADD_CONNECTION';
const REMOVE_CONNECTION = 'react/project-tasks/REMOVE_CONNECTION';
const SELECT_CONDITION_OPTION = 'react/project-tasks/SELECT_CONDITION_OPTION';
const LOAD_TASKS_HISTORY = 'react/project-tasks/LOAD_TASKS_HISTORY';
const LOAD_TASKS_HISTORY_FOR_ALL_PROJECTS = 'react/project-tasks/LOAD_TASKS_HISTORY_FOR_ALL_PROJECTS';
const DO = 'react/project-tasks/DO';
const DO_VENDOR = 'react/project-tasks/DO_VENDOR';
const REVIEW = 'react/project-tasks/REVIEW';
const ADD_FORMULA = 'react/project-tasks/ADD_FORMULA';
const STOP_OUTSOURCE = 'react/project-tasks/STOP_OUTSOURCE';
const SET_PROJECTS_TO_LOAD = 'react/project-tasks/SET_PROJECTS_TO_LOAD';
const LOAD_TASK_IN_STATUS = 'react/project-tasks/LOAD_TASK_IN_STATUS';
const LOAD_USER_TASK_IN_STATUS = 'react/project-tasks/LOAD_USER_TASK_IN_STATUS';
const LOAD_VENDOR_TASK_IN_STATUS = 'react/project-tasks/LOAD_VENDOR_TASK_IN_STATUS';
const ADD_CHECKED_TODOS = 'react/project-tasks/ADD_CHECKED_TODOS';
const ADD_TODOS_FOR_WORKER = 'react/project-task/ADD_TODOS_FOR_WORKER';
const ADD_TODOS_FOR_REVIEWER = 'react/project-task/ADD_TODOS_FOR_REVIEWER';
const UPDATE_TASK_TODOS = 'react/project-task/UPDATE_TASK_TODOS';
const CANCEL_VENDOR_TASK = 'react/project-task/CANCEL_VENDOR_TASK';
const UPDATE_EXPIRED_JOB_INVITES_TO_OWNER = 'react/project-task/UPDATE_EXPIRED_JOB_INVITES_TO_OWNER';
const PROJECT_TASK_FILTER = 'react/project-task/FORMULA_FILTER';
const SELECT_PROJECT_TASK_USER = 'react/project-task/SELECT_PROJECT_TASK_USER';
const SELECT_PROJECT_TASK_VENDOR_USER = 'react/project-task/SELECT_PROJECT_TASK_VENDOR_USER';
const SELECT_ALL_PROJECT_TASK_USERS = 'react/project-task/SELECT_ALL_PROJECT_TASK_USERS';
const UN_SELECT_ALL_PROJECT_TASK_USERS = 'react/project-task/UN_SELECT_ALL_PROJECT_TASK_USERS';
const FILTER_TASKS_BY_USER_TYPE = 'react/project-task/FILTER_TASKS_BY_USER_TYPE';

const take = 6;

const initialState = {
    loading: false,
    tasks: [],
    userTasks: [],
    projectTaskUsers: [],
    projectVendorUsers: [],
    selectedUsers: [],
    selectedVendorUsers: [],
    tasksHistory: [],
    projectsToLoad: [],
    hasMoreProjects: true,
    hasMoreTasks: {
        New: true,
        NeedsReview: true,
        InProgress: true
    },
    filterByStatusModel: {
        selectedFilter: 'ALL',
        isAllEnabled: true,
        isUpcomingEnabled: false,
        isOverdueEnabled: false,
        isAtRiskEnabled: false,
        isTodoEnabled: false,
        isReviewEnabled: false,
        isCompletedEnabled: false,
    },
    outsourcerSelected: false,
    todos: [],
    workerTodos: [],
    reviewerTodos: [],
    notes: []
};

// Reducer
const reducer = {
    [CHECK_OVERDUE]: (state) => ({
        ...state,
        tasks: state.tasks.map(t => {
            t.isOverdue = isOverdue(t);
            return t;
        })
    }),

    [LOAD_PROJECT_TASKS]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => {
            return ({
                ...state,
                loading: false,
                tasks: action.payload.data.map(mapStatus)
            })
        }
    },

    [LOAD_ALL_PROJECTS_TASKS]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => {
            const previousTasks = [...state.tasks];
            const tasks = previousTasks.concat(action.payload.data);
            state.projectsToLoad.splice(0, 3);
            const hasMore = state.projectsToLoad.length !== 0;
            return ({ ...state, tasks, hasMore, loading: false });
        }
    },

    [LOAD_TASK_IN_STATUS]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => {
            const previousTasks = [...state.tasks];
            const hasMorePrivious = state.hasMoreTasks;

            const newTasks = action.payload.data.items;
            if (newTasks.length === 0) {
                switch (action.payload.data.status) {
                    case 'New':
                        hasMorePrivious['New'] = false;
                        break;
                    case 'InProgress':
                        hasMorePrivious['InProgress'] = false;
                        break;
                    case 'NeedsReview':
                        hasMorePrivious['NeedsReview'] = false;
                        break;
                }
            } else {
                hasMorePrivious['New'] = newTasks.filter(t => t.status === 'New').length === take;
                hasMorePrivious['InProgress'] = newTasks
                    .filter(t => t.status === 'InProgress').length === take;
                hasMorePrivious['NeedsReview'] = newTasks
                    .filter(t => t.status === 'NeedsReview').length === take;
            }

            const tasks = previousTasks.concat(newTasks).map(t => {
                t.isOverdue = isOverdue(t);
                return t;
            });
            return ({
                ...state,
                loading: false,
                tasks,
                hasMore: hasMorePrivious
            });
        }
    },

    [LOAD_USER_TASK_IN_STATUS]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            userTasks: action.payload.data.userTasks,
            projectTaskUsers: action.payload.data.usersList,
            projectVendorUsers: action.payload.data.outsourceList,
            selectedUsers: action.payload.data.usersList.map(u => u.userId),
            selectedVendorUsers: action.payload.data.outsourceList.map(u => u.userId),
        })
    },

    [LOAD_VENDOR_TASK_IN_STATUS]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            userTasks: action.payload.data,
        })
    },

    [SET_PROJECTS_TO_LOAD]: (state, action) => ({
        ...state,
        tasks: [],
        projectsToLoad: action.payload,
        hasMore: true
    }),

    [UPDATE_TASKS_FOR_PROJECT]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            tasks: updateTasksForState(state.tasks, action.payload.data)
        })
    },

    [UPDATE_PROCESSING_USER]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => {
            const tasks = [...state.tasks];
            const index = tasks.findIndex(t => t.id === Number(action.payload.data.id));
            tasks[index] = mapStatus(action.payload.data);
            return ({ ...state, tasks, loading: false });
        }
    },

    [UPDATE_NEW_PROCESSING_USER]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => {
            const {
                proccessingUserId,
                proccessingUserName,
                reviewingUserName, id
            } = action.payload.data;
            const userTasks = [...state.userTasks];
            const index = userTasks.findIndex(t => t.id === Number(id));

            return ({
                ...state,
                loading: false,
                userTasks: update(userTasks, {
                    [index]: {
                        proccessingUserName: {
                            $set: proccessingUserName || reviewingUserName
                        },
                        proccessingUserId: {
                            $set: proccessingUserId
                        }
                    }
                })
            });
        }
    },

    [STOP_OUTSOURCE]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => {
            const tasks = [...state.tasks];
            const index = tasks.findIndex(t => t.id === Number(action.payload.data.id));
            tasks[index] = mapStatus(action.payload.data);
            return ({ ...state, tasks, loading: false });
        }
    },

    [LOAD_TASKS_HISTORY]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            tasksHistory: action.payload.data
        })
    },

    [LOAD_TASKS_HISTORY_FOR_ALL_PROJECTS]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            tasksHistory: action.payload.data
        })
    },

    [ADD]: {
        FULFILLED: (state, action) => {
            return ({
                ...state,
                tasks: [...state.tasks, mapStatus(action.payload.data)]
            });
        }
    },
    [EDIT]: {
        FULFILLED: (state, action) => {
            const tasks = [...state.tasks];
            const index = tasks.findIndex(t => t.id === Number(action.payload.data.id));
            tasks[index] = mapStatus(action.payload.data);
            return ({ ...state, tasks });
        }
    },

    [DELETE]: {
        PENDING: (state, action) => {
            const task = action.payload;
            const toDelete = [task.id];

            const includeNestedFormulaTasks = _task => {
                const nestedTasks = state.tasks
                    .filter(t => t.parentTaskId === _task.id)
                    .map(t => {
                        if (t.formulaId)
                            includeNestedFormulaTasks(t)
                        return t.id;
                    });

                toDelete.push(...nestedTasks);
            }

            if (task.formulaId)
                // recursive calls !
                includeNestedFormulaTasks(task);

            return {
                ...state,
                loading: true,
                tasks: state.tasks.filter(p => !toDelete.includes(p.id))
            }
        },
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state) => ({ ...state, loading: false })
    },

    [UPDATE_STATUS]: {
        PENDING: (state, action) => {
            // console.log('...state.tasks',...state.tasks);
            // console.log('action',action.payload);
            const tasks = [...state.tasks, action.payload];
            // console.log('taskss',tasks);
            const task = tasks.find(t => t.id === Number(action.payload.id));
            // console.log('taask', task);
            task.status = action.payload.status;
            return ({ ...state, tasks: tasks.map(mapStatus) });
        }
    },

    [UPDATE_STATUS_LIST]: (state, { payload: taskStatusList }) => {
        let updatedTasks;
        for (const [taskId, status] of taskStatusList) {
            const index = state.tasks.findIndex(t => t.id === taskId);
            if (index !== -1) {
                updatedTasks = update(state.tasks,
                    {
                        [index]: {
                            $apply: t => {
                                t.status = status;
                                return t;
                            }
                        }
                    });
            }
        }

        if (!updatedTasks) {
            updatedTasks = state.tasks;
        }

        return ({ ...state, tasks: updatedTasks.map(mapStatus) });
    },

    [UPDATE_POSITIONS]: {
        PENDING: (state, action) => {
            const { positions } = action.payload;
            let { tasks } = state;


            for (const p of positions) {
                const idx = tasks.findIndex(t => t.id === p.id);
                tasks = update(tasks, {
                    [idx]: {
                        $apply: (task) => ({
                            ...task,
                            posX: p.posX,
                            posY: p.posY
                        })
                    }
                })
            }

            return { ...state, tasks: tasks };
        }
    },

    [ADD_CONNECTION]: {
        PENDING: (state) => ({ ...state, loading: false }),
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state, action) => {
            let { parentTaskId, childTaskId } = action.payload;
            parentTaskId = Number(parentTaskId);
            childTaskId = Number(childTaskId);

            const { tasks } = state;

            const source = tasks.find(t => t.id === parentTaskId);
            const target = tasks.find(t => t.id === childTaskId);

            if (!source.childTasks.includes(childTaskId))
                source.childTasks.push(childTaskId);

            if (!target.parentTasks.includes(parentTaskId))
                target.parentTasks.push(parentTaskId);

            return ({
                ...state,
                loading: true,
                tasks: [...tasks]
            })
        }
    },

    [REMOVE_CONNECTION]: {
        PENDING: (state, action) => {
            let { parentTaskId, childTaskId } = action.payload;
            parentTaskId = Number(parentTaskId);
            childTaskId = Number(childTaskId);

            const { tasks } = state;

            const source = tasks.find(t => t.id === parentTaskId);
            const target = tasks.find(t => t.id === childTaskId);

            source.childTasks = source.childTasks.filter(i => i !== childTaskId);
            target.parentTasks = target.parentTasks.filter(i => i !== parentTaskId);

            return ({
                ...state,
                tasks: [...tasks]
            })
        }
    },

    [SELECT_CONDITION_OPTION]: {
        FULFILLED: (state) => state
    },

    [DO]: {
        FULFILLED: (state, action) => {
            const index = state.tasks.findIndex(t => t.id === Number(action.payload.data.id));

            return {
                ...state,
                tasks: update(state.tasks, {
                    [index]: {
                        $set: action.payload.data
                    }
                })
            };
        }
    },
    [DO_VENDOR]: {
        FULFILLED: (state, action) => {

            const index = state.tasks.findIndex(t => t.id === Number(action.payload.data.id));

            return {
                ...state,
                tasks: update(state.tasks, {
                    [index]: {
                        $set: action.payload.data
                    }
                })
            };
        }
    },

    [REVIEW]: {
        FULFILLED: (state, action) => {
            const index = state.tasks.findIndex(t => t.id === Number(action.payload.data.id));

            return {
                ...state,
                tasks: update(state.tasks, {
                    [index]: {
                        $set: action.payload.data
                    }
                })
            };
        }
    },

    [ADD_FORMULA]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            tasks: update(state.tasks, { $push: action.payload.data })
        })
    },

    [CANCEL_VENDOR_TASK]: {
        REJECTED: (state) => ({ ...state, loading: false }),
        PENDING: (state, action) => {
            return {
                ...state,
                tasks: state.tasks.filter(t => t.id !== action.payload),
                loading: false
            };
        }
    },

    [ADD_CHECKED_TODOS]: (state, action) => ({
        ...state,
        todos: action.payload
    }),

    [ADD_TODOS_FOR_WORKER]: (state, action) => ({
        ...state,
        workerTodos: action.payload
    }),

    [ADD_TODOS_FOR_REVIEWER]: (state, action) => ({
        ...state,
        reviewerTodos: action.payload
    }),

    [UPDATE_EXPIRED_JOB_INVITES_TO_OWNER]: (state) => ({ ...state }),

    [PROJECT_TASK_FILTER]: {
        FULFILLED: (state, action) => ({
            ...state,
            filterByStatusModel: {
                ...state.filterByStatusModel,
                selectedFilter: action.payload,
                isAllEnabled: action.payload === "ALL",
                isUpcomingEnabled: action.payload === "UPCOMING",
                isOverdueEnabled: action.payload === "OVERDUE",
                isAtRiskEnabled: action.payload === "ATRISK",
                isTodoEnabled: action.payload === "TODO",
                isReviewEnabled: action.payload === "REVIEW",
                isCompletedEnabled: action.payload === "COMPLETED"
            }
        })
    },

    [FILTER_TASKS_BY_USER_TYPE]: {
        FULFILLED: (state, action) => ({
            ...state,
            outsourcerSelected: !action.payload
        })
    },

    [SELECT_PROJECT_TASK_USER]: {
        FULFILLED: (state, action) => {
            const { selectedUsers } = state;

            // console.log("selectedUsershhhhh", selectedUsers);
            const userExists = selectedUsers.includes(action.payload);
            // console.log("selectedUserspayload", action.payload);
            if (userExists) {
                return {
                    ...state,
                    selectedUsers: selectedUsers.filter((u) => u !== action.payload)
                }
            } else {
                return {
                    ...state,
                    selectedUsers: [...selectedUsers, action.payload]
                }
            }
        }
    },


    [SELECT_PROJECT_TASK_VENDOR_USER]: {
        FULFILLED: (state, action) => {
            // console.log('vendor', state);
            // console.log("vendorUserid", state.selectedUsers);
            // console.log('selectedVendorUsers',state.selectedVendorUsers);
            const { selectedUsers, selectedVendorUsers } = state;
            const userExists = selectedVendorUsers.includes(action.payload);
            // console.log("vendorselectedUserspayload", action.payload);
            const ppp = selectedVendorUsers.filter((u) => u !== action.payload)
            // console.log('rrr', ppp);


            // console.log("selectedUsers111", selectedUsers);
            // console.log("selectedVendorUsers111", selectedVendorUsers);
            if (userExists) {
                return {
                    ...state,
                    selectedVendorUsers: selectedVendorUsers.filter((u) => u !== action.payload),
                    //userTasks:[state.selectedVendorUsers= {selectedVendorUsers}]
                }
            } else {
                return {
                    ...state,
                    selectedVendorUsers: [...selectedVendorUsers, action.payload]
                }
            }
        }
    },

    [SELECT_ALL_PROJECT_TASK_USERS]: {
        FULFILLED: (state) => ({
            ...state,
            selectedUsers: state.projectTaskUsers.map(u => u.userId),
            selectedVendorUsers: state.projectVendorUsers.map(u => u.userId)
        })
    },

    [UN_SELECT_ALL_PROJECT_TASK_USERS]: {
        FULFILLED: (state) => ({
            ...state,
            selectedUsers: [],
            selectedVendorUsers: []
        })
    },

    [UPDATE_TASK_TODOS]: {
        PENDING: (state) => ({ ...state }),
        REJECTED: (state) => ({ ...state }),
        FULFILLED: (state) => ({ ...state })
    },
};

// Actions
const baseUrl = '/api/tasks';

export function loadTasks(projectId) {
    return {
        type: LOAD_PROJECT_TASKS,
        payload: axios.get(`${baseUrl}/projects/${projectId}`, getAuthHeaders())
    };
}

export function updateTasksForProject(projectId) {
    return {
        type: UPDATE_TASKS_FOR_PROJECT,
        payload: axios.get(`${baseUrl}/projects/${projectId}`, getAuthHeaders())
    };
}


export function updateProcessingUser(processingUser) {
    // console.log('actionupdate called', processingUser);
    return {
        type: UPDATE_PROCESSING_USER,
        payload: axios.post(`${baseUrl}/processingUser`, processingUser, getAuthHeaders())
    };
}

export function updateNewProcessingUser(processingUser) {
    // console.log('actionupdatenew called', processingUser);
    return {
        type: UPDATE_NEW_PROCESSING_USER,
        payload: axios.post(`${baseUrl}/changeProcessingUser`, processingUser, getAuthHeaders())
    };
}

export function loadTasksForAllProjects() {
    return (dispatch, getStore) => {
        const projectIds = [...getStore().projectTasks.projectsToLoad];
        const projectsToLoad = projectIds.splice(0, 3).map(item => `projectIds=${item}`).join('&')
        dispatch({
            type: LOAD_ALL_PROJECTS_TASKS,
            payload: axios.get(`${baseUrl}/projects/getTasksFromProjects?${projectsToLoad}`, getAuthHeaders())
        });
    }
}

export function loadTasksInStatus(statuses) {
    return (dispatch, getStore) => {
        const skipCount = getStore().projectTasks.tasks
            .filter(t => t.status === statuses).length;
        dispatch({
            type: LOAD_TASK_IN_STATUS,
            payload: axios.get(`${baseUrl}/${statuses}/${skipCount}/${take}`, getAuthHeaders())
        });
    }
}

export function loadOwnerUserTasksInStatus(userId, statuses) {
    return (dispatch, getStore) => {
        const skipCount = getStore().projectTasks.tasks
            .filter(t => t.status === statuses).length;
        dispatch({
            type: LOAD_TASK_IN_STATUS,
            payload: axios.get(`${baseUrl}/${userId}/${statuses}/${skipCount}/${take}`, getAuthHeaders())
        });
    }
}

export function loadUserTasksInStatus() {
    return {
        type: LOAD_USER_TASK_IN_STATUS,
        payload: axios.get(`${baseUrl}/alltasks`, getAuthHeaders())
    };
}

export function loadVendorTasksInStatus() {
    return {
        type: LOAD_VENDOR_TASK_IN_STATUS,
        payload: axios.get(`${baseUrl}/vendor-tasks`, getAuthHeaders())
    };
}

export function loadSelectedUserTasksInStatus(userIds, statuses) {
    return (dispatch, getStore) => {
        const skipCount = getStore().projectTasks.tasks
            .filter(t => t.status === statuses).length;
        dispatch({
            type: LOAD_TASK_IN_STATUS,
            payload: axios.post(`${baseUrl}/${statuses}/${skipCount}/${take}`, userIds, getAuthHeaders())
        });
    }
}

export function addTask(task) {
    return {
        type: ADD,
        payload: axios.post(baseUrl, task, getAuthHeaders())
    };
}

export function editTask(task) {
    return {
        type: EDIT,
        payload: axios.put(`${baseUrl}/${task.id}`, task, getAuthHeaders())
    };
}

export function stopOutsource(taskId) {
    return {
        type: STOP_OUTSOURCE,
        payload: axios.put(`${baseUrl}/stopOutsource?taskId=${taskId}`, null, getAuthHeaders())
    }
}

export function updateStatus(id, status, rating) {//taskId changed to id by AT 26-mar-2021 
    return {
        type: UPDATE_STATUS,
        payload: {
            data: { id, status },
            promise: axios.put(`${baseUrl}/${id}/status`, { status, rating }, getAuthHeaders())
        }
    }
}

export function updateStatusList(taskStatusList) {
    return {
        type: UPDATE_STATUS_LIST,
        payload: taskStatusList
    }
}


export function deleteTask(task) {
    return {
        type: DELETE,
        payload: {
            data: task,
            promise: axios.delete(`${baseUrl}/${task.id}`, getAuthHeaders())
        }
    };
}

export function addConnection(parentTaskId, childTaskId) {
    return {
        type: ADD_CONNECTION,
        payload: {
            data: { parentTaskId, childTaskId },
            promise: axios.put(`${baseUrl}/connect`, { parentTaskId, childTaskId }, getAuthHeaders())
        }
    };
}

export function removeConnection(parentTaskId, childTaskId) {
    return {
        type: REMOVE_CONNECTION,
        payload: {
            data: { parentTaskId, childTaskId },
            promise: axios.put(`${baseUrl}/disconnect`, { parentTaskId, childTaskId }, getAuthHeaders())
        }
    };
}

export function completeConditionalTask(taskId, conditionOptionId, status, rating) {
    return {
        type: SELECT_CONDITION_OPTION,
        payload: axios.post(`${baseUrl}/conditional/${taskId}/complete`,
            JSON.stringify({ conditionOptionId, status, rating }), getAuthHeaders())
    };
}

export function loadTasksHistory(projectId) {
    return {
        type: LOAD_TASKS_HISTORY,
        payload: axios.get(`${baseUrl}/history/${projectId}`, getAuthHeaders())
    }
}

export function loadTasksHistoryForAllProjects() {
    return {
        type: LOAD_TASKS_HISTORY_FOR_ALL_PROJECTS,
        payload: axios.get(`${baseUrl}/getAllProjectsHistory`, getAuthHeaders())
    }
}

export function doTask(taskId) {
    console.log('action do task',taskId);
    return {
        type: DO,
        payload: axios.put(`${baseUrl}/do-task/${taskId}`, null, getAuthHeaders())
    };
}
export function doVendorTask(taskId) {
    return {
        type: DO_VENDOR,
        payload: axios.put(`${baseUrl}/do-vendor-task/${taskId}`, null, getAuthHeaders())
    };
}

export function reviewTask(taskId) {
    //console.log('action call reviewTask',taskId);
    return {
        type: REVIEW,
        payload: axios.put(`${baseUrl}/review-task/${taskId}`, null, getAuthHeaders())
    };
}

export const checkOverdue = () => ({ type: CHECK_OVERDUE })

export function updatePositions(positions) {
    return {
        type: UPDATE_POSITIONS,
        payload: {
            data: { positions },
            promise: axios.put(`${baseUrl}/position`, positions, getAuthHeaders())
        }
    }
}

export function addFormulaTask(task) {
    return {
        type: ADD_FORMULA,
        payload: axios.post(`${baseUrl}/add-formula-task`, task, getAuthHeaders())
    };
}

export function setProjectsToLoad(pojectIds) {
    return {
        type: SET_PROJECTS_TO_LOAD,
        payload: pojectIds
    };
}

export function addProjectTaskChecklist(todos) {
    return {
        type: ADD_TODOS_FOR_WORKER,
        payload: todos
    }
}

export function addProjectTaskReviewCheckList(reviewerTodos) {
    return {
        type: ADD_TODOS_FOR_REVIEWER,
        payload: reviewerTodos
    }
}

export function addCheckedTodos(todos) {
    return {
        type: ADD_CHECKED_TODOS,
        payload: todos
    }
}

export function updateTaskTodos(todos) {
    return {
        type: UPDATE_TASK_TODOS,
        payload: axios.patch(`${baseUrl}/update-task-checklist`, todos, getAuthHeaders())
    }
}

export function cancelVendorTask(projectTaskId) {
    return {
        type: CANCEL_VENDOR_TASK,
        payload: {
            data: projectTaskId,
            promise: axios.post(`${baseUrl}/cancelOutsource`, projectTaskId, getAuthHeaders())
        }
    }
}

export function updateExpiredInvitesToOwner(taskId) {
    return {
        type: UPDATE_EXPIRED_JOB_INVITES_TO_OWNER,
        payload: axios.post(`${baseUrl}/expired-job-invites`, taskId, getAuthHeaders())
    }
}

export function projectTaskFilter(value) {
    return {
        type: PROJECT_TASK_FILTER,
        payload: {
            promise: Promise.resolve(value)
        }
    };
}

export function selectProjectTaskUser(user) {
    return {
        type: SELECT_PROJECT_TASK_USER,
        payload: {
            promise: Promise.resolve(user)
        }
    }
}

export function selectProjectTaskVendorUser(user) {
    // console.log('selectProjectTaskVendorUser action called', user);
    return {
        type: SELECT_PROJECT_TASK_VENDOR_USER,
        payload: {
            promise: Promise.resolve(user)
        }
    }
}

export function selectAllProjectTaskUsers() {
    return {
        type: SELECT_ALL_PROJECT_TASK_USERS,
        payload: {
            promise: Promise.resolve()
        }
    }
}

export function unSelectAllProjectTaskUsers() {
    return {
        type: UN_SELECT_ALL_PROJECT_TASK_USERS,
        payload: {
            promise: Promise.resolve()
        }
    }
}

export function filterTasksByUserType(value) {
    return {
        type: FILTER_TASKS_BY_USER_TYPE,
        payload: {
            promise: Promise.resolve(value)
        }
    }
}

export default typeToReducer(reducer, initialState)