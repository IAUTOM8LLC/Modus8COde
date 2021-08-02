import React from 'react'
import { Message, Icon, Form } from 'semantic-ui-react'

import { CheckboxInput, SimpleSegment } from '@components'

export default function IsAutomatedInput({ isAutomated = false, isConditional, disabled }) {
    if (isConditional)
        return null;

    return (
        <Form.Field >
            <SimpleSegment>
                <CheckboxInput
                    name="isAutomated"
                    disabled={disabled}
                    label="Is Automated"
                />
            </SimpleSegment>
            {
                isAutomated &&
                <Message info>
                    <Icon name="info" />
                    Automated task will be finished by the system once the duration time has elapsed
                </Message>
            }
        </Form.Field>
    );
}