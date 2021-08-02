import { createSelector } from 'reselect'

import { formulaAccessor } from '@utils/sort'

import { selectSearchQueryTrimLowerCase, selectSearchColumn } from './layout'

const getFormulas = (state) => state.formula.formulas;
export const getState = (state) => state;

const getOwnedFormulas = (state) => state.formula.ownedFormulas;
const getFormulaIdParam = (state, props) => Number(props.match.params.formulaId) || 0;

export const selectFormulaFromUrl = createSelector(
    [getFormulaIdParam, getFormulas],
    (formulaId, formulas) => {
        return formulas.find(f => f.id === formulaId) || {};
    }
);

export const selectCurrentFormulaShared = createSelector(
    [selectFormulaFromUrl],
    (currentFormula) => {
        return currentFormula
            && currentFormula.formulaShareStatus
            && currentFormula.formulaShareStatus.shareType !== 0;
    }
);

export const selectCurrentFormulaName = createSelector(
    [selectFormulaFromUrl],
    (currentFormula) => currentFormula.name
);

export const filterFormulasByQuery = createSelector(
    [selectSearchQueryTrimLowerCase, selectSearchColumn, getFormulas],
    (query, column, formulas) => {
        if (query) {
            const accessor = formulaAccessor(column);
            return formulas.filter(f =>
                accessor
                && accessor(f)
                && accessor(f).includes(query)
            ) || [];
        }

        return formulas;
    }
);

// export const selectFormulasWithTasks = createSelector(
//     [getFormulas],
//     (formulas) => {
//         return formulas.filter(f => f.tasksNumber > 0);
//     }
// );


export const selectFormulasWithTasks = createSelector(
    [getOwnedFormulas],
    (ownedFormulas = []) => {
        return ownedFormulas.filter(f => f.tasksNumber > 0);
    }
);