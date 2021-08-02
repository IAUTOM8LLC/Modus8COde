import React from 'react'
import { Segment, Input, Checkbox, Form } from 'semantic-ui-react'

import { ModusButton, Inline } from '@components'

import FormulaShareLink from '../FormulaShareLink'

export default function SharePublicLink({ formulaId, shareStatus, onResharingChange }) {
    return (
        <Segment>
            <h4 className="formula-sharing__link-label">Your formula sharing link is:</h4>
            <Inline className="formula-sharing__link">
                <Input
                    readOnly
                    type="text"
                    defaultValue={`${document.location.origin}/formulas/share/${formulaId}`}
                />
                <FormulaShareLink formulaId={formulaId}>
                    <ModusButton
                        wide
                        filled
                        content="Copy"
                        type="button"
                    />
                </FormulaShareLink >
            </Inline>

            <Form.Field style={{ marginTop: '40px' }}>
                <Checkbox
                    checked={shareStatus.isResharingAllowed}
                    label="Allow formula re-sharing"
                    onChange={onResharingChange}
                />
            </Form.Field>
        </Segment>
    );
}
