import _ from 'lodash'
import axios from 'axios'
import typeToReducer from 'type-to-reducer'
import update from 'immutability-helper'

import { getAuthHeaders } from '@infrastructure/auth'
import { Function, Promise } from 'core-js';
import qs from 'qs';

import { fullName } from '../../utils/validators';

// Types
const LOAD_FORMULAS = 'react/formula/LOAD_FORMULAS';
const LOAD_OWNED_FORMULAS = 'react/formula/LOAD_OWNED_FORMULAS';
const LOAD_FORMULA = 'react/formula/LOAD_FORMULA';
const LOAD_CATEGORIES = 'react/formula/LOAD_CATEGORIES';
const LOAD_SKILLS = 'react/formula/LOAD_SKILLS';
const ADD_FORMULA = 'react/formula/ADD_FORMULA';
const DELETE_FORMULA = 'react/formula/DELETE_FORMULA';
const EDIT_FORMULA = 'react/formula/EDIT_FORMULA';
const UNLOCK_FORMULA = 'react/formula/UNLOCK_FORMULA';
const LOCK_FORMULA = 'react/formula/LOCK_FORMULA';
const FORMULA_SELECTOR = 'react/formula/FORMULA_SELECTOR';
const FORMULA_CATEGORY_APPLY = 'react/formula/FORMULA_CATEGORY_APPLY';
const FORMULA_CATEGORY_REMOVE = 'react/formula/FORMULA_CATEGORY_REMOVE';
const FORMULA_CATEGORY_CLEAR = 'react/formula/FORMULA_CATEGORY_CLEAR';
const FORMULA_SEARCH = 'react/formula/FORMULA_SEARCH';
const FORMULA_SORT = 'react/formula/FORMULA_SORT';
const FORMULA_PAGE = 'react/formula/FORMULA_PAGE';
const FORMULA_TOGGLE_SELECTION = 'react/formula/FORMULA_TOGGLE_SELECTION';
const FORMULA_TOGGLE_ALL_SELECTION = 'react/formula/FORMULA_TOGGLE_ALL_SELECTION';
const FORMULA_NEXT_PAGE = 'react/formula/FORMULA_NEXT_PAGE';
const FORMULA_CLEAR_FORMULAS = 'react/formula/FORMULA_CLEAR_FORMULAS';
const FORMULA_OWNES = 'react/formula/FORMULA_OWNES';
const FORMULA_FILTER = 'react/formula/FORMULA_FILTER';
const CHANGE_FORMULA_STATUS = 'react/formula/CHANGE_FORMULA_STATUS';
const ADD_STAR_TO_SELECTED_FORMULA = 'react/formula/ADD_STAR_TO_SELECTED_FORMULA';
const REMOVE_STAR_TO_SELECTED_FORMULA = 'react/formula/REMOVE_STAR_TO_SELECTED_FORMULA';
const COPY_FORMULA = 'react/formula/COPY_FORMULA';
const LOAD_MEAN_TAT_FOR_FORMULA = 'react/formula/LOAD_MEAN_TAT_FOR_FORMULA';

const initialState = {
    loading: false,
    formulas: [],
    ownedFormulas: [],
    selectedFormula: {},
    formulaTasks: [],
    formulaSkills: [],
    allUsers: [],
    managerUsers: [],
    categories: [],
    searchModel: {
        perPage: 45,
        page: 1,
        isCustom: false,
        //isMyFormula: true,
        filterCategorieIds: [],
        filterSearch: '',
        sortField: 'name',
        sortDirection: 'ascending'
    },
    filterButtonModel: {
        selectedButton: 'all',
        isAllEnabled: true,
        isTracksEnabled: false,
        isFormulasEnabled: false
    },
    selectedFormulas: [],
    canNextPage: false,
    outsourceUsers: [],
    formulaMeanTat: '00:00',
    formulaMeanTatObj: {}
    // topRatedVendors: [],
};

