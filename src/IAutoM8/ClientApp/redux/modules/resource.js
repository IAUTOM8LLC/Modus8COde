import axios from 'axios'
import typeToReducer from 'type-to-reducer'
import update from 'immutability-helper'

import { getAuthHeaders } from '@infrastructure/auth'

// Types
const UPDATE_FILES = 'react/resource/UPDATE_FILES';
const SET_FILES = 'react/resource/SET_FILES';
const RESET_FILES = 'react/resource/RESET_FILES';
const UPLOAD_FILE = 'react/resource/UPLOAD_FILE';
const UPLOAD_FILE_COMPLETE = 'react/resource/UPLOAD_FILE_COMPLETE';
const DELETE_FILE = 'react/resource/DELETE_FILE';
const ADD_VIDEO = 'react/resource/ADD_VIDEO';
const DELETE_VIDEO = 'react/resource/DELETE_VIDEO';
const SHARE_VIDEO = 'react/resource/SHARE_VIDEO';
const SHARE_GLOBAL_VIDEO = 'react/resource/SHARE_GLOBAL_VIDEO';
const SHARE_FILE = 'react/resource/SHARE_FILE';
const SHARE_GLOBAL_FILE = 'react/resource/SHARE_GLOBAL_FILE';
const GET_ALL_PROJECTS = 'react/resource/GET_ALL_PROJECTS';

const initialState = {
    loading: false,
    fileResources: [],
    videoResources: [],
    updatedResources: [],
    deleteResources: [],
    error: null
};

function updateSharedResources(state, resId, resourcesSelector, sharingType) {
    const { updatedResources } = state;
    const resources = state[resourcesSelector];

    const oldResource = resources.find(res => res.localId === resId);
    const newResource = update(oldResource, { [sharingType]: { $apply: shared => !shared } });

    let nextUpdatedResources = updatedResources;
    if (newResource.id !== null) {
        const idx = updatedResources.findIndex(res => res.localId === resId && res.type === oldResource.type)
        if (idx !== -1) {
            nextUpdatedResources = update(updatedResources, { [idx]: { $set: newResource } });
        }
        else {
            nextUpdatedResources = update(updatedResources, { $push: [newResource] })
        }
    }

    return {
        ...state,
        updatedResources: nextUpdatedResources,
        [resourcesSelector]: resources.map(res =>
            res.localId === resId
                ? newResource
                : res
        )
    }
}

// Reducer
const reducer = {
    [RESET_FILES]: () => ({
        ...initialState
    }),

    [UPDATE_FILES]: {
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data
        }),
        FULFILLED: () => ({
            fileResources: [],
            videoResources: [],
            deleteResources: [],
            updatedResources: [],
            loading: false
        })
    },

    [SET_FILES]: (state, action) => ({
        loading: false,
        fileResources: action.payload
            .filter(res => res.type === 0)
            .map((res, index) => ({
                ...res,
                status: 'Loaded',
                localId: index
            })),
        videoResources: action.payload
            .filter(res => res.type === 1)
            .map((res, index) => ({
                ...res,
                localId: index
            })),
        deleteResources: [],
        updatedResources: []
    }),

    [UPLOAD_FILE]: (state, action) => {
        const newResource = {
            localId: action.payload.id,
            id: null,
            name: action.payload.file.name,
            size: action.payload.file.size,
            status: 'Uploading',
            mime: action.payload.file.type,
            type: 0,
            isSharedFromParent: false,
            isGlobalShared: false,
            isShared: false,
            cameFromParent: false
        };

        return {
            ...state,
            fileResources: update(state.fileResources, { $push: [newResource] })
        }
    },

    [DELETE_FILE]: (state, action) => {
        const { fileResources, updatedResources } = state;

        const resource = fileResources.find(res =>
            res.localId === action.payload && res.id !== null);

        const deleteResources = resource
            ? update(state.deleteResources, { $push: [resource.id] })
            : state.deleteResources;

        return {
            ...state,
            deleteResources,
            fileResources: fileResources.filter(res => res.localId !== action.payload),
            updatedResources: updatedResources.filter(res =>
                !(res.localId === action.payload && res.type === 0))
        }
    },

    [ADD_VIDEO]: (state, action) => {
        const localIds = state.videoResources.map(r => r.localId);
        const localId = localIds.length > 0 ? Math.max(...localIds) + 1 : 0;
        const newVideo = {
            localId,
            id: null,
            url: action.payload,
            type: 1,
            isSharedFromParent: false,
            isGlobalShared: false,
            isShared: false,
            cameFromParent: false
        };
        return {
            ...state,
            videoResources: update(state.videoResources, { $push: [newVideo] })
        }
    },

    [DELETE_VIDEO]: (state, action) => {
        const { videoResources, updatedResources } = state;
        const resource = videoResources.find(res => res.localId === action.payload);

        const deleteResources = resource.id !== null
            ? update(state.deleteResources, { $push: [resource.id] })
            : state.deleteResources;

        return {
            ...state,
            deleteResources,
            videoResources: videoResources.filter(res => res.localId !== action.payload),
            updatedResources: updatedResources.filter(res =>
                !(res.localId === action.payload && res.type === 1))
        }
    },

    [UPLOAD_FILE_COMPLETE]: (state, action) => {
        return {
            ...state,
            fileResources: state.fileResources.map(res =>
                res.localId === action.payload.id
                    ? ({
                        ...res,
                        url: action.payload.data.fileInfo.url,
                        timestamp: action.payload.data.fileInfo.timestamp,
                        status: 'Completed'
                    })
                    : res
            )
        }
    },

    [SHARE_VIDEO]: (state, action) =>
        updateSharedResources(state, action.payload, 'videoResources', 'isShared'),

    [SHARE_GLOBAL_VIDEO]: (state, action) =>
        updateSharedResources(state, action.payload, 'videoResources', 'isGlobalShared'),

    [SHARE_FILE]: (state, action) =>
        updateSharedResources(state, action.payload, 'fileResources', 'isShared'),

    [SHARE_GLOBAL_FILE]: (state, action) =>
        updateSharedResources(state, action.payload, 'fileResources', 'isGlobalShared'),
    [GET_ALL_PROJECTS]: {
        PENDING: () => ({ ...initialState, loading: true }),
        FULFILLED: (state, action) => ({
            loading: false,
            fileResources: action.payload.data
                .filter(res => res.type === 0)
                .map((res, index) => ({
                    ...res,
                    status: 'Loaded',
                    localId: index
                })),
            videoResources: action.payload.data
                .filter(res => res.type === 1)
                .map((res, index) => ({
                    ...res,
                    localId: index
                })),
            deleteResources: [],
            updatedResources: []
        })
    }

};

