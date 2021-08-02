import typeToReducer from 'type-to-reducer'
import axios from 'axios'

import { getAuthHeaders } from '@infrastructure/auth'
import { mapStatus } from '@utils/task'

const LOAD_VENDOR_UPSKILLS = 'react/vendor/LOAD_VENDOR_UPSKILLS';
const LOAD_VENDOR_NOTIFICATION = 'react/vendor/LOAD_VENDOR_NOTIFICATION';
const LOAD_FORMULA_FOR_CERTIFICATION = 'react/vendor/LOAD_FORMULA_FOR_CERTIFICATION';
const LOAD_PERFORMANCE_DATA = 'react/vendor/LOAD_PERFORMANCE_DATA';
const LOAD_COMPANY_PERFORMANCE_DATA = 'react/vendor/LOAD_COMPANY_PERFORMANCE_DATA';
const LOAD_COMPANY_USER_DATA = 'react/vendor/LOAD_COMPANY_USER_DATA';
const ANSWER_FOR_VENDOR_NOTIFICATION = 'react/vendor/ANSWER_FOR_VENDOR_NOTIFICATION';
const ANSWER_FOR_VENDOR_CERTIFICATION = 'react/vendor/ANSWER_FOR_VENDOR_CERTIFICATION';
const SET_PENDING_VENDOR_NOTIFICATION = 'react/vendor/SET_PENDING_VENDOR_NOTIFICATION';
const DELETE_VENDOR_TASK_PRICE = 'react/vendor/DELETE_VENDOR_TASK_PRICE';
const DELETE_COMPANY_USER_TASKS = 'react/vendor/DELETE_COMPANY_USER_TASKS';
const DELETE_COMPANY_PERFORMANCE_TASKS = 'react/vendor/DELETE_COMPANY_PERFORMANCE_TASKS';
const UPDATE_VENDOR_TASK_PRICE = 'react/vendor/UPDATE_VENDOR_TASK_PRICE';
const UPDATE_COMPANY_TASK_PRICE = 'react/vendor/UPDATE_COMPANY_TASK_PRICE';
const LOAD_VENDOR_SNAPSHOT = 'react/vendor/LOAD_VENDOR_SNAPSHOT';
const GET_NEW_TASK_DATA = 'react/vendor/GET_NEW_TASK_DATA';
const SYNC_VENDOR_DATA = 'react/vendor/SYNC_VENDOR_DATA';
const COMPANY_WORKER_EMAIL = 'react/vendor/COMPANY_WORKER_EMAIL';
const LOAD_VENDORS_BY_OPTION_TYPE = 'react/vendor/LOAD_VENDORS_BY_OPTION_TYPE';
const LOAD_VENDOR_JOBINVITES = 'react/vendor/LOAD_VENDOR_JOBINVITES';
const LOAD_VENDOR_FORMULA_BIDS = 'react/vendor/LOAD_VENDOR_FORMULA_BIDS';
const LOAD_COMPANY_FORMULA_BIDS = 'react/vendor/LOAD_COMPANY_FORMULA_BIDS';
const REMOVE_INVITE = 'react/vendor/REMOVE_INVITE';
const REMOVE_BID = 'react/vendor/REMOVE_BID';
const LOAD_COMPANY_USER_DETAILS = 'react/vendor/LOAD_COMPANY_USER_DETAILS';
const USER_SELECT = 'react/vendor/USER_SELECT';


const initialState = {
    loading: false,
    vendorNotification: {},
    performanceData: [],
    companyPerformanceData: [],
    companyUserData: [],
    vendorUpSkills: [],
    vendorSnapshots: {},
    vendorJobInvites: [],
    selectedVendors: [],
    vendorFormulaBids: [],
    companyUserDetails: [],
    selectedUsers: {},

};

