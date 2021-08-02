import axios from 'axios'
import typeToReducer from 'type-to-reducer'

import { getAuthHeaders } from '@infrastructure/auth'
import { Promise, Function } from 'core-js';

// Types
const NEW_UNREAD_MESSAGE = 'react/notifications/NEW_UNREAD_MESSAGE';
const READ_MESSAGE = 'react/notifications/READ_MESSAGE';
const READ_ALL_MESSAGE = 'react/notifications/READ_ALL_MESSAGE';
const DELETE_MESSAGE = 'react/notifications/DELETE_MESSAGE';
const GET_UNREAD_MESSAGE = 'react/notifications/GET_UNREAD_MESSAGE';
const GET_MESSAGES = 'react/notifications/GET_MESSAGES';
const NOTIFICATION_SEARCH = 'react/notifications/NOTIFICATION_SEARCH';
const NOTIFICATION_PAGE = 'react/notifications/NOTIFICATION_PAGE';
const NOTIFICATION_NEXT_PAGE = 'react/notifications/NOTIFICATION_NEXT_PAGE';
const SEND_NUDGE_NOTIFICATION = 'react/notifications/SEND_NUDGE_NOTIFICATION';
const READ_ALL_COMMENTS = 'react/notifications/READ_ALL_COMMENTS';

const initialState = {
    loading: false,
    pageCount: 10,
    messages: [],
    unreadMessageCount: 0,
    totalMessageCount: 0,
    searchModel: {
        perPage: 15,
        page: 1,
        filterSearch: ''
    },
    infinityMessages: [],
    canNextPage: false,
    notificationStatus: {},
    showCancelButton: false
};

// Reducer
const reducer = {
    [NEW_UNREAD_MESSAGE]: {
        FULFILLED: (state, action) => {
            const { messages, unreadMessageCount,
                totalMessageCount,
                infinityMessages } = state;
            return ({
                ...state,
                loading: false,
                messages: [action.payload, ...messages],
                infinityMessages: [action.payload, ...infinityMessages],
                unreadMessageCount: unreadMessageCount + 1,
                totalMessageCount: totalMessageCount + 1
            });
        }
    },
    [DELETE_MESSAGE]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => {
            const { messages, unreadMessageCount } = state;
            const message = messages.find(message => action.payload[0] === message.id);
            return ({
                ...state,
                loading: false,
                messages: messages.filter(message => action.payload[0] !== message.id),
                unreadMessageCount: unreadMessageCount -
                    (message && !message.isRead ? 1 : 0)
            });
        }
    },
    [READ_MESSAGE]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        FULFILLED: (state, action) => {
            const { messages, unreadMessageCount } = state;
            return ({
                ...state,
                loading: false,
                messages: messages
                    .map(message => ({
                        ...message,
                        isRead: action.payload[0] === message.id
                            ? true : message.isRead
                    }))
                    .sort(sortMessage),
                unreadMessageCount: unreadMessageCount - 1
            });
        }
    },
    [READ_ALL_MESSAGE]: {
        FULFILLED: (state, action) => {
            const { messages } = state;
            return ({
                ...state,
                loading: false,
                messages: messages
                    .map(message => ({
                        ...message,
                        isRead: true
                    }))
                    .sort(sortMessage),
                unreadMessageCount: 0
            });
        }
    },
    [GET_UNREAD_MESSAGE]: {
        FULFILLED: (state, action) => {
            return ({
                ...state,
                loading: false,
                messages: action.payload.data.messages,
                unreadMessageCount: action.payload.data.unreadCount,
                totalMessageCount: action.payload.data.totalCount
            });
        }
    },
    [GET_MESSAGES]: {
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
            const [requestResponse, page] = action.payload;
            const { data } = requestResponse;
            return ({
                ...state,
                loading: false,
                infinityMessages: page === 1
                    ? data.messages
                    : state.infinityMessages.concat(data.messages),
                totalMessageCount: data.totalCount,
                canNextPage: data.messages.length ===
                    state.searchModel.perPage
            })
        }
    },

    [NOTIFICATION_SEARCH]: {
        FULFILLED: (state, action) => ({
            ...state,
            formulas: [],
            canNextPage: false,
            searchModel: {
                ...state.searchModel,
                filterSearch: action.payload,
                page: 1
            },
            selectedFormulas: []
        })
    },

    [NOTIFICATION_PAGE]: {
        FULFILLED: (state, action) => ({
            ...state,
            searchModel: {
                ...state.searchModel,
                page: action.payload
            }
        })
    },

    [NOTIFICATION_NEXT_PAGE]: {
        FULFILLED: (state, action) => ({
            ...state,
            searchModel: {
                ...state.searchModel,
                page: state.searchModel.page + 1
            },
            canNextPage: false
        })
    },
    [SEND_NUDGE_NOTIFICATION]: {
        FULFILLED: (state, action) => {            
           return {
            ...state,
        notificationStatus: action.payload.data
    }
        }
    },
    [READ_ALL_COMMENTS]: {
        FULFILLED: (state) => {
            return ({
                ...state,
                loading: false
            });
        }
    }
};

