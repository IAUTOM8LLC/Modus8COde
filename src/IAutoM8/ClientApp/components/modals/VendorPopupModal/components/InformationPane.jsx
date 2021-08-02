import React from 'react'
import { Form } from 'semantic-ui-react'

import { TextInput, RichEditor } from '@components'

export default function InformationPane() {
    return (
        <div className="formula-details-modal__pane">
            <Form as="section">
                <TextInput
                    readOnly
                    required
                    name="title"
                    label="Formula name"
                />

                <RichEditor
                    readOnly
                    label="Description"
                    name="description"
                    placeholder="Type description"
                    withoutToolbar
                />
            </Form>
        </div>
    );
}
