import * as store from 'store2'
import Cookies from 'js-cookie'
import JwtDecode from 'jwt-decode'

export const JWT_TOKEN = 'JWT_TOKEN';

export function getLocalToken() {
    const token = store.get(JWT_TOKEN) || Cookies.get(JWT_TOKEN);
    return token;
}

export function resetLocalToken() {
    store.remove(JWT_TOKEN);
    Cookies.remove(JWT_TOKEN);
}

export function setLocalToken(token) {
    if (!token)
        return;

    store.set(JWT_TOKEN, token);
    Cookies.set(JWT_TOKEN, token, { expires: 365 });
}


export function decodeUserFromToken(token) {
    try {
        const payload = JwtDecode(token);

        const res = {};
        Object.keys(payload).forEach((key) => {
            res[key.replace(/http:\/\/schemas(.*)\/claims\//g, '')] = payload[key];
        });
        return {
            name: res.name,
            fullName: res.fullName,
            roles: Array.isArray(res.role) ? res.role : [res.role],
            id: res.sid
        };

    } catch (err) {
        return {
            name: '',
            fullName: '',
            roles: [],
            id: ''
        };
    }
}
