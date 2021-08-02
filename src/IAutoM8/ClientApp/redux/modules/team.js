import axios from 'axios'
import typeToReducer from 'type-to-reducer'
import update from 'immutability-helper'

import { getAuthHeaders } from '@infrastructure/auth'

import createCrudReducer from './abstract'

const LOAD_DDL_TEAMS = 'modus/team/LOAD_DDL_TEAMS';
const LOAD_OUTSOURCERS = 'modus/team/LOAD_OUTSOURCERS';
const LOAD_SKILLS_FOR_TEAM = 'modus/team/LOAD_SKILLS_FOR_TEAM';
const SET_FILTER_TEAM_ID = 'modus/team/SET_FILTER_TEAM_ID';
const SET_FORMULA_OUTSOURCE = 'modus/team/SET_FORMULA_OUTSOURCE';
const SAVE_FORMULA_OUTSOURCE = 'modus/team/SAVE_FORMULA_OUTSOURCE';
const PUBLISH_FORMULA = 'modus/team/PUBLISH_FORMULA';
const PUBLISH_TEAM = 'modus/team/PUBLISH_TEAM';
const PUBLISH_SKILL = 'modus/team/PUBLISH_SKILL';

const baseUrl = '/api/teams';

const initialState = {
    loading: false,
    teams: [],
    ddlTeams: [],
    error: null,
    teamSkills: [],
    filterTeamId: 0,
    outsourcers: [],
    updatedOutsources: []
};

const {
    reducer: crudReducer,
    actions: { load, add, edit, remove }
} = createCrudReducer('team', `${baseUrl}`)

const reducer = typeToReducer({
    ...crudReducer,

    [SET_FILTER_TEAM_ID]: (state, action) => ({
        ...state,
        filterTeamId: action.payload
    }),

    [LOAD_SKILLS_FOR_TEAM]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload
        }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            teamSkills: action.payload.data
        })
    },

    [PUBLISH_FORMULA]: {        
        FULFILLED: (state, action) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state, action) => ({
            ...state,
            loading: false,
            error: action.payload
        }),
    },

    [PUBLISH_TEAM]: {
        FULFILLED: (state, action) => ({
            ...state,
            loading: true
        })
    },

    [PUBLISH_SKILL]: {
        FULFILLED: (state, action) => ({
            ...state,
            loading: true
        })
    },

    [LOAD_DDL_TEAMS]: {
        PENDING: (state) => ({ ...state }),
        REJECTED: (state) => ({ ...state }),
        FULFILLED: (state, action) => ({
            ...state,
            ddlTeams: action.payload.data
        })
    },

    [LOAD_OUTSOURCERS]: {
        PENDING: (state) => ({ ...state }),
        REJECTED: (state) => ({ ...state }),
        FULFILLED: (state, action) => ({
            ...state,
            outsourcers: action.payload.data
        })
    },
    [SET_FORMULA_OUTSOURCE]: {
        FULFILLED: (state, action) => {
            const index = state.updatedOutsources
                .findIndex(o => o.id === action.payload.id);
            const indexOutsource = state.outsourcers
                .findIndex(o => o.id === action.payload.id);

            if (index > -1) {
                return ({
                    ...state,
                    updatedOutsources: update(state.updatedOutsources, {
                        [index]: {
                            $set: {
                                id: action.payload.id,
                                isSelected: action.payload.isSelected
                            }
                        }
                    }),
                    outsourcers: update(state.outsourcers, {
                        [indexOutsource]: {
                            $apply: (out) => ({
                                ...out,
                                status: action.payload.isSelected ?
                                    'None' : 'DeclinedByOwner'
                            })
                        }
                    })
                })
            }
            else {
                return ({
                    ...state,
                    updatedOutsources: update(state.updatedOutsources, {
                        $push: [{
                            id: action.payload.id,
                            isSelected: action.payload.isSelected
                        }]
                    }),
                    outsourcers: update(state.outsourcers, {
                        [indexOutsource]: {
                            $apply: (out) => ({
                                ...out,
                                status: action.payload.isSelected ?
                                    'None' : 'DeclinedByOwner'
                            })
                        }
                    })
                })
            }
        }
    },
    [SAVE_FORMULA_OUTSOURCE]: {
        PENDING: (state) => ({
            ...state,
            loading: true
        }),
        REJECTED: (state) => ({
            ...state,
            loading: false
        }),
        FULFILLED: (state) => ({
            ...state,
            loading: false,
            hasMore: true,
            outsources: [],
            updatedOutsources: []
        })
    },

}, initialState);

// TODO: This belongs to layout reducer
const setFilterTeamId = (teamId) => ({
    type: SET_FILTER_TEAM_ID,
    payload: teamId
});

const loadDDLTeams = () => ({
    type: LOAD_DDL_TEAMS,
    payload: axios.get(`${baseUrl}/allteams`, getAuthHeaders())
})

const loadSkillsForNewTeam = (teamId) => ({
    type: LOAD_SKILLS_FOR_TEAM,
    payload: axios.get(`${baseUrl}/GetUnusedSkills/${teamId}`, getAuthHeaders())
});

const publishFormula = (formulaId) => ({
    type: PUBLISH_FORMULA,
    payload: axios.get(`${baseUrl}/publishformula/${formulaId}`, getAuthHeaders())
});

const publishTeam = (teamId) => ({
    type: PUBLISH_TEAM,
    payload: axios.get(`${baseUrl}/publishteam/${teamId}`, getAuthHeaders())
});

const publishSkill = (skillId) => ({
    type: PUBLISH_SKILL,
    payload: axios.get(`${baseUrl}/publishskill/${skillId}`, getAuthHeaders())
});



const loadOutsourcers = (id) => ({
    type: LOAD_OUTSOURCERS,
    payload: axios.get(`/api/formulaTask/outsources/${id}/0/100`, getAuthHeaders())
});

const setFormulaOutsourceAssigne = (id, isSelected) => {
    return (dispatch) => {
        dispatch({
            type: SET_FORMULA_OUTSOURCE,
            payload: Promise.resolve({
                id, isSelected
            })
        })
    };
};

const saveFormulaOutsource = (id) => {
    return (dispatch, getStore) => {
        dispatch({
            type: SAVE_FORMULA_OUTSOURCE,
            payload: axios.put(`/api/formulaTask/outsources`, {
                taskId: id,
                outsources: getStore().team.updatedOutsources
            }, getAuthHeaders())
        });
    };
};

export {
    reducer as default,

    loadSkillsForNewTeam,
    publishFormula,
    publishTeam,
    publishSkill,
    loadOutsourcers,
    loadDDLTeams,
    setFilterTeamId,
    setFormulaOutsourceAssigne,
    saveFormulaOutsource,
    load as loadTeams,
    add as addTeam,
    edit as editTeam,
    remove as deleteTeam,
}
