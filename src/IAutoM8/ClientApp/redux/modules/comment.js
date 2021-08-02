import axios from 'axios'
import typeToReducer from 'type-to-reducer'

import { getAuthHeaders } from '@infrastructure/auth'

// Types
const LOAD_TASK_COMMENTS = 'react/project-task/LOAD_TASK_COMMENTS';
const ADD = 'react/project-task/ADD_COMMENT';
const DELETE = 'react/project-task/DELETE_COMMENT';

const initialState = {
    loading: false,
    comments: []
};

// Reducer
const reducer = {
    [LOAD_TASK_COMMENTS]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => ({
            loading: false,
            comments: action.payload.data
        })
    },

    [ADD]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: ({ comments }, action) => ({
            loading: false,
            comments: [...comments, action.payload.data]
        })

    },

    [DELETE]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => {
            const { comments } = state;
            return {
                loading: false,
                comments: comments.filter(comment => comment.id !== action.payload.data)
            }
        }
    }
};

// Actions
const baseUrl = '/api/taskComment';

export function loadComments(taskId) {
    return {
        type: LOAD_TASK_COMMENTS,
        payload: axios.get(`${baseUrl}/${taskId}`, getAuthHeaders())
    };
}

export function addComment(detail) {
    return {
        type: ADD,
        payload: axios.post(`${baseUrl}`, detail, getAuthHeaders())
    };
}

export function deleteComment(commentId) {
    return {
        type: DELETE,
        payload: axios.delete(`${baseUrl}/${commentId}`, getAuthHeaders())
    };
}

export default typeToReducer(reducer, initialState)
