import axios from 'axios'
import typeToReducer from 'type-to-reducer'
import update from 'immutability-helper'

import { getAuthHeaders } from '@infrastructure/auth'

import createCrudReducer from './abstract'

const ADD_CUSTOM_SKILL = 'modus/SKILL/ADD_CUSTOM_SKILL';
const SET_FILTER_SKILL_ID = 'modus/SKILL/SET_FILTER_SKILL_ID';
const LOAD_SKILLS_BY_TEAM_ID = 'modus/SKILL/LOAD_SKILLS_BY_TEAM_ID';
const LOAD_REVIEWER_SKILLS = 'modus/SKILL/LOAD_REVIEWER_SKILLS';

const initialState = {
    loading: false,
    skills: [],
    revskills: [],
    filterSkillId: 0
};

const {
    reducer: crudReducer,
    actions: { load, add, edit, remove }
} = createCrudReducer('skill', '/api/skill')

const reducer = typeToReducer({
    ...crudReducer,

    [SET_FILTER_SKILL_ID]: (state, action) => ({
        ...state,
        filterSkillId: action.payload
    }),

    [LOAD_SKILLS_BY_TEAM_ID]: {
        PENDING: (state) => ({ ...state }),
        REJECTED: (state) => ({ ...state }),
        FULFILLED: (state, action) => ({
            ...state,
            skills: action.payload.data
        })
    },

    [LOAD_REVIEWER_SKILLS]: {
        PENDING: (state) => ({ ...state }),
        REJECTED: (state) => ({ ...state }),
        FULFILLED: (state, action) => ({
            ...state,
            revskills: action.payload.data
        })
    },

    [ADD_CUSTOM_SKILL]: {
        FULFILLED: (state, action) => ({
            ...state,
            skills: update(state.skills, { $push: [action.payload.data] })
        })
    }

}, initialState);

// TODO: This belongs to layout reducer
const setFilterSkillId = (skillId) => ({
    type: SET_FILTER_SKILL_ID,
    payload: skillId
});

const loadSkillsByTeam = (teamId) => ({
    type: LOAD_SKILLS_BY_TEAM_ID,
    payload: axios.get(`/api/skill/team/${teamId}`, getAuthHeaders())
});

const loadReviewerSkills = () => ({
    type: LOAD_REVIEWER_SKILLS,
    payload: axios.get(`/api/skill/revskills`, getAuthHeaders())
});

const addCustomSkill = (customSkill) => ({
    type: ADD_CUSTOM_SKILL,
    payload: axios.post("/api/skill/custom-skill", customSkill, getAuthHeaders())
})

export {
    reducer as default,

    loadSkillsByTeam,
    loadReviewerSkills,
    setFilterSkillId,
    addCustomSkill,
    load as loadSkills,
    add as addSkill,
    edit as editSkill,
    remove as deleteSkill,
}
