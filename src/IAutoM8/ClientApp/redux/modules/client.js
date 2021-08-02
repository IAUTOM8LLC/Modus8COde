import typeToReducer from 'type-to-reducer'

import createCrudReducer from './abstract'

const initialState = {
    loading: false,
    clients: []
};

const {
    reducer: crudReducer,
    actions: { load, add, edit, remove }
} = createCrudReducer('client', '/api/client')

const reducer = typeToReducer(crudReducer, initialState)

const canLoadClients = store => store.auth.permissions.readClient;
const loadClients = () => (dispatch, getStore) => canLoadClients(getStore()) && dispatch(load())

export {
    reducer as default,

    loadClients,
    add as addClient,
    edit as editClient,
    remove as deleteClient,
}