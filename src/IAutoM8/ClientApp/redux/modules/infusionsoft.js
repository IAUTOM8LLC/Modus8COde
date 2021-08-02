import axios from 'axios'
import typeToReducer from 'type-to-reducer'

// Types
const LOAD_DATA = 'iauto/users/LOAD_DATA';

const initialState = {
    loading: false,
    orderFormUrl: "",
    affiliateCookieUrl: ""
};

// Reducer
const reducer = {
    [LOAD_DATA]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => ({
            ...state,
            ...action.payload.data
        })
    }
};

// Actions
const baseUrl = '/api/account';

export function loadData() {
    return {
        type: LOAD_DATA,
        payload: axios.get(`${baseUrl}/infusiondata`)
    }
}

export default typeToReducer(reducer, initialState)
