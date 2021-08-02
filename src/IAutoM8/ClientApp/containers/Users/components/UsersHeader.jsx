import React from 'react'
import { Header } from 'semantic-ui-react'

import { SimpleSegment, ModusButton, Inline } from '@components'

export default function UsersHeader({ onEditRoleNotififcationSettings }) {
    return (
        <SimpleSegment clearing className="iauto-projects__header">
            <Header as="h2" size="large" floated="left">
                Users
            </Header>

            <Inline floated="right">
                <ModusButton
                    circular
                    grey
                    size="small"
                    popup="Role notifications"
                    icon="iauto--notification"
                    whenPermitted="editRoleNotificationSettings"
                    onClick={onEditRoleNotififcationSettings}
                />
            </Inline>
        </SimpleSegment>
    );
}
