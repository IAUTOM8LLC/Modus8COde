import { createSelector } from 'reselect'
import { skillAccessor } from '@utils/sort'

import { selectSearchQueryTrimLowerCase, selectSearchColumn } from './layout'

const getProjectTasks = (state) => state.projectTasks.tasks;

export const getSkills = (state) => state.skill.skills;
export const getFilterSkillId = (state) => Number(state.skill.filterSkillId);
export const getFormulaId = (state, props) => props.formulaId;

export const filterSkillsByQuery = createSelector(
    [selectSearchQueryTrimLowerCase, selectSearchColumn, getSkills],
    (query, column, skills) => {
        if (query) {
            const accessor = skillAccessor(column);
            return skills.filter(p =>
                accessor
                && accessor(p)
                && accessor(p).includes(query)
            ) || [];
        } else {
            return skills;
        }
    }
);

export const getAssignedSkillId = (state) => state.pendingTask.pendingTask.assignedSkillId;
export const getReviewingSkillId = (state) => state.pendingTask.pendingTask.reviewingSkillId;

export const selectUsersAssignedToSkill = createSelector(
    [getSkills, getAssignedSkillId],
    (allSkills, assignedSkillId) => {
        const skill = allSkills.find(s =>
            s.id === assignedSkillId);
        const result = !skill ? [] : skill.users.map(s => s.userId);

        return result;
    }
);

export const selectUsersAssignedToReviewingSkill = createSelector(
    [getSkills, getReviewingSkillId],
    (allSkills, reviewingSkillId) => {
        const skill = allSkills.find(s =>
            s.id === reviewingSkillId);
        const result = !skill ? [] : skill.users.map(s => s.userId);

        return result;
    }
);

export const selectSkillsAssignedToTasks = createSelector(
    [getSkills, getProjectTasks],
    (allSkills, tasks) => {
        const ids = tasks.filter(t => t.assignedSkillId !== null)
            .map(t => t.assignedSkillId)
            .concat(tasks
                .filter(t => t.reviewingSkillId !== null)
                .map(t => t.reviewingSkillId)
            );
        return allSkills.filter(t => ids.includes(t.id));
    }
);

export const selectWorkerSkills = getSkills;
export const selectReviewingSkills = createSelector(
    [getSkills],
    (skills) => {
        return skills.filter(t => !t.isWorkerSkill);
    }
);