const reducer = {
    [LOAD_VENDOR_NOTIFICATION]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state, action) => {
            return ({
                ...state,
                loading: false,
                vendorNotification: action.payload.data,
            })
        }
    },

    [LOAD_PERFORMANCE_DATA]: {
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
            performanceData: action.payload.data
        })
    },
    [LOAD_COMPANY_PERFORMANCE_DATA]: {
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
            return {
                ...state,
                loading: false,
                companyPerformanceData: action.payload.data
            }
        }
    },
    [LOAD_COMPANY_USER_DATA]: {
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
            companyUserData: action.payload.data
        })
    },
    [LOAD_COMPANY_USER_DETAILS]: {
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
            companyUserDetails: action.payload.data
        })
    },
    [USER_SELECT]: {
        FULFILLED: (state, action) => {
            const selectedUsers = state.selectedUsers
            selectedUsers[action.payload] = selectedUsers[action.payload] ? false : true;
            return ({
                ...state,
                selectedUsers: selectedUsers
            })
        }
    },
    [LOAD_VENDOR_UPSKILLS]: {
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
            vendorUpSkills: action.payload.data
        })
    },

    [LOAD_FORMULA_FOR_CERTIFICATION]: {
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
            vendorNotification: action.payload.data
        })
    },

    [ANSWER_FOR_VENDOR_NOTIFICATION]: {
        FULFILLED: (state) => state
    },

    [ANSWER_FOR_VENDOR_CERTIFICATION]: {
        FULFILLED: (state) => state
    },

    [SET_PENDING_VENDOR_NOTIFICATION]: (state, action) => ({
        ...state, pendingTask: mapStatus(action.payload),
    }),

    [UPDATE_VENDOR_TASK_PRICE]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data
        }),
        FULFILLED: (state) => ({
            ...state,
            loading: false
        })
    },
    [UPDATE_COMPANY_TASK_PRICE]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data
        }),
        FULFILLED: (state) => {
            return {

                ...state,
                loading: false
            }

        }
    },

    [DELETE_VENDOR_TASK_PRICE]: {
        PENDING: (state) => ({ ...state })
    },
    [DELETE_COMPANY_USER_TASKS]: {
        PENDING: (state) => ({ ...state })
        },
    [DELETE_COMPANY_PERFORMANCE_TASKS]: {
        PENDING: (state) => ({ ...state })
        },

    [LOAD_VENDOR_SNAPSHOT]: {
        FULFILLED: (state, action) => {
            return ({
                ...state,
                loading: false,
                vendorSnapshots: action.payload.data,
            })
        }
    },
    [GET_NEW_TASK_DATA]: {
        FULFILLED: (state, action) => {
            return ({
                ...state,
                loading: false,
                newTaskData: action.payload.data,
            })
        },
        REJECTED: (state, action) => ({
            ...state,
            error: action.payload.data
        }),
    },

    [LOAD_VENDOR_JOBINVITES]: {
        FULFILLED: (state, action) => ({
            ...state,
            vendorJobInvites: action.payload.data,
        }),
        REJECTED: (state) => ({ ...state }),
    },

    [LOAD_VENDOR_FORMULA_BIDS]: {
        FULFILLED: (state, action) => ({
            ...state,
            vendorFormulaBids: action.payload.data,
        }),
        REJECTED: (state) => ({ ...state }),
    },

    [LOAD_COMPANY_FORMULA_BIDS]: {
        FULFILLED: (state, action) => ({
            ...state,
            companyFormulaBids: action.payload.data,
        }),
        REJECTED: (state) => ({ ...state }),
    },

    [SYNC_VENDOR_DATA]: {
        REJECTED: (state) => ({ ...state }),
        FULFILLED: (state) => ({ ...state })
    },
    [COMPANY_WORKER_EMAIL]: {
        REJECTED: (state) => ({ ...state }),
        FULFILLED: (state) => ({ ...state })
    },

    [LOAD_VENDORS_BY_OPTION_TYPE]: {
        FULFILLED: (state, action) => ({
            ...state,
            selectedVendors: action.payload.data
        }),
        REJECTED: (state, action) => ({
            ...state,
            error: action.payload.data
        }),
    },

    [REMOVE_INVITE]: {
        FULFILLED: (state, action) => ({
            ...state,
            vendorJobInvites: state.vendorJobInvites
                .filter(p => p.projectTaskVendorId !== action.payload)
        }),
    },

    [REMOVE_BID]: {
        FULFILLED: (state, action) => ({
            ...state,
            vendorFormulaBids: state.vendorFormulaBids
                .filter(p => p.bidId !== action.payload)
        }),
    }
}