// Reducer
const reducer = {
    [FORMULA_OWNES]: {
        FULFILLED: (state, action) => ({
            ...state,
            formulas: state.formulas.map(formula => {
                if (action.payload.some(id => id === formula.id))
                    return {
                        ...formula,
                        isOwned: true
                    }
                return formula;
            })
        })
    },

    [CHANGE_FORMULA_STATUS]: {
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
            const { formulas } = state;
            const index = formulas.findIndex(val => val.id === action.payload.data.id);
            return ({
                ...state,
                loading: false,
                formulas: update(formulas, {
                    [index]: {
                        status: {
                            $set: action.payload.data.status || 1
                        }
                    }
                })
            })
        }
    },

    [FORMULA_CLEAR_FORMULAS]: {
        FULFILLED: (state) => ({
            ...state,
            //formulas: [],
            searchModel: {
                ...state.searchModel,
                page: 1,
                isCustom: false,
                filterCategorieIds: [],
                filterSearch: '',
                sortField: 'name',
                sortDirection: 'ascending'
            }
        }),
    },

    [FORMULA_SELECTOR]: {
        FULFILLED: (state, action) => ({
            ...state,
            formulas: [],
            canNextPage: false,
            searchModel: {
                ...state.searchModel,
                //isMyFormula: action.payload,
                isCustom: action.payload,
                page: 1,
                sortField: 'name',
                sortDirection: 'ascending'
            },
            selectedFormulas: []
        })
    },

    [FORMULA_CATEGORY_APPLY]: {
        FULFILLED: (state, action) => ({
            ...state,
            formulas: [],
            canNextPage: false,
            searchModel: {
                ...state.searchModel,
                filterCategorieIds: action.payload,
                page: 1
            },
            selectedFormulas: []
        })
    },

    [FORMULA_SORT]: {
        FULFILLED: (state, action) => ({
            ...state,
            formulas: [],
            canNextPage: false,
            searchModel: {
                ...state.searchModel,
                page: 1,
                sortField: action.payload,
                sortDirection: state.searchModel.sortField === action.payload
                    ? state.searchModel.sortDirection === 'ascending'
                        ? 'descending' : 'ascending'
                    : 'ascending'
            }
        })
    },

    [FORMULA_FILTER]: {
        FULFILLED: (state, action) => ({
            ...state,
            filterButtonModel: {
                ...state.filterButtonModel,
                selectedButton: action.payload,
                isAllEnabled: action.payload === "all",
                isTracksEnabled: action.payload === "tracks",
                isFormulasEnabled: action.payload === "formulas"
            }
        })
    },

    [FORMULA_SEARCH]: {
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

    [FORMULA_PAGE]: {
        FULFILLED: (state, action) => ({
            ...state,
            searchModel: {
                ...state.searchModel,
                page: action.payload
            }
        })
    },

    [FORMULA_NEXT_PAGE]: {
        FULFILLED: (state, action) => ({
            ...state,
            searchModel: {
                ...state.searchModel,
                page: state.searchModel.page + 1
            },
            canNextPage: false
        })
    },

    [FORMULA_CATEGORY_REMOVE]: {
        FULFILLED: (state, action) => ({
            ...state,
            formulas: [],
            canNextPage: false,
            searchModel: {
                ...state.searchModel,
                filterCategorieIds: state.searchModel.filterCategorieIds
                    .filter(value => value !== action.payload),
                page: 1
            },
            selectedFormulas: []
        })
    },

    [FORMULA_CATEGORY_CLEAR]: {
        FULFILLED: (state, action) => ({
            ...state,
            formulas: [],
            canNextPage: false,
            searchModel: {
                ...state.searchModel,
                filterCategorieIds: [],
                page: 1
            },
            selectedFormulas: []
        })
    },

    [FORMULA_TOGGLE_SELECTION]: {
        FULFILLED: (state, action) => {
            const { selectedFormulas } = state;
            const index = selectedFormulas.findIndex(id => id === action.payload);
            return ({
                ...state,
                selectedFormulas: index === -1
                    ? update(selectedFormulas, { $push: [action.payload] })
                    : update(selectedFormulas, { $splice: [[index, 1]] })
            })
        }
    },

    [FORMULA_TOGGLE_ALL_SELECTION]: {
        FULFILLED: (state, action) => {
            const { selectedFormulas, formulas } = state;
            const isAnySelected = formulas.some(f => selectedFormulas
                .some(id => id === f.id));
            if (isAnySelected) {
                const allUnselected = formulas
                    .filter(f => !f.isOwned &&
                        !selectedFormulas.some(id => id === f.id))
                    .map(f => f.id);
                if (allUnselected.length === 0) {
                    return ({
                        ...state,
                        selectedFormulas: selectedFormulas.filter(id =>
                            !formulas.some(f => f.id === id))
                    })
                }
                else
                    return ({
                        ...state,
                        selectedFormulas:
                            update(selectedFormulas, { $push: allUnselected })
                    })
            }
            else {
                const allUnselected = formulas
                    .filter(f => !f.isOwned)
                    .map(f => f.id);
                return ({
                    ...state,
                    selectedFormulas:
                        update(selectedFormulas, { $push: allUnselected })
                })
            }
        }
    },

    [LOAD_FORMULA]: {
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
            selectedFormula: action.payload.data
        })
    },

    [LOAD_FORMULAS]: {
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
                formulas: page === 1 ? data.formulas : state.formulas.concat(data.formulas),
                searchModel: {
                    ...state.searchModel,
                    totalCount: data.totalCount,
                    isAdmin: data.isAdmin
                },
                canNextPage: data.formulas.length ===
                    state.searchModel.perPage
            })
        }
    },

    [LOAD_OWNED_FORMULAS]: {
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
            ownedFormulas: action.payload.data
        })
    },

    [LOAD_CATEGORIES]: {
        FULFILLED: (state, action) => ({
            ...state,
            categories: action.payload.data
        })
    },

    [LOAD_SKILLS]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state, action) => ({
            ...state,
            error: action.payload.data,
            loading: false
        }),
        // FULFILLED: (state, action) => ({
        //     ...state,
        //     loading: false,
        //     formulaTasks: action.payload.data.formulaTaskItems,
        //     formulaSkills: action.payload.data.skillItems.map(skill => ({
        //         ...skill,
        //         userIds: (skill.canBeOutsourced ? ["outsource"] : [])
        //             .concat(skill.userIds)
        //     })),
        //     allUsers: action.payload.data.allUsers,
        //     managerUsers: action.payload.data.managerUsers,
        //     outsourceUsers: action.payload.data.outsourcedUsers,
        // }),

        FULFILLED: (state, action) => {
            const topRatedVendors = action.payload[1].data;

            return {
                ...state,
                loading: false,
                formulaTasks: action.payload[0].data.formulaTaskItems.map(skill => {
                    if (skill.canBeOutsourced) {
                        const selectedVendors = topRatedVendors
                            .find(v => v.formulaTaskId === skill.formulaTaskId);
                        skill.outsourceUserIds = selectedVendors ? selectedVendors.outsourcerIds : [];
                    }

                    if (skill.isDisabled) {
                        skill.userIds = [];
                        skill.reviewingUserIds = [];
                        skill.outsourceUserIds = [];
                        skill.certifiedVendors = [];
                    }

                    return { ...skill };
                }),
                formulaSkills: action.payload[0].data.skillItems.map(skill => ({
                    ...skill,
                    userIds: (skill.canBeOutsourced ? ["outsource"] : [])
                        .concat(skill.userIds)
                })),
                allUsers: action.payload[0].data.allUsers,
                managerUsers: action.payload[0].data.managerUsers,
                outsourceUsers: action.payload[0].data.outsourcedUsers,
            }
        }
    },

    [ADD_FORMULA]: {
        PENDING: (state) => ({
            ...state,
        }),
        FULFILLED: (state, action) => ({
            ...state,
            formulas: update(state.formulas, {
                $push: [{
                    ...action.payload.data,
                    categories: state.categories.filter(cat => action.payload.data
                        .categoryIds.some(id => id === cat.id))
                        .map(cat => cat.name)
                }]
            })
        })
    },

    [EDIT_FORMULA]: {
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
            const { formulas, categories } = state;
            const index = formulas.findIndex(val => val.id === action.payload.data.id);
            categories.filter(cat => action.payload.data
                .categoryIds.some(id => id === cat.id))
                .map(cat => cat.name);
            return ({
                ...state,
                formulas: update(formulas, {
                    [index]: {
                        $set: {
                            ...action.payload.data,
                            categories: categories.filter(cat => action.payload.data
                                .categoryIds.some(id => id === cat.id))
                                .map(cat => cat.name)
                        }
                    }
                })
            });
        }
    },

    [DELETE_FORMULA]: {
        PENDING: (state, action) => ({
            ...state,
            formulas: state.formulas.filter(p => p.id !== action.payload)
        })
    },

    [UNLOCK_FORMULA]: {
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
            const { formulas } = state;
            const index = formulas.findIndex(val => val.id === action.payload.data);

            return ({
                ...state,
                formulas: update(formulas, { [index]: { isLocked: { $set: false } } })
            });
        }
    },

    [LOCK_FORMULA]: {
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
            const { formulas } = state;
            const index = formulas.findIndex(val => val.id === action.payload.data);

            return ({
                ...state,
                formulas: update(formulas, { [index]: { isLocked: { $set: true } } })
            });
        }
    },

    [ADD_STAR_TO_SELECTED_FORMULA]: {
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
            const { formulas } = state;
            const index = formulas.findIndex(val => val.id === action.payload.data);

            return ({
                ...state,
                loading: false,
                formulas: _.orderBy(
                    update(formulas, { [index]: { isStarred: { $set: true } } }),
                    ['isStarred', state.searchModel.sortField],
                    ['desc', state.searchModel.sortDirection.substring(0, 3)]
                )
            });
        }
    },

    [REMOVE_STAR_TO_SELECTED_FORMULA]: {
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
            const { formulas } = state;
            const index = formulas.findIndex(val => val.id === action.payload.data);

            return ({
                ...state,
                formulas: _.orderBy(
                    update(formulas, { [index]: { isStarred: { $set: false } } }),
                    ['isStarred', state.searchModel.sortField],
                    ['desc', state.searchModel.sortDirection.substring(0, 3)]
                )
            });
        }
    },

    [COPY_FORMULA]: {
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
            formulas: update(state.formulas, {
                $push: [{
                    ...action.payload.data,
                    categories: [], // adding categories as an empty array
                }]
            })
        })
    },

    //[LOAD_MEAN_TAT_FOR_FORMULA]: {
    //    PENDING: (state) => ({ ...state, loading: true }),
    //    REJECTED: (state, action) => ({
    //        ...state,
    //        loading: false,
    //        error: action.payload.data
    //    }),
    //    FULFILLED: (state, action) => ({
    //        ...state,
    //        loading: false,
    //        formulaMeanTat: action.payload.data
    //    })
    //},


    [LOAD_MEAN_TAT_FOR_FORMULA]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload.data
        }),
        FULFILLED: (state, action) => {            
            return ({
                ...state,
                loading: false,
                formulaMeanTatObj: action.payload.data
            });
        }
    },
};

