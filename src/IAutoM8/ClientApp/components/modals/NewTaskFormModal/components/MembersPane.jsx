import React from 'react'
import { Header, Icon } from 'semantic-ui-react'
import { Field } from 'redux-form'

export default function MembersPane({ users, onChange }) {
    return (
        <div className="project-details-modal__pane">
            <Header as="h2" icon textAlign="center">
                <Icon name="bug" circular />
                Coming soon
            </Header>
        </div>
    );
}
