import React from 'react'
import {
    Segment,
    // Button,
    // Divider, 
    // Dropdown, 
    // Sticky 
} from 'semantic-ui-react'

import {
    ModusButton, 
    // Inline, 
    SimpleSegment
} from '@components'

import FormulasCategoryDropdown from './FormulasCategoryDropdown.jsx';

export default function FormulasActionFilters({ 
    categories,
    filterCategorieIds,
    formulaCategoryApply,
    // isMyFormula,
    onAddFormulaClick,
    // onDownloadProjects, 
    // contextRef
}) {

    return (
        <Segment clearing basic>
            <SimpleSegment clearing floated="left">
                {/* {
                    isMyFormula
                        ?
                        <ModusButton
                            filled
                            icon="iauto--add-white"
                            content="New formula"
                            onClick={onAddFormulaClick}
                            whenPermitted="createFormula"
                        />
                        : <Sticky context={contextRef} offset={50}
                            style={{ width: '231px' }}
                            className="iauto-formulas-header_add-formulas">
                            <ModusButton
                                filled
                                content="ADD TO MY FORMULA VAULT"
                                onClick={onDownloadProjects}
                                whenPermitted="createFormula"
                            />
                        </Sticky>
                } */}
                <ModusButton
                    filled
                    icon="iauto--add-white"
                    content="New formula"
                    onClick={onAddFormulaClick}
                    whenPermitted="createFormula"
                />
            </SimpleSegment>
            {/* <FormulasCategoryDropdown
                categories={categories}
                filterCategorieIds={filterCategorieIds}
                formulaCategoryApply={formulaCategoryApply}
            /> */}
        </Segment>
    );
}