// Actions
const baseUrl = '/api/formula';

export function loadFormulas(isInitialLoad) {

    return (dispatch, getStore) => {
        const {
            page,
            perPage,
            isCustom,
            //isMyFormula,
            filterCategorieIds,
            filterSearch,
            sortField,
            sortDirection
        } = getStore().formula.searchModel;
        if (isInitialLoad && page !== 1)
            return;
        dispatch({
            type: LOAD_FORMULAS,
            payload: Promise.all([
                //axios.get(`${baseUrl}${isMyFormula ? "" : "/allformulas"}`, {
                axios.get(`${baseUrl}${isCustom ? "/custom" : "/public"}`, {
                    params: {
                        page,
                        perPage,
                        isCustom,
                        sortField,
                        filterSearch,
                        sortDirection,
                        filterCategorieIds,
                    },
                    paramsSerializer: function (params) {
                        return qs.stringify(params, { arrayFormat: 'repeat' })
                    },
                    ...getAuthHeaders()
                }),
                Promise.resolve(page)
            ])
        })

    }
}

export function loadFormulaById(formulaId) {
    return {
        type: LOAD_FORMULA,
        payload: axios.get(`${baseUrl}/${formulaId}`, getAuthHeaders())
    }
}

export function loadCategories() {
    return {
        type: LOAD_CATEGORIES,
        payload: axios.get(`${baseUrl}/categories`, getAuthHeaders())
    }
}

