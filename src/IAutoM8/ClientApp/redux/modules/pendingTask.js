/**
 * App-wide parameters are stored here
 */
import typeToReducer from 'type-to-reducer'
import axios from 'axios'
import update from 'immutability-helper'

import { getAuthHeaders } from '@infrastructure/auth'
import { mapStatus } from '@utils/task'
import { getFormValues } from 'redux-form'

import moment from 'moment'

// Types
const SET_PENDING_TASK = 'iauto/pendingTask/SET_PENDING_TASK';
const LOAD_PENDING_TASK = 'iauto/pendingTask/LOAD_PENDING_TASK';
const LOAD_TASK_OUTSOURCES = 'iauto/pendingTask/LOAD_TASK_OUTSOURCES';
const SET_FORMULA_OUTSOURCE = 'iauto/pendingTask/SET_FORMULA_OUTSOURCE';
const CLEAR_FORMULA_OUTSOURCE = 'iauto/pendingTask/CLEAR_FORMULA_OUTSOURCE';
const SAVE_FORMULA_OUTSOURCE = 'iauto/pendingTask/SAVE_FORMULA_OUTSOURCE';
const SET_PROJECT_OUTSOURCE = 'iauto/pendingTask/SET_PROJECT_OUTSOURCE';
const SAVE_PROJECT_OUTSOURCE = 'iauto/pendingTask/SAVE_PROJECT_OUTSOURCE';
const UPDATE_AVAILABLE_CREDITS = 'iauto/credits/UPDATE_AVAILABLE_CREDITS';
const HIDE_TRAINING_NOTIFICATION = 'iauto/pendingTask/HIDE_TRAINING_NOTIFICATION';
const PUBLISH_TASK = 'iauto/pendingTask/PUBLISH_TASK';
const LOAD_TASK_NOTES = 'iauto/pendingTask/LOAD_TASK_NOTES';
const CLEAR_TASK_NOTES = 'iauto/pendingTask/CLEAR_TASK_NOTES';
const CREATE_TASK_NOTE = 'iauto/pendingTask/CREATE_TASK_NOTE';
const REMOVE_TASK_NOTE = 'iauto/pendingTask/REMOVE_TASK_NOTE';
const PUBLISH_TASK_NOTE = 'iauto/pendingTask/PUBLISH_TASK_NOTE';
const LOCK_TRAINING = "iauto/pendingTask/LOCK_TRAINING";
const UNLOCK_TRAINING = "iauto/pendingTask/UNLOCK_TRAINING";

const count = 10;

const initialState = {
    loading: false,
    pendingTask: {},
    hasMore: true,
    taskNotes: [],
    outsources: [],
    updatedOutsources: [],
    showTrainingNotification: false
};