// Actions
const baseUrl = '/api/vendor';
const baseUrlCompany = '/api/company';


export function getFormulaTaskVendorNotification(vendorNotificationId) {
    return {
        type: LOAD_VENDOR_NOTIFICATION,
        payload: axios.get(`${baseUrl}/formulaTask/${vendorNotificationId}`, getAuthHeaders())
    }
}

export function getProjectTaskVendorNotification(vendorNotificationId) {
    return {
        type: LOAD_VENDOR_NOTIFICATION,
        payload: axios.get(`${baseUrl}/projectTask/${vendorNotificationId}`, getAuthHeaders())
    }
}

export function getFormulaTaskForCertification(formulaTaskId) {
    return {
        type: LOAD_FORMULA_FOR_CERTIFICATION,
        payload: axios.get(`${baseUrl}/certify/formulatask/${formulaTaskId}`, getAuthHeaders())
    }
}

export function loadPerformanceData() {
    return {
        type: LOAD_PERFORMANCE_DATA,
        payload: axios.get(`${baseUrl}/performance-data`, getAuthHeaders())
    }
}
export function loadCompanyPerformanceData() {
    return {
        type: LOAD_COMPANY_PERFORMANCE_DATA,
        payload: axios.get(`/api/company/company-performance-data`, getAuthHeaders())
    }
}
export function loadCompanyUserData() {
    return {
        type: LOAD_COMPANY_USER_DATA,
        payload: axios.get(`https://jsonplaceholder.typicode.com/users`, getAuthHeaders())
    }
}
export function loadCompanyUserDetails() {
    return {
        type: LOAD_COMPANY_USER_DETAILS,
        payload: axios.get(`/api/company/company-user-details`, getAuthHeaders())
    }
}

export function selectUser(value) {
    return {
        type: USER_SELECT,
        payload: {
            promise: Promise.resolve(value)
        }
    }
}
export function loadVendorUpSkills() {
    return {
        type: LOAD_VENDOR_UPSKILLS,
        payload: axios.get(`${baseUrl}/upskills`, getAuthHeaders())
    }
}

export function projectTaskNotificationResponse(response) {
    return {
        type: ANSWER_FOR_VENDOR_NOTIFICATION,
        payload: axios.post(`${baseUrl}/projectTaskResponse`, response, getAuthHeaders())
    };
}

export function formulaTaskNotificationResponse(response) {
    return {
        type: ANSWER_FOR_VENDOR_NOTIFICATION,
        payload: axios.post(`${baseUrl}/formulaTaskResponse`, response, getAuthHeaders())
    };
}

export function formulaTaskCertificationResponse(response) {
    return {
        type: ANSWER_FOR_VENDOR_CERTIFICATION,
        payload: axios.post(`${baseUrl}/certificationResponse`, response, getAuthHeaders())
    }
}

export function setPendingVendorNotification(task) {
    return {
        type: SET_PENDING_VENDOR_NOTIFICATION,
        payload: task
    };
}