export function loadFormulaMeanTat(formulaId) {
    return {
        type: LOAD_MEAN_TAT_FOR_FORMULA,
        payload: axios.get(`${baseUrl}/get-formula-mean-tat/${formulaId}`, getAuthHeaders())
    }
}

export function addFormula(formula) {
    return {
        type: ADD_FORMULA,
        payload: axios.post(baseUrl, formula, getAuthHeaders())
    };
}

export function editFormula(formula) {

    return {
        type: EDIT_FORMULA,
        payload: axios.put(baseUrl, formula, getAuthHeaders())
    };
}

export function deleteFormula(formulaId) {
    return {
        type: DELETE_FORMULA,
        payload: {
            data: formulaId,
            promise: axios.delete(`${baseUrl}/${formulaId}`, getAuthHeaders())
        }
    };
}

export function updateShareStatus(formulaId, shareStatus) {
    return {
        type: EDIT_FORMULA,
        payload: axios.put(`api/formula-sharing/${formulaId}/status`, shareStatus, getAuthHeaders())
    }
}

export function lockFormula(formulaId) {
    return {
        type: LOCK_FORMULA,
        payload: axios.put(`${baseUrl}/lock`, JSON.stringify(formulaId), getAuthHeaders())
    }
}

export function loadSkillsForFormula(formulaId) {
    return {
        type: LOAD_SKILLS,
        //payload: axios.get(`${baseUrl}/get-skills/${formulaId}`, getAuthHeaders())
        payload: Promise.all([
            axios.get(`${baseUrl}/get-skills/${formulaId}`, getAuthHeaders()),
            axios.get(`/api/vendor/selected-vendors/${formulaId}`, getAuthHeaders())
        ])
    }
}

