import axios from 'axios'
import typeToReducer from 'type-to-reducer'

import { getAuthHeaders } from '@infrastructure/auth'

// Types
const LOAD_TOKEN = 'iauto/credits/LOAD_TOKEN';
const BUY_CREDITS = 'iauto/credits/BUY_CREDITS';
const TRANSFER_REQUEST = 'iauto/credits/TRANSFER_REQUEST';
const ACCEPT_TRANSFER_REQUEST = 'iauto/credits/ACCEPT_TRANSFER_REQUEST';
const LOAD_OWNER_CREDITS = 'iauto/credits/BUY_OWNER_CREDITS';
const LOAD_BALANCE = 'iauto/credits/LOAD_BALANCE';
const LOAD_VENDOR_TAX = 'iauto/credits/LOAD_VENDOR_TAX';
const UPDATE_AVAILABLE_CREDITS = 'iauto/credits/UPDATE_AVAILABLE_CREDITS';

const initialState = {
    loading: false,
    token: null,
    credits: null,
    balance: null,
    vendorTax: null,
    activeTransferRequest: null
};

// Reducer
const reducer = {
    [LOAD_TOKEN]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            token: action.payload.data
        })
    },
    [BUY_CREDITS]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            credits: action.payload.data
        }),
        REJECTED: (state) => ({ ...state, loading: false })
    },
    [LOAD_OWNER_CREDITS]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            credits: action.payload.data
        })
    },
    [LOAD_BALANCE]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            balance: action.payload.data
        })
    },
    [LOAD_VENDOR_TAX]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            vendorTax: action.payload.data
        })
    },
    [TRANSFER_REQUEST]: {
        PENDING: (state) => ({ ...state, loading: true }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            activeTransferRequest: action.payload.data
        })
    },
    [UPDATE_AVAILABLE_CREDITS]: {
        FULFILLED: (state, action) => {
            state.credits.availableCredits += action.payload.updatedPrice;
            state.credits.reservedCredits += -action.payload.updatedPrice;
            return ({ ...state });
        }
    },
    [ACCEPT_TRANSFER_REQUEST]: {
        FULFILLED: (state) => state
    },
}

// Actions
const baseUrl = '/api/credits';

export function loadToken() {
    return {
        type: LOAD_TOKEN,
        payload: axios.get(`${baseUrl}/token`, getAuthHeaders())
    }
}

export function loadCredits() {
    return {
        type: LOAD_OWNER_CREDITS,
        payload: axios.get(`${baseUrl}/load-credits`, getAuthHeaders())
    }
}

export function loadBalance() {
    return {
        type: LOAD_BALANCE,
        payload: axios.get(`${baseUrl}/load-balance`, getAuthHeaders())
    }
}

export function loadVendorTax() {
    return {
        type: LOAD_VENDOR_TAX,
        payload: axios.get(`${baseUrl}/vendor-tax`, getAuthHeaders())
    }
}

export function buyCredits(model) {
    return {
        type: BUY_CREDITS,
        payload: axios.post(`${baseUrl}/buy-credits`, model, getAuthHeaders())
    }
}

export function transferRequest() {
    return {
        type: TRANSFER_REQUEST,
        payload: axios.get(`${baseUrl}/transfer-request`, getAuthHeaders())
    }
}

export function loadActiveTransferRequest() {
    return {
        type: TRANSFER_REQUEST,
        payload: axios.get(`${baseUrl}/load-active-transfer`, getAuthHeaders())
    }
}

export function acceptTransferRequest(transferRequestId) {
    return {
        type: ACCEPT_TRANSFER_REQUEST,
        payload: axios.get(`${baseUrl}/accept-transfer-request/${transferRequestId}`, getAuthHeaders())
    }
}

export default typeToReducer(reducer, initialState)