export function syncVendorData() {
    return {
        type: SYNC_VENDOR_DATA,
        payload: axios.post(`${baseUrl}/syncvendordata`, null, getAuthHeaders())
    };
}
export function companyWorkerEmail(email) {
    const UserDetailsInfo = JSON.parse(localStorage.getItem("UserInfo"));
    const userId = UserDetailsInfo.id;
    const userName = UserDetailsInfo.fullName;

    return {
        type: COMPANY_WORKER_EMAIL,
        // payload: axios.get(`${baseUrlCompany}/company-worker-email?email=${email.email}
        // /&CompanyName=${userName}`, getAuthHeaders())
        payload: axios.get(`${baseUrlCompany}/company-worker-email?email=${email.email}
        /&CompanyName=${userName}/&userId=${userId}`,
            getAuthHeaders())
        // payload: axios.post(`${baseUrlCompany}/company-worker-email`, email, getAuthHeaders())
    };
}

export function updateTaskPrice(formulaTaskId, newPrice) {
    return {
        type: UPDATE_VENDOR_TASK_PRICE,
        payload: axios.put(`${baseUrl}/modify-price/${formulaTaskId}/${newPrice}`, null, getAuthHeaders())
    }
}
export function updateCompanyTaskPrice(formulaTaskId, data) {
    return {
        type: UPDATE_COMPANY_TASK_PRICE,
        payload: axios.put(`${baseUrlCompany}/modify-price/${formulaTaskId}`, data, getAuthHeaders())
    }
}
export function getNewTaskData(formulaTaskId) {
    return {
        type: GET_NEW_TASK_DATA,
        payload: axios.get(`/api/company/company-user-price/${formulaTaskId}`, getAuthHeaders())
    }
}

export function deleteTaskPrice(formulaTaskId) {
    return {
        type: DELETE_VENDOR_TASK_PRICE,
        payload: axios.delete(`${baseUrl}/delete-price/${formulaTaskId}`, getAuthHeaders())
    }
}
export function deleteCompanyUserTasks(userId,formulaTaskId) {
        //console.log('formulaTaskIdffff',userId,formulaTaskId);
        return {
            type: DELETE_COMPANY_USER_TASKS,
            payload: axios.delete(`${baseUrlCompany}/companyuser-delete-price/${userId}/${formulaTaskId}`, 
                getAuthHeaders())
        }
    }
export function deleteCompanyPerformanceTasks(userId,formulaTaskId) {
    //console.log('action Performance',userId,formulaTaskId);
    return {
        type: DELETE_COMPANY_PERFORMANCE_TASKS,
        payload: axios.delete(`${baseUrlCompany}/companyperformance-delete-price/${userId}/${formulaTaskId}`, 
            getAuthHeaders())
    }
}
export function loadVednorSnapshotDetail() {
    return {
        type: LOAD_VENDOR_SNAPSHOT,
        payload: axios.get(`/api/vendor/snapshotdetail`, getAuthHeaders())
    };
}

export function loadVednorJobInvites() {
    return {
        type: LOAD_VENDOR_JOBINVITES,
        payload: axios.get(`/api/tasks/vendorjobinvites`, getAuthHeaders())
    };
}

export function loadVendorFormulaBids() {
    return {
        type: LOAD_VENDOR_FORMULA_BIDS,
        payload: axios.get(`/api/vendor/formula-bids`, getAuthHeaders()),
    };

}

export function loadCompanyFormulaBids() {
    return {
        type: LOAD_COMPANY_FORMULA_BIDS,
        payload: axios.get(`${baseUrlCompany}/company-formula-bids`, getAuthHeaders()),
    };

}

export function loadVendorsByOptionType(formulaId, formulaTaskId, optionType) {
    return {
        type: LOAD_VENDORS_BY_OPTION_TYPE,
        payload: axios.get(
            `/api/vendor/selected-vendors/${formulaId}/${formulaTaskId}/${optionType}`,
            getAuthHeaders()
        ),
    };
}

export function removeJobInvite(value) {
    return {
        type: REMOVE_INVITE,
        payload: {
            promise: Promise.resolve(value)
        }
    };
}

export function removeFormulaBid(value) {
    return {
        type: REMOVE_BID,
        payload: {
            promise: Promise.resolve(value)
        }
    };
}

export default typeToReducer(reducer, initialState);