// Reducer
const reducer = {
    [LOAD_PENDING_TASK]: {
        PENDING: (state) => ({
            ...state,
            loading: true,
            hasMore: true,
            outsources: [],
            updatedOutsources: []
        }),
        REJECTED: (state) => ({
            ...state,
            loading: false
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            pendingTask: mapStatus(action.payload.data),
            showTrainingNotification: action.payload.data.descNotificationFlag
        })
    },
    [SET_PENDING_TASK]: (state, action) => ({
        ...state, pendingTask: mapStatus(action.payload),
        hasMore: true,
        outsources: [],
        showTrainingNotification: false,
        updatedOutsources: []
    }),

    [LOAD_TASK_OUTSOURCES]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => {
            const [res, task] = action.payload;
            const condition = state.pendingTask.condition;
            return ({
                ...state,
                loading: false,
                outsources: update(state.outsources, {
                    $push: res.data
                }),
                hasMore: res.data.length === count,
                pendingTask: { ...task, condition}
            })
        }
    },
    [SET_FORMULA_OUTSOURCE]: {
        FULFILLED: (state, action) => {
            console.log('payloadrole',action.payload.role );
            console.log('payloadid',action.payload.ownerid );
            const index = state.updatedOutsources
                .findIndex(o => o.id === action.payload.id);
            const indexOutsource = state.outsources
                .findIndex(o => o.id === action.payload.id);
            if (index > -1) {
                return ({
                    ...state,
                    pendingTask: action.payload.task,
                    updatedOutsources: update(state.updatedOutsources, {
                        [index]: {
                            $set: {
                                id: action.payload.id,
                                isSelected: action.payload.isSelected,
                                role: action.payload.role,
                                ownerid: action.payload.ownerid,

                            }
                        }
                    }),
                    outsources: update(state.outsources, {
                        [indexOutsource]: {
                            $apply: (out) => ({
                                ...out,
                                status: action.payload.isSelected ?
                                    'None' : 'DeclinedByOwner'
                            })
                        }
                    })
                })
            }
            else {
                return ({
                    ...state,
                    pendingTask: action.payload.task,
                    updatedOutsources: update(state.updatedOutsources, {
                        $push: [{
                            id: action.payload.id,
                            isSelected: action.payload.isSelected,
                            role: action.payload.role,
                            ownerid: action.payload.ownerid,
                        }]
                    }),
                    outsources: update(state.outsources, {
                        [indexOutsource]: {
                            $apply: (out) => ({
                                ...out,
                                status: action.payload.isSelected ?
                                    'None' : 'DeclinedByOwner'
                            })
                        }
                    })
                })
            }
        }
    },
    [CLEAR_FORMULA_OUTSOURCE]: (state) => {
        return ({
            ...state,
            hasMore: true,
            outsources: [],
            updatedOutsources: []
        })
    },
    [SAVE_FORMULA_OUTSOURCE]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state) => ({
            ...state,
            loading: false
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            hasMore: true,
            outsources: [],
            updatedOutsources: []
        })
    },
    [SET_PROJECT_OUTSOURCE]: {
        FULFILLED: (state, action) => {
            const index = state.updatedOutsources
                .findIndex(o => o.id === action.payload.id);
            const indexOutsource = state.pendingTask.outsources
                .findIndex(o => o.id === action.payload.id);
            if (index > -1) {
                return ({
                    ...state,
                    updatedOutsources: update(state.updatedOutsources, {
                        [index]: {
                            $set: action.payload
                        }
                    }),
                    pendingTask: {
                        ...state.pendingTask,
                        outsources: update(state.pendingTask.outsources, {
                            [indexOutsource]: {
                                $apply: (out) => ({
                                    ...out,
                                    status: action.payload.isSelected ?
                                        'Send' : 'None'
                                })
                            }
                        })
                    }
                })
            }
            else {
                return ({
                    ...state,
                    updatedOutsources: update(state.updatedOutsources, {
                        $push: [action.payload]
                    }),
                    pendingTask: {
                        ...state.pendingTask,
                        outsources: update(state.pendingTask.outsources, {
                            [indexOutsource]: {
                                $apply: (out) => ({
                                    ...out,
                                    status: action.payload.isSelected ?
                                        'Send' : 'None'
                                })
                            }
                        })
                    }
                })
            }
        }
    },
    [SAVE_PROJECT_OUTSOURCE]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state) => ({
            ...state,
            loading: false
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            hasMore: true,
            outsources: [],
            updatedOutsources: []
        })
    },
    [HIDE_TRAINING_NOTIFICATION]: {
        FULFILLED: (state) => ({
            ...state,
            showTrainingNotification: false
        })
    },

    [PUBLISH_TASK]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            publishedTask: action.payload.data
        }),
        REJECTED: (state) => ({ ...state, loading: false })
    },
    [CLEAR_TASK_NOTES]: (state) => ({
        ...state,
        taskNotes: []
    }),
    [LOAD_TASK_NOTES]: {
        PENDING: (state) => ({
            ...state,
        }),
        REJECTED: (state) => ({
            ...state,
        }),
        FULFILLED: (state, action) => {
            return {
                ...state,
                taskNotes: action.payload.data
            };
        },
    },
    [CREATE_TASK_NOTE]: {
        PENDING: (state) => ({
            ...state,
        }),
        REJECTED: (state) => ({
            ...state,
        }),
        FULFILLED: (state, action) => ({
            ...state,
            taskNotes: update(state.taskNotes, { $unshift: [action.payload.data] })
        }),
    },
    [PUBLISH_TASK_NOTE]: {
        FULFILLED: (state, action) => {
            const index = state.taskNotes.findIndex(n => n.id === Number(action.payload.data.id));
            if (index > -1) {
                return {
                    ...state,
                    taskNotes: update(state.taskNotes, {
                        [index]: {
                            $set: action.payload.data
                        }
                    })
                }
            } else {
                return { ...state }
            }
        }
    },
    [REMOVE_TASK_NOTE]: {
        PENDING: (state, action) => {
            return {
                ...state,
                taskNotes: state.taskNotes.filter(n => n.id !== action.payload)
            };
        },
        REJECTED: (state) => ({
            ...state,
        }),
        FULFILLED: (state) => ({
            ...state,
        })
    },
    [LOCK_TRAINING]: {
        PENDING: (state) => ({ ...state, }),
        REJECTED: (state, action) => ({ 
            ...state,
            error: action.payload.data,
        }),
        FULFILLED: (state) => ({
            ...state,
            pendingTask: { 
                ...state.pendingTask, 
                isTrainingLocked: true,
            }
        })
    },
    [UNLOCK_TRAINING]: {
        PENDING: (state) => ({ ...state, }),
        REJECTED: (state, action) => ({
            ...state,
            error: action.payload.data,
        }),
        FULFILLED: (state) => ({
            ...state,
            pendingTask: {
                ...state.pendingTask, 
                isTrainingLocked: false,
            }
        })
    },
};

// Actions
export function loadProjectTask(id) {
    
    return {
        type: LOAD_PENDING_TASK,
        payload: axios.get(`/api/tasks/${id}`, getAuthHeaders())
    };
}

