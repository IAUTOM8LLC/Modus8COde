import React, { Component } from 'react'
import { Form } from 'semantic-ui-react'

import { DurationInput } from '@components'

import IsAutomatedInput from './IsAutomatedInput'

export default class FormulaSchedulePane extends Component {

    render() {
        const { isConditional, isAutomated } = this.props;

        return (
            <div className="project-details-modal__pane">
                <Form as="section" >
                    <DurationInput
                        label="Duration"
                        name="duration"
                        min={1}
                    />
                    <DurationInput
                        name="startDelay"
                        label="Start delay"
                    />
                    <IsAutomatedInput
                        isAutomated={isAutomated}
                        isConditional={isConditional}
                    />
                </Form>
            </div>
        );
    }
}