// Actions
const baseUrl = '/api/resource';

export function updateFormulaResource(id, resources) {
    return {
        type: UPDATE_FILES,
        payload: axios.put(`${baseUrl}/formula`,
            getModel(id, resources),
            getAuthHeaders())
    }
}

export function updateFormulaTaskResource(id, resources) {
    console.log('resources', id, resources);
    return {
        type: UPDATE_FILES,
        payload: axios.put(`${baseUrl}/formula-task`,
            getTaskModel(id, resources),
            getAuthHeaders())
    }
}

export function updateProjectResource(id, resources) {
    return {
        type: UPDATE_FILES,
        payload: axios.put(`${baseUrl}/project`,
            getModel(id, resources),
            getAuthHeaders())
    }
}

export function updateProjectTaskResource(id, resources) {
    resources.fileResources.forEach((res, idx) => {
        if (res.originType === undefined || Number(res.originType) !== 0) {
            resources.fileResources[idx]['originType'] = 1;
        }
    });

    return {
        type: UPDATE_FILES,
        payload: axios.put(`${baseUrl}/project-task`,
            getTaskModel(id, resources),
            getAuthHeaders())
    }
}

export function uploadFile(id, file) {
    return {
        type: UPLOAD_FILE,
        payload: { id, file }
    }
}

export function deleteFile(id) {
    return {
        type: DELETE_FILE,
        payload: id
    }
}

export function uploadFileComplete(id, data) {
    return {
        type: UPLOAD_FILE_COMPLETE,
        payload: {
            id, data
        }
    }
}

export function addLink(url) {
    return {
        type: ADD_VIDEO,
        payload: url
    }
}
export function deleteLink(id) {
    return {
        type: DELETE_VIDEO,
        payload: id
    }
}

export function shareVideo(id) {
    return {
        type: SHARE_VIDEO,
        payload: id
    }
}
export function shareGlobalVideo(id) {
    return {
        type: SHARE_GLOBAL_VIDEO,
        payload: id
    }
}

export function shareFile(id) {
    return {
        type: SHARE_FILE,
        payload: id
    }
}
export function shareGlobalFile(id) {
    return {
        type: SHARE_GLOBAL_FILE,
        payload: id
    }
}

export function clearResources() {
    return {
        type: RESET_FILES
    }
}

export function setResources(resources) {
    return {
        type: SET_FILES,
        payload: resources
    }
}

export function getAllResources(projectId) {
    return {
        type: GET_ALL_PROJECTS,
        payload: axios.get(`${baseUrl}/get-project-files/${projectId}`, getAuthHeaders())
    }
}

const getModel = (id, resources) => {
    return {
        id,
        toDeleteList: resources.deleteResources,
        toAddFileList: resources.fileResources
            .filter(file => file.id === null)
            .map((file) => {
                return {
                    name: file.name,
                    mime: file.mime,
                    size: file.size
                }
            }),
        toAddUrlList: resources.videoResources
            .filter(vid => vid.id === null)
            .map(res => res.url)
    }
}

const getTaskModel = (id, resources) => {
    return {
        id,
        toDeleteList: resources.deleteResources,
        toAddFileList: resources.fileResources
            .filter(file => file.id === null)
            .map((file) => {
                return {
                    name: file.name,
                    mime: file.mime,
                    size: file.size,
                    timestamp: file.timestamp || new Date().toUTCString(),
                    originType: file.originType || 0,
                    isGlobalShared: file.isGlobalShared,
                    isShared: file.isShared
                }
            }),
        toAddUrlList: resources.videoResources
            .filter(vid => vid.id === null)
            .map(res => {
                return {
                    url: res.url,
                    isGlobalShared: res.isGlobalShared,
                    isShared: res.isShared
                }
            }),
        updateResourceList: resources.updatedResources
            .map(res => {
                return {
                    id: res.id,
                    isGlobalShared: res.isGlobalShared,
                    isShared: res.isShared
                }
            })
    }
}

export default typeToReducer(reducer, initialState)
