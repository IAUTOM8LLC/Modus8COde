import React from 'react'
import { Form } from 'semantic-ui-react'

import { DurationInput, TextInput, SimpleSegment } from '@components'

export default function IntervalPane({ canEdit }) {
    return (
        <div className="project-details-modal__pane">
            <Form as="section" >
                <SimpleSegment>
                    <TextInput
                        required
                        name="title"
                        label="Name"
                    />
                    <DurationInput
                        disabled={!canEdit}
                        label="Duration"
                        name="duration"
                        min={1}
                    />
                </SimpleSegment>
            </Form>
        </div>
    );

}
