import axios from 'axios'
import typeToReducer from 'type-to-reducer'

import { getAuthHeaders } from '@infrastructure/auth'

// Types
const LOAD_PROFILE = 'react/profile/LOAD_PROFILE';
const UPDATE_PROFILE = 'react/profile/UPDATE_PROFILE';
const SET_PROFILE_IMAGE = 'react/profile/SET_PROFILE_IMAGE';

const initialState = {
    loading: false,
    profile: {
        userProfile: {
            name: '',
            gender: -1
        },
    },
};

// Reducer
const reducer = {
    [LOAD_PROFILE]: {
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
            profile: action.payload.data
        })
    },

    [LOAD_PROFILE]: {
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
            profile: action.payload.data
        })
    },

    [UPDATE_PROFILE]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data
        }),
        FULFILLED: (state) => ({
            ...state,
            loading: false
        })
    },

    [SET_PROFILE_IMAGE]: {
        FULFILLED: (state, action) => ({
            ...state,
            profile: {
                ...state.profile,
                userProfile: {
                    ...state.profile.userProfile,
                    profileImage: action.payload
                }
            }
        })
    },
};

// Actions
const baseUrl = '/api/account/profile';

export function loadProfile() {
    return {
        type: LOAD_PROFILE,
        payload: axios.get(baseUrl, getAuthHeaders())
    }
}

export function loadProfileById(userId) {
    return {
        type: LOAD_PROFILE,
        payload: axios.get(`${baseUrl}/${userId}`, getAuthHeaders())
    }
}

export function updateProfile(profile) {
    return {
        type: UPDATE_PROFILE,
        payload: axios.put(baseUrl, profile, getAuthHeaders())
    };
}

export function setProfileImage(value) {
    return {
        type: SET_PROFILE_IMAGE,
        payload: {
            promise: Promise.resolve(value)
        }
    };
}

export default typeToReducer(reducer, initialState)
