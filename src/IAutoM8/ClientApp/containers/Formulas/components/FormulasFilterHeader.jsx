import React from "react";
import { 
    Segment, 
    // Button, 
    Divider, 
    // Dropdown, 
    Input 
} from "semantic-ui-react";

// import { ModusButton, Inline, SimpleSegment } from "@components";

import FormulasHeader from "./FormulasHeader.jsx";
import FormulasActionFilters from "./FormulasActionFilters.jsx";
import FormulasSelectedCategoryList from "./FormulasSelectedCategoryList.jsx";
// Don't remove, it would be needed in future
// import FormulasFilterButtonGroup from './FormulasFilterButtonGroup';

export default function FormulasFilterHeader({
    // perPage,
    // totalCount,
    // isMyFormula,
    // onChangeFormulaSelector,
    formulaCategoryClear,
    formulaCategoryApply,
    formulaCategoryRemove,
    filterCategorieIds,
    categories,
    onAddFormulaClick,
    formulaSearch,
    filterSearch,
    // onDownloadProjects,
    // onChangeFilter,
    contextRef,
    loggedInUserRole,
    // filterButtonModel,
}) {
    return (
        <div className="iauto-formulas-header">
            {loggedInUserRole === "Admin" ? (
                <Segment clearing basic></Segment>
            ) : (
                <Segment clearing basic>
                    <FormulasHeader
                        //content={isMyFormula ? "MY FORMULAS" : "ALL FORMULAS"}
                        content={"MY FORMULAS"}
                    />
                    {/* <Inline floated="right">
                        {isMyFormula ? (
                            <ModusButton
                                filled
                                onClick={() => onChangeFormulaSelector(false)}
                                content="GO TO ALL FORMULAS"
                            />
                        ) : (
                            <ModusButton
                                filled
                                onClick={() => onChangeFormulaSelector(true)}
                                content="GO TO MY FORMULAS"
                            />
                        )}
                    </Inline> */}
                </Segment>
            )}
            <Divider />
            <FormulasActionFilters
                categories={categories}
                filterCategorieIds={filterCategorieIds}
                formulaCategoryApply={formulaCategoryApply}
                //isMyFormula={isMyFormula}
                onAddFormulaClick={onAddFormulaClick}
                //onDownloadProjects={onDownloadProjects}
                contextRef={contextRef}
            />
            {/* <FormulasSelectedCategoryList
                categories={categories}
                filterCategorieIds={filterCategorieIds}
                formulaCategoryRemove={formulaCategoryRemove}
                formulaCategoryClear={formulaCategoryClear}
            /> */}

            {/* <FormulasFilterButtonGroup
                className="iauto-formulas-btn_grp"
                onChangeFilter={onChangeFilter}
                filterButtonModel={filterButtonModel}
            /> */}

            <Input
                className="iauto-formulas-header_search"
                iconPosition="left"
                icon="search"
                placeholder="Type here to search for a formula"
                onChange={formulaSearch}
                value={filterSearch}
            />
        </div>
    );
}
