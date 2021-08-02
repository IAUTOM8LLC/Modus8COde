import axios from 'axios'
import moment from 'moment'
import { push } from 'react-router-redux'

import { logoff } from '@store/auth'

import { refreshToken, shouldRefreshToken } from './auth'
import { getUTCFromCustomFormat, formatUTCTime } from './dateTimeManagment'


/* eslint-disable no-console */
export default function (store) {
    axios.defaults.transformRequest = [].concat(transformRequest, axios.defaults.transformRequest)
    axios.defaults.transformResponse = [].concat(axios.defaults.transformResponse, transformResponse)

    axios.interceptors.response.use(response => {
        const hasAccess = ensureHasAccess(response, store.dispatch)

        if (hasAccess)
            return response;

    }, error => {
        const hasAccess = ensureHasAccess(error.response, store.dispatch);

        if (__DEV__) {
            console.log('====================================');
            console.log('Error response', error.response);
            console.log(error);
            console.log('====================================');
        }

        if (hasAccess && !__DEV__) {
            // Not auth problem, something bad happened. Log error 
            console.log('Error response', error.response)
            console.error(error);
        }

        return Promise.reject(error.response
            ? error.response
            : error
        );
    });

    axios.interceptors.request.use(
        (config) => {
            tryRefreshJwtToken(store);
            return config;
        },
        (err) => Promise.reject(err)
    );
}

function ensureHasAccess(response, dispatch) {
    if (!response)
        return true;

    const { status = 200 } = response;

    if (status === 401) {
        dispatch(logoff());
        dispatch(push('/login'));
        return false;

    } else if (status === 403) {
        dispatch(push('/'));
        return false;
    }

    return true;
}

function transformRequest(data) {
    transform(data, property => {
        const customFormat = moment(property, "MMM D, YYYY h:mm A", true);
        if (customFormat.isValid()) {
            return getUTCFromCustomFormat(customFormat);
        }

        return property;
    })

    return data;
}

function transformResponse(data) {
    transform(data, property => {
        if (moment(property, [
            "YYYY-MM-DDTHH:mm:ssZ",
            "YYYY-MM-DDTHH:mm:ss",
            "YYYY-MM-DDTHH:mm:ss.sssssss",
            "YYYY-MM-DDTHH:mm:ss.sssssssZ"], true).isValid()
        ) {
            return formatUTCTime(property);
        }
        return property;
    })
    return data;
}

function transform(obj, transformer) {
    if (typeof obj === "object") {
        for (const property in obj) {
            if (obj.hasOwnProperty(property)) {
                if (typeof obj[property] === "object") {
                    transform(obj[property], transformer);
                } else {
                    obj[property] = transformer(obj[property])
                }
            }
        }
    }
}

let pendingRefresh = false;
function tryRefreshJwtToken(store) {
    if (!pendingRefresh && store.getState().auth.loggedIn && shouldRefreshToken()) {
        pendingRefresh = true;
        refreshToken().finally(() => { pendingRefresh = false; });
    }
}