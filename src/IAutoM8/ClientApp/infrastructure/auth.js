import axios from 'axios'
import JwtDecode from 'jwt-decode'
import moment from 'moment'

import { getLocalToken, setLocalToken, decodeUserFromToken } from './jwtLocalStore'

export function getAuthHeaders() {
    return {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json;charset=utf-8',
            'Authorization': `Bearer ${getLocalToken()}`
        }
    }
}

export function getJsonHeaders() {
    return {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json;charset=utf-8'
        }
    }
}

export function refreshToken() {
    return axios.get('/api/auth/refresh-token', getAuthHeaders())
        .then(response => {
            setLocalToken(response.data.access_token);
            return Promise.resolve();
        });
}

export function checkAuth() {
    return new Promise((resolve) => {
        if (isLoggedIn()) {
            resolve(decodeUserFromToken(getLocalToken()))
        } else {
            resolve(null);
        }
    });
}

/**
 * Returns true if jwt token is older then half of its lifetime.
 */
export function shouldRefreshToken() {
    try {
        const token = JwtDecode(getLocalToken());
        const lifetime = token.exp - token.iat;
        const age = moment().unix() - token.iat;
        return age > lifetime / 2;
    } catch (err) {
        return false;
    }
}

export function isLoggedIn() {
    try {
        const decoded = JwtDecode(getLocalToken());
        const tokenExpired = moment.unix(decoded.exp).isBefore(moment());
        return !tokenExpired;
    } catch (err) {
        return false;
    }
}
