import React from 'react'
import { Segment, Button } from 'semantic-ui-react'

import {
    ModusButton, SimpleSegment
} from '@components'

import './../Formulas.less'

export default function FormulasSelectedCategoryList({ categories, filterCategorieIds,
    formulaCategoryRemove,
    formulaCategoryClear }) {

    return (
        <Segment clearing basic>
            <SimpleSegment clearing floated="left">
                {
                    categories.map(category =>
                        filterCategorieIds.some(id => id === category.id)
                            ? <ModusButton
                                key={category.id}
                                icon="iauto--remove"
                                className="iauto-formulas-header_filter"
                                content={category.name}
                                onClick={() => formulaCategoryRemove(category.id)}
                                />
                            : null)
                }
                {
                    filterCategorieIds.length !== 0
                        ? <ModusButton
                            className="iauto-formulas-header_clear-filters"
                            content="clear all filters"
                            onClick={formulaCategoryClear}
                            />
                        : null
                }
            </SimpleSegment>
        </Segment>
    );
}
