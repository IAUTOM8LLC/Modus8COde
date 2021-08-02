import React from 'react'
import { Segment, Button } from 'semantic-ui-react'

import {
    ModusButton, SimpleSegment
} from '@components'

import './../Formulas.less'

const FormulasFilterButtonGroup = ({ 
    filterButtonModel: { 
        isAllEnabled,
        isTracksEnabled,
        isFormulasEnabled
    }, 
    onChangeFilter 
}) => {
    return (
        <Segment clearing basic>
            <SimpleSegment clearing floated="left">
                <Button.Group>
                    <ModusButton
                        filled={isAllEnabled}
                        grey={!isAllEnabled}
                        content="ALL"
                        onClick={() => onChangeFilter("all")}
                    />
                    <ModusButton
                        filled={isTracksEnabled}
                        grey={!isTracksEnabled}
                        content="TRACKS"
                        onClick={() => onChangeFilter("tracks")}
                    />
                    <ModusButton
                        filled={isFormulasEnabled}
                        grey={!isFormulasEnabled}
                        content="FORMULAS"
                        onClick={() => onChangeFilter("formulas")}
                    />
                </Button.Group>
            </SimpleSegment>
        </Segment>
    );
};

export default FormulasFilterButtonGroup;