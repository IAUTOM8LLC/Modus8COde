import './polyfills'
import './infrastructure/customImmutabilityHelpers'

import React from 'react'
import ReactDOM from 'react-dom'
import { Provider } from 'react-redux'
import { AppContainer } from 'react-hot-loader'
import { ConnectedRouter } from 'react-router-redux'
import { createBrowserHistory } from 'history'

import Router from './routes'
import configureStore from './redux/configureStore'
import configureAxios from './infrastructure/configureAxios'
import { checkAuth } from './infrastructure/auth'
import { setUser, loadPermissions } from './redux/modules/auth'

const history = createBrowserHistory();
const store = configureStore(window.__data, history);

// ----------------------------------------
// Rendering
// ----------------------------------------

const mountNode = document.createElement('div')
document.body.appendChild(mountNode)

const render = (NewApp) => {
    ReactDOM.render(
        <AppContainer>
            <Provider store={store}>
                <ConnectedRouter history={history}>
                    <NewApp />
                </ConnectedRouter>
            </Provider>
        </AppContainer>, mountNode)
}

// ----------------------------------------
// HMR
// ----------------------------------------

if (__DEV__) {
    /* eslint-disable no-console */
    // When the application source code changes, re-render the whole thing.
    if (module.hot) {
        module.hot.accept('./routes', () => {
            // restore scroll
            const { scrollLeft, scrollTop } = document.scrollingElement
            ReactDOM.unmountComponentAtNode(mountNode)

            try {
                render(require('./routes').default)
                document.scrollingElement.scrollTop = scrollTop
                document.scrollingElement.scrollLeft = scrollLeft
            } catch (e) {
                console.error(e)
            }
        })
    }
    /* eslint-enable no-console */
}

// ----------------------------------------
// Start the app
// ----------------------------------------

configureAxios(store);

checkAuth()
    .then(user => {
        if (user) {
            store.dispatch(setUser(user));
        }
        return user;
    })
    .then(user => user && store.dispatch(loadPermissions()))
    .then(() => render(Router));
