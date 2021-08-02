import axios from 'axios'
import update from 'immutability-helper'

import { getAuthHeaders } from '@infrastructure/auth'

export default function (entityName, baseUrl) {

    const types = buildActionTypes(entityName);
    const actions = buildActionCreators(types, baseUrl);
    const reducer = buildReducer(types, entityName)

    return {
        reducer,
        actions,
        types
    }
}

function buildActionTypes(entityName) {
    const prefix = entityName.toUpperCase();
    const LOAD = `modus/${prefix}/LOAD_${prefix}S`;
    const ADD = `modus/${prefix}/ADD_${prefix}`;
    const EDIT = `modus/${prefix}/EDIT_${prefix}`;
    const DELETE = `modus/${prefix}/DELETE_${prefix}`;

    return { LOAD, ADD, EDIT, DELETE };
}

function buildReducer(types, entityName) {

    const entitySelectorName = `${entityName}s`;
    const selectItems = state => state[entitySelectorName];

    return {
        [types.LOAD]: {
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
                [entitySelectorName]: action.payload.data
            })
        },

        [types.ADD]: {
            FULFILLED: (state, action) => ({
                ...state,
                [entitySelectorName]: update(selectItems(state), { $push: [action.payload.data] })
            })
        },

        [types.EDIT]: {
            PENDING: (state) => ({
                ...state,
                loading: true
            }),
            REJECTED: (state, action) => ({
                ...state,
                loading: false,
                error: action.payload.data
            }),
            FULFILLED: (state, action) => {
                const items = selectItems(state);
                const updated = action.payload.data;
                const updatedIndex = items.findIndex(t => t.id === updated.id);

                return ({
                    ...state,
                    loading: false,
                    [entitySelectorName]: update(items, { [updatedIndex]: { $set: updated } })
                });
            }
        },

        [types.DELETE]: {
            PENDING: (state, action) => ({
                ...state,
                [entitySelectorName]: selectItems(state).filter(p => p.id !== action.payload)
            })
        }
    };
}

function buildActionCreators(types, baseUrl) {
    const load = () => ({
        type: types.LOAD,
        payload: axios.get(baseUrl, getAuthHeaders())
    });

    const add = item => ({
        type: types.ADD,
        payload: axios.post(baseUrl, item, getAuthHeaders())
    });

    const edit = item => ({
        type: types.EDIT,
        payload: axios.put(baseUrl, item, getAuthHeaders())
    });

    const remove = id => ({
        type: types.DELETE,
        payload: {
            data: id,
            promise: axios.delete(`${baseUrl}/${id}`, getAuthHeaders())
        }
    });

    return { load, add, edit, remove };
}
