import { createStore, applyMiddleware, compose } from 'redux'
import { routerMiddleware } from 'react-router-redux'
import promiseMiddleware from 'redux-promise-middleware'
import thunk from 'redux-thunk'

import reducer from './reducer'

let devTools = f => f;
if (typeof window === 'object'
    && typeof window.devToolsExtension !== 'undefined') {
    devTools = window.devToolsExtension();
}

export default function configureStore(initialState, history) {

    const enhancer = compose(
        applyMiddleware(thunk),
        applyMiddleware(promiseMiddleware()),
        applyMiddleware(routerMiddleware(history)),
        devTools
    )(createStore);

    const store = enhancer(reducer, initialState);

    if (module.hot) {
        module.hot.accept('./reducer', () => {
            store.replaceReducer(require('./reducer').default);
        });
    }

    return store;
}