export function loadFormulaTask(id, formulaId) {
    return {
        type: LOAD_PENDING_TASK,
        payload: axios.get(`/api/formulaTask/${id}/${formulaId}`, getAuthHeaders())
    };
}

export function loadFormulaOutsources(id) {
    return (dispatch, getStore) => {
        const outSourceCount = getStore().pendingTask.outsources.length;
        dispatch({
            type: LOAD_TASK_OUTSOURCES,
            payload: Promise.all([
                axios.get(`/api/formulaTask/outsources/` +
                    `${id}/${outSourceCount}/${count}`, getAuthHeaders()),
                Promise.resolve(getFormValues('taskFormModal')(getStore()))
            ])
        });
    }
}

export function loadFormulaNotes(formulaId) {
    return {
        type: LOAD_TASK_NOTES,
        payload: axios.get(`/api/formulaTask/notes/${formulaId}`, getAuthHeaders())
    };
}

export function loadProjectNotes(taskId) {    
    return {
        type: LOAD_TASK_NOTES,
        payload: axios.get(`/api/tasks/notes/${taskId}`, getAuthHeaders())
    }
}

export function saveFormulaOutsource(id) {
    return (dispatch, getStore) => {
        dispatch({
            type: SAVE_FORMULA_OUTSOURCE,
            payload: axios.put(`/api/formulaTask/outsources`, {
                taskId: id,
                outsources: getStore().pendingTask.updatedOutsources
            }, getAuthHeaders())
        });
    };
}

export function setFormulaOutsourceAssigne(id, isSelected, role ,ownerid) {
    return (dispatch, getStore) => {
        dispatch({
            type: SET_FORMULA_OUTSOURCE,
            payload: Promise.resolve({
                id, isSelected,role ,ownerid,
                task: getFormValues('taskFormModal')(getStore())
            })
        })
    };
}
export function setProjectOutsourceAssigne(id, isSelected) {
    return (dispatch, getStore) => {

        dispatch({
            type: SET_PROJECT_OUTSOURCE,
            payload: Promise.resolve({ id, isSelected })
        });
    }
}
export function saveProjectOutsourceAssigne(id) {
    return (dispatch, getStore) => {
        dispatch({
            type: SAVE_PROJECT_OUTSOURCE,
            payload: axios.put(`/api/tasks/outsources`, {
                taskId: id,
                outsources: getStore().pendingTask.updatedOutsources
            }, getAuthHeaders())
        });
    };
}

export function clearFormulaOutsoureces() {
    return {
        type: CLEAR_FORMULA_OUTSOURCE
    };
}

export function setPendingTask(task) {

    return {
        type: SET_PENDING_TASK,
        payload: task
    };
}

export function hideTrainingNotification() {
    return (dispatch, getState) => {
        const { pendingTask: { pendingTask: { id } } } = getState();
        dispatch({
            type: HIDE_TRAINING_NOTIFICATION,
            payload: axios.put(`/api/tasks/${id}/notification`, {}, getAuthHeaders())
        })
    };
}

export function publishProjectTask(id, params) {
    return {
        type: PUBLISH_TASK,
        payload: axios.put(`/api/tasks/${id}/status`, params, getAuthHeaders())
    };
}

export function clearTaskNotes() {
    return {
        type: CLEAR_TASK_NOTES
    }
}

export function createFormulaNote(formulaNote) {
    return {
        type: CREATE_TASK_NOTE,
        payload: axios.post("/api/formulaTask/notes", formulaNote, getAuthHeaders())
    }
}

export function createProjectNote(projectNote, sharedTaskIds) {
    return {
        type: CREATE_TASK_NOTE,
        payload: axios.post("/api/tasks/notes",
            JSON.stringify({ projectNote, sharedTaskIds }),
            getAuthHeaders())
    }
}

export function publishProjectNote(noteId, isPublished) {
    return {
        type: PUBLISH_TASK_NOTE,
        payload: axios.put(`/api/tasks/notes/${noteId}/${isPublished}`, null, getAuthHeaders())
    }
}

export function removeFormulaNote(noteId) {
    return {
        type: REMOVE_TASK_NOTE,
        payload: {
            data: noteId,
            promise: axios.delete(`/api/formulaTask/notes/${noteId}`, getAuthHeaders())
        }
    }
}

export function removeProjectNote(noteId) {
    return {
        type: REMOVE_TASK_NOTE,
        payload: {
            data: noteId,
            promise: axios.delete(`/api/tasks/notes/${noteId}`, getAuthHeaders())
        }
    }
}

export function lockTraining(taskId) {
    return {
        type: LOCK_TRAINING,
        payload: axios
            .put(`/api/formulaTask/lock-training`, JSON.stringify(taskId), getAuthHeaders())
    }
}

export function unlockTraining(taskId) {
    return {
        type: UNLOCK_TRAINING,
        payload: axios
            .put(`/api/formulaTask/unlock-training`, JSON.stringify(taskId), getAuthHeaders())
    }
}

export default typeToReducer(reducer, initialState)