function sortMessage(first, second) {
    if ((!first.isRead && !second.isRead) ||
        (first.isRead && second.isRead)) {
        return second.id - first.id;
    }
    else if (!first.isRead) {
        return -1;
    }
    return 1;
}
// Actions
const baseUrl = '/api/notifications';

export function loadNotifications(isInitialLoad) {
    return (dispatch, getStore) => {
        const {
            page,
            perPage,
            filterSearch
        } = getStore().notification.searchModel;
        if (isInitialLoad && page !== 1)
            return;
        dispatch({
            type: GET_MESSAGES,
            payload: Promise.all([
                axios.get(`${baseUrl}`, {
                    params: {
                        page,
                        perPage,
                        filterSearch
                    },
                    ...getAuthHeaders()
                }),
                Promise.resolve(page)
            ])
        })

    }
}

export function newMessage(message) {
    return {
        type: NEW_UNREAD_MESSAGE,
        payload: {
            promise: Promise.resolve(message)
        }
    }
}

export function readMessage(id) {
    return {
        type: READ_MESSAGE,
        payload: Promise.all([Promise.resolve(id), axios.post(`${baseUrl}`, id, getAuthHeaders())])
    }
}

export function readAllMessage() {
    return {
        type: READ_ALL_MESSAGE,
        payload: axios.post(`${baseUrl}/read-all`, null, getAuthHeaders())
    }
}

export function deleteMessage(id) {
    return {
        type: DELETE_MESSAGE,
        payload: Promise.all([Promise.resolve(id), axios.delete(`${baseUrl}/${id}`, getAuthHeaders())])
    }
}

export function getAllUnread() {
    return {
        type: GET_UNREAD_MESSAGE,
        payload: axios.get(`${baseUrl}/get-all-unread`, getAuthHeaders())
    }

}

export function notificationSearch(event, { value }) {
    return {
        type: NOTIFICATION_SEARCH,
        payload: {
            promise: Promise.resolve(value)
        }
    }
}

export function notificationPage(value) {
    return {
        type: NOTIFICATION_PAGE,
        payload: {
            promise: Promise.resolve(value)
        }
    }
}

export function nextPage() {
    return {
        type: NOTIFICATION_NEXT_PAGE,
        payload: {
            promise: Promise.resolve()
        }
    }
}

export function sendNotificationMessage(projectTaskId) {
    return {
        type: SEND_NUDGE_NOTIFICATION,
        payload: axios.get(`${baseUrl}/send-nudge/${projectTaskId}`, getAuthHeaders())
    }
}

export function readComments(taskId) {
    return {
        type: READ_ALL_COMMENTS,
        payload: axios.put(`${baseUrl}/read-comments/${taskId}`, null, getAuthHeaders())
    }
}

export default typeToReducer(reducer, initialState)
