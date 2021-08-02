/**
 * App-wide parameters are stored here
 */
import { LOCATION_CHANGE } from 'react-router-redux'
import typeToReducer from 'type-to-reducer'

// Types
const SET_SEARCH_QUERY = 'iauto/layout/SET_SEARCH_QUERY';
const UI_EDITOR_FORCE_REDRAW = 'iauto/layout/UI_EDITOR_FORCE_REDRAW';
const SET_FILTER_COLUMN = 'iauto/layout/SET_FILTER_COLUMN';

const initialState = {
    searchQuery: '',
    filterBy: null,
    forceRedrawUIEditor: false
};

// Reducer 
const reducer = {
    [SET_SEARCH_QUERY]: (state, action) => ({
        ...state,
        searchQuery: action.payload
    }),

    [SET_FILTER_COLUMN]: (state, action) => ({
        ...state,
        filterBy: action.payload
    }),

    [LOCATION_CHANGE]: (state) => ({
        ...state,
        searchQuery: ''
    }),

    [UI_EDITOR_FORCE_REDRAW]: {
        FULFILLED: (state, action) => ({
            ...state,
            forceRedrawUIEditor: action.payload
        })
    }
};


// Actions 
export const setSearchQuery = (query) => ({
    type: SET_SEARCH_QUERY,
    payload: query
});

export const requestRedrawUIEditor = () =>
    (dispatch) =>
        dispatch({
            type: UI_EDITOR_FORCE_REDRAW,
            payload: Promise.resolve(true)
        }).then(() => dispatch({
            type: UI_EDITOR_FORCE_REDRAW,
            payload: Promise.resolve(false)
        }));

export const setFilterColumn = (filterBy) => ({
    type: SET_FILTER_COLUMN,
    payload: filterBy
});

export default typeToReducer(reducer, initialState)
