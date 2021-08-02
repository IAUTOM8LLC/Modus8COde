import { combineReducers } from 'redux'

import * as reducers from './modules'

const appReducer = combineReducers(reducers);
const initialState = appReducer({}, {});

const rootReducer = (state, action) => {
    if (action.type === 'iauto/auth/LOGOFF') {
        state = initialState
    }

    return appReducer(state, action)
}

export default rootReducer;