export function unlockFormula(formulaId) {
    return {
        type: UNLOCK_FORMULA,
        payload: axios.put(`${baseUrl}/unlock`, JSON.stringify(formulaId), getAuthHeaders())
    }
}

export function formulaSelector(value) {
    return {
        type: FORMULA_SELECTOR,
        payload: {
            promise: Promise.resolve(value)
        }
    }
}

export function formulaCategoryClear() {
    return {
        type: FORMULA_CATEGORY_CLEAR,
        payload: {
            promise: Promise.resolve()
        }
    }
}

export function formulaCategoryApply(value) {
    return {
        type: FORMULA_CATEGORY_APPLY,
        payload: {
            promise: Promise.resolve(value)
        }
    }
}

export function formulaCategoryRemove(value) {
    return {
        type: FORMULA_CATEGORY_REMOVE,
        payload: {
            promise: Promise.resolve(value)
        }
    }
}

export function formulaSearch(event, { value }) {
    return {
        type: FORMULA_SEARCH,
        payload: {
            promise: Promise.resolve(value)
        }
    }
}

export function formulaSort(value) {
    return {
        type: FORMULA_SORT,
        payload: {
            promise: Promise.resolve(value)
        }
    }
}

export function formulaPage(value) {
    return {
        type: FORMULA_PAGE,
        payload: {
            promise: Promise.resolve(value)
        }
    }
}
export function formulaToggleSelection(value) {
    return {
        type: FORMULA_TOGGLE_SELECTION,
        payload: {
            promise: Promise.resolve(value)
        }
    }

}
export function formulaToggleAllSelection() {
    return {
        type: FORMULA_TOGGLE_ALL_SELECTION,
        payload: {
            promise: Promise.resolve()
        }
    }

}
export function nextPage() {
    return {
        type: FORMULA_NEXT_PAGE,
        payload: {
            promise: Promise.resolve()
        }
    }
}
// export function loadOwnedFormulas() {
//     return (dispatch, getStore) => {
//         const { isCustom } = getStore().formula.searchModel;
//         dispatch({
//             type: LOAD_FORMULAS,
//             payload: Promise.all([
//                 axios.get(`${baseUrl}`, {
//                     params: {
//                         page: 1,
//                         perPage: 10000,
//                         isCustom,
//                     },
//                     paramsSerializer: function (params) {
//                         return qs.stringify(params, { arrayFormat: 'repeat' })
//                     },
//                     ...getAuthHeaders()
//                 }),
//                 Promise.resolve(1)
//             ])
//         });
//     };
// }
export function loadOwnedFormulas() {
    return {
        type: LOAD_OWNED_FORMULAS,
        payload: axios.get(`${baseUrl}/ownedformulas`, getAuthHeaders())
    }
}
export function clearFormulas() {
    return {
        type: FORMULA_CLEAR_FORMULAS,
        payload: {
            promise: Promise.resolve()
        }
    }
}
export function formulaOwnes(ids) {
    return {
        type: FORMULA_OWNES,
        payload: {
            promise: Promise.resolve(ids)
        }
    }
}

export function formulaFilter(value) {
    return {
        type: FORMULA_FILTER,
        payload: {
            promise: Promise.resolve(value)
        }
    };
}

export function changeFormulaStatus(formulaId) {
    return {
        type: CHANGE_FORMULA_STATUS,
        payload: axios.get(`${baseUrl}/ChangeFormulaStatus/${formulaId}`, getAuthHeaders())
    }
}

export function addStar(formulaId) {
    return {
        type: ADD_STAR_TO_SELECTED_FORMULA,
        payload: axios.patch(`${baseUrl}/addstar`, JSON.stringify(formulaId), getAuthHeaders())
    }
}

export function removeStar(formulaId) {
    return {
        type: REMOVE_STAR_TO_SELECTED_FORMULA,
        payload: axios.patch(`${baseUrl}/removestar`, JSON.stringify(formulaId), getAuthHeaders())
    }
}

export function copyFormula(formula) {
    return {
        type: COPY_FORMULA,
        payload: axios.post(`${baseUrl}/copyformula`, formula, getAuthHeaders())
    }
}

export default typeToReducer(reducer, initialState)
