import { createSelector } from 'reselect'

import { selectFormulaProjectSkills } from '@selectors/formulaTasks'

import { selectSearchQueryTrimLowerCase } from './layout'

export const getUsers = (state) => state.users.users;

export const getFormulaSkills = (state) => state.formula.formulaSkills;

export const getFormulaTaskSkills = (state) => state.formula.formulaTasks;

export const getFilterUserId = (state) => state.users.filterUserId;

export const getManagerUsers = (state) => state.formula.managerUsers

export const getAllUsers = (state) => state.formula.allUsers;

export const filterUsersByQuery = createSelector(
    [selectSearchQueryTrimLowerCase, getUsers],
    (query, users) => query
        ? users.filter(p =>
            [p.fullName, p.userName].some(t => t && t.toLowerCase().includes(query))
        )
        : users
);

export const getUsersForSkill = createSelector(
    [getManagerUsers, getAllUsers, getFormulaSkills],
    (managerUsers, allUsers, skillMaps) => {
        let usersOptions = {};
        skillMaps.forEach((element, index, array) => {
            usersOptions[element.skillId] = {
                users: (element.canBeOutsourced ? [{
                        key: "outsource", value: "outsource",
                        text: "Outsourcers Available"
                    }] : [])
                    .concat(element.canWorkerHasSkill ? allUsers.map(t => ({
                        key: t.id, value: t.id, text: t.fullName
                    })) : managerUsers.map(t => ({
                        key: t.id, value: t.id, text: t.fullName
                    })))
            }
        });

        return usersOptions;
    }
);

export const getInHouseUsersForSkill = createSelector(
    [getAllUsers, getFormulaSkills],
    (allUsers, skillMaps) => {
        const inHouseUserOptions = {};
        
        skillMaps.forEach((element) => {
            inHouseUserOptions[element.skillId] = {
                users: allUsers
            }
        });

        return inHouseUserOptions;
    }
);

export const getReviewersForSkill = createSelector(
    [getManagerUsers, getFormulaTaskSkills],
    (managerUsers, skillMaps) => {
        const reviewingUserOptions = {};

        skillMaps.forEach((element) => {
            reviewingUserOptions[element.reviewingSkillId] = {
                reviewers: managerUsers
            }
        });

        return reviewingUserOptions;
    }
);

