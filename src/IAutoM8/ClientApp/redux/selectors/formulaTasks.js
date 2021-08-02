import { createSelector } from 'reselect'
import { formulaRootTasks } from '@utils/task'

const getFormulaTaskId = (state) => Number(state.pendingTask.pendingTask.id) || 0;
const getFormulaTasks = (state) => state.formulaTasks.tasks;
const getRootTaskIds = (state, props) => props.rootTaskIds || [];

const getSkills = (state) => state.skill.skills;

export const selectTasksForOptions = createSelector(
    [getFormulaTaskId, getFormulaTasks],
    (id, formulaTasks) => formulaTasks.filter(t => t.id !== id) || []
);

export const selectRootTasks = createSelector(
    [getRootTaskIds, getFormulaTasks],
    (ids, formulaTasks) => {
        if (formulaTasks.length === 0)
            return [];

        if (ids.length > 0) {
            const selectedTask = formulaTasks.filter(t => ids.includes(t.id));
            return formulaRootTasks(selectedTask, ids);
        }

        return formulaRootTasks(formulaTasks);
    }
);

export const getFilteredFormulaTasks = createSelector(
    [getRootTaskIds, getFormulaTasks],
    (ids, formulaTasks) => ids.length > 0
        ? formulaTasks.filter(t => ids.includes(t.id))
        : formulaTasks
);

export const selectFormulaProjectSkills = createSelector(
    [getFilteredFormulaTasks, getSkills],
    (formulaTasks, skills) => {
        if (skills.length === 0)
            return [];
        const result = [];
        for (const t of formulaTasks) {
            if (t.assignedSkillId && result.every(r => r.skillId !== t.assignedSkillId)) {
                result.push({
                    skillId: t.assignedSkillId,
                    userIds: skills.find(s =>
                        s.id === t.assignedSkillId).users.map(s => s.userId),
                    isOutsorced: false
                });
            }

            if (t.reviewingSkillId) {
                const rIndex = result.findIndex(r => r.skillId === t.reviewingSkillId);
                if (rIndex !== -1) {
                    result[rIndex].isReviewingSkill = true;
                } else {
                    result.push({
                        skillId: t.reviewingSkillId,
                        userIds: skills.find(s =>
                            s.id === t.reviewingSkillId).users.map(s => s.userId),
                        isOutsorced: false,
                        isReviewingSkill: true
                    });
                }
            }
        }

        return result;
    }
)
