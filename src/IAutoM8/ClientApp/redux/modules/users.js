import axios from 'axios'
import typeToReducer from 'type-to-reducer'

import { getAuthHeaders } from '@infrastructure/auth'

// Types
const LOAD_USERS = 'iauto/users/LOAD_USERS';
const LOAD_USERS_FOR_ALL_PROJECTS = 'iauto/users/LOAD_USERS_FOR_ALL_PROJECTS';
const SET_FILTER_USER_ID = 'modus/users/SET_FILTER_USER_ID';
const ADD_USER = 'iauto/users/ADD_USER';
const CHANGE_ROLE = 'iauto/users/CHANGE_ROLE';
const DELETE_USER = 'iauto/users/DELETE_USER';
const LOCK_USER = 'iauto/users/LOCK_USER';
const UNLOCK_USER = 'iauto/users/UNLOCK_USER';
const RESEND_EMAIL = 'iauto/users/RESEND_EMAIL';

const initialState = {
    loading: false,
    users: [],
    filterUserId: 0,
    error: null
};

// Reducer
const reducer = {
    [LOAD_USERS]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            users: action.payload.data
        })
    },

    [DELETE_USER]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            users: action.payload.data
        })
    },

    [LOAD_USERS_FOR_ALL_PROJECTS]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            users: action.payload.data
        })
    },

    [SET_FILTER_USER_ID]: (state, action) => ({
        ...state,
        filterUserId: action.payload
    }),

    [ADD_USER]: {
        PENDING: (state) => ({ ...state, error: false }),
        REJECTED: (state) => ({ ...state, error: true }),
        FULFILLED: (state) => ({ ...state, error: false })
    },

    [LOCK_USER]: {
        PENDING: (state) => ({ ...state, error: false }),
        REJECTED: (state) => ({ ...state, error: true }),
        FULFILLED: (state, action) => {
            const users = [...state.users];
            const user = users.find(u => u.id.toString() === action.payload.data);
            user.isLocked = true;
            return ({
                ...state,
                error: false,
                users
            });
        }
    },

    [UNLOCK_USER]: {
        PENDING: (state) => ({ ...state, error: false }),
        REJECTED: (state) => ({ ...state, error: true }),
        FULFILLED: (state, action) => {
            const users = [...state.users];
            const user = users.find(u => u.id.toString() === action.payload.data);
            user.isLocked = false;
            return ({
                ...state,
                error: false,
                users
            });
        }
    },

    [CHANGE_ROLE]: {
        PENDING: (state, action) => {
            const users = [...state.users];
            const user = users.find(u => u.id === action.payload.user.id);
            user.roles = [action.payload.role];
            return ({ ...state, users })
        }
    },
    [RESEND_EMAIL]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state) => ({ ...state, loading: false })
    },
};

// Actions
const baseUrl = '/api/admin';

export function loadUsers() {
    return {
        type: LOAD_USERS,
        payload: axios.get(`${baseUrl}/assigned-users`, getAuthHeaders())
    }
}


export function loadUsersForAllProjects() {
    return {
        type: LOAD_USERS_FOR_ALL_PROJECTS,
        payload: axios.get(`${baseUrl}/getUsersForAllProjects`, getAuthHeaders())
    }
}

export function addUser(user) {
    return {
        type: ADD_USER,
        payload: axios.post(`${baseUrl}/assignuser`, user, getAuthHeaders())
    }
}

export function deleteUser(userId) {
    return {
        type: DELETE_USER,
        payload: axios.delete(`${baseUrl}/${userId}`, getAuthHeaders())
    }
}

export function lockUser(userId) {
    return {
        type: LOCK_USER,
        payload: axios.put(`${baseUrl}/lockuser`, JSON.stringify(userId), getAuthHeaders())
    }
}

export function unlockUser(userId) {
    return {
        type: UNLOCK_USER,
        payload: axios.put(`${baseUrl}/unlockuser`, JSON.stringify(userId), getAuthHeaders())
    }
}

export function makeWorker(user) {
    return {
        type: CHANGE_ROLE,
        payload: {
            data: { user, role: 'Worker' },
            promise: axios.put(`${baseUrl}/makeworker`, user, getAuthHeaders())
        }
    }
}

export function makeManager(user) {
    return {
        type: CHANGE_ROLE,
        payload: {
            data: { user, role: 'Manager' },
            promise: axios.put(`${baseUrl}/makemanager`, user, getAuthHeaders())
        }
    }
}

export function resend(userId) {
    return {
        type: RESEND_EMAIL,
        payload: axios.put(`${baseUrl}/resend`, JSON.stringify(userId), getAuthHeaders())
    }
}

export function setFilterUserId(userId) {
    return {
        type: SET_FILTER_USER_ID,
        payload: userId
    }
}

export default typeToReducer(reducer, initialState)
