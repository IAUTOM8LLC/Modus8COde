import React from 'react'
import { Form } from 'semantic-ui-react'

import { TextInput, DropdownInput, WhenPermitted } from '@components'

export default function InformationPane({ users, clients }) {
    return (
        <div className="project-details-modal__pane">
            <Form as="section">
                <TextInput
                    required
                    name="name"
                    label="Project name"
                />

                {/* <TextInput
                    name="description"
                    label="Description"
                /> */}

                <WhenPermitted rule="setProjectClient">
                    {
                        (permitted) =>
                            <DropdownInput
                                selection
                                disabled={!permitted}
                                label="Project client"
                                name="clientId"
                                options={clients}
                            />
                    }
                </WhenPermitted>

                <WhenPermitted rule="setProjectMembers">
                    <DropdownInput
                        multiple
                        label="Project managers"
                        name="managers"
                        options={users}
                    />
                </WhenPermitted>
            </Form>
        </div>
    );
}
