import axios from "axios";
import typeToReducer from "type-to-reducer";

import * as jwtStore from "@infrastructure/jwtLocalStore";
import { getJsonHeaders, getAuthHeaders } from "@infrastructure/auth";

// Types
const REGISTER = "iauto/auth/REGISTER";
const LOGIN = "iauto/auth/LOGIN";
const LOGOFF = "iauto/auth/LOGOFF";
const LOGINSTATE_RESET = "iauto/account/LOGINSTATE_RESET";
const FORGOTPASSWORD = "iauto/account/FORGOTPASSWORD";
const FORGOTPASSWORDCHANGE = "iauto/account/FORGOTPASSWORDCHANGE";
const PUSHURLPARAMS = "iauto/account/PUSHURLPARAMS";
const SET_LOGGED_USER = "iauto/account/SET_LOGGED_USER";
const LOAD_PERMISSIONS = "iauto/account/LOAD_PERMISSIONS";
const SAVE_INFUSION_URL = "iauto/account/SAVE_INFUSION_URL";
const LOGIN_AS_USER = "iauto/auth/LOGIN_AS_USER";

const emptyUser = {
    name: "",
    fullName: "",
    roles: [],
    error: "",
    id: "",
};

const initialState = {
    loading: false,
    loggedIn: false,
    signedUp: false,
    error: false,
    user: emptyUser,
    permissions: {},
    urlParams: { email: "", code: "" },
};

// Reducer
const reducer = {
    [SET_LOGGED_USER]: (state, action) => ({
        ...state,
        loggedIn: true,
        user: action.payload,
    }),

    [LOGIN]: {
        PENDING: (state) => ({
            ...state,
            loading: true,
            error: false,
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data,
        }),
        FULFILLED: (state, action) => {
            const response = action.payload.data;
            jwtStore.setLocalToken(response.access_token);
            return {
                ...state,
                loading: false,
                loggedIn: true,
                error: false,
                user: jwtStore.decodeUserFromToken(response.access_token),
            };
        },
    },

    [REGISTER]: {
        PENDING: (state) => ({
            ...state,
            loading: true,
            error: false,
            signedUp: false,
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data,
        }),
        FULFILLED: (state, action) => {
            jwtStore.setLocalToken(action.payload.data.access_token);
            return {
                ...state,
                error: false,
                loading: false,
                signedUp: true,
            };
        },
    },

    [LOGOFF]: (state) => ({
        ...state,
        loggedIn: false,
        user: emptyUser,
        permissions: {},
    }),

    [LOGINSTATE_RESET]: () => initialState,

    [FORGOTPASSWORD]: {
        PENDING: (state) => ({
            ...state,
            loading: true,
            error: false,
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data,
        }),
        FULFILLED: (state) => ({
            ...state,
            loading: false,
            error: false,
        }),
    },

    [FORGOTPASSWORDCHANGE]: {
        PENDING: (state) => ({
            ...state,
            loading: true,
            error: false,
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data,
        }),
        FULFILLED: (state, action) => {
            const token = action.payload.data.access_token;
            jwtStore.setLocalToken(token);

            return {
                ...state,
                loading: false,
                loggedIn: true,
                user: jwtStore.decodeUserFromToken(token),
                error: false,
            };
        },
    },

    [LOAD_PERMISSIONS]: {
        PENDING: (state) => ({
            ...state,
            loading: true,
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data,
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            permissions: action.payload.data,
        }),
    },

    [PUSHURLPARAMS]: (state, action) => ({
        ...state,
        urlParams: { ...action.payload },
    }),

    [LOGIN_AS_USER]: {
        PENDING: (state) => ({
            ...state,
            loading: true,
            error: false,
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data,
        }),
        FULFILLED: (state, action) => {
            const response = action.payload.data;
            jwtStore.setLocalToken(response.access_token);
            return {
                ...state,
                loading: false,
                loggedIn: true,
                error: false,
                user: jwtStore.decodeUserFromToken(response.access_token),
            };
        },
    },
};

// Actions
export function login(user) {
    return {
        type: LOGIN,
        payload: axios.post("/api/auth/signin", user, getJsonHeaders()),
    };
}

export function updateProfile(profile) {
    return {
        type: LOGIN,
        payload: Promise.resolve(profile),
    };
}

export function logoff() {
    jwtStore.resetLocalToken();
    return { type: LOGOFF };
}

export function resetLoginState() {
    jwtStore.resetLocalToken();
    return { type: LOGINSTATE_RESET };
}

export function register(user) {
    return {
        type: REGISTER,
        payload: axios.post("/api/auth/signup", user, getJsonHeaders()),
    };
}

export function saveInfusionUrl(infusionData) {
    return {
        type: SAVE_INFUSION_URL,
        payload: axios.post(
            "/api/auth/saveInfusionUrl",
            infusionData,
            getJsonHeaders()
        ),
    };
}

export function registerVendor(user) {
    return {
        type: REGISTER,
        payload: axios.post("/api/auth/vendor-signup", user, getJsonHeaders()),
    };
}

export function registerCVendor(user) {
    return {
        type: REGISTER,
        payload: axios.post("/api/auth/cvendor-signup", user, getJsonHeaders()),
    };
}

export function forgotPassword(user) {
    jwtStore.resetLocalToken();
    return {
        type: FORGOTPASSWORD,
        payload: axios.put(
            "/api/account/forgotpassword",
            user,
            getJsonHeaders()
        ),
    };
}

export function forgotPasswordChange(user) {
    return {
        type: FORGOTPASSWORDCHANGE,
        payload: axios.post(
            "/api/account/restorepassword",
            user,
            getJsonHeaders()
        ),
    };
}

export function confirmEmail(user) {
    return {
        type: LOGIN,
        payload: axios.post(
            "/api/account/confirmemail",
            user,
            getJsonHeaders()
        ),
    };
}

export function setUser(user) {
    return {
        type: SET_LOGGED_USER,
        payload: user,
    };
}

export function loadPermissions() {
    return {
        type: LOAD_PERMISSIONS,
        payload: axios.get("/api/auth/permissions", getAuthHeaders()),
    };
}

export function pushUrlParams(params) {
    return { type: PUSHURLPARAMS, payload: params };
}

export function adminLoginAsUser(user) {
    return {
        type: LOGIN_AS_USER,
        payload: axios.post(
            "/api/SuperAdmin/signin-user",
            user,
            getAuthHeaders()
        ),
    };
}

export default typeToReducer(reducer, initialState);
