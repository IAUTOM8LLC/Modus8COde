import React, { Component } from 'react'
import { reduxForm } from 'redux-form'
import { Modal, Form, Tab } from 'semantic-ui-react'

import { NotificationSettingsListInput, ModusButton } from '@components'

import editModalHoc from '../../common/editModalHoc'

@editModalHoc
@reduxForm({
    form: 'roleNotificationSettigns',
    enableReinitialize: true
})
export default class RoleNotificationSettingsModal extends Component {

    render() {
        const { open, loading, onClose, handleSubmit } = this.props;

        const panes = [
            {
                menuItem: { key: 'owner', content: 'Owner' },
                pane: { key: 'owner', content: <NotificationSettingsListInput name="owner" /> }
            },
            {
                menuItem: { key: 'manager', content: 'Manager' },
                pane: { key: 'manager', content: <NotificationSettingsListInput name="manager" /> }
            },
            {
                menuItem: { key: 'worker', content: 'Worker' },
                pane: { key: 'worker', content: <NotificationSettingsListInput name="worker" /> }
            },
        ];

        const menuOptions = { secondary: true, pointing: true };

        return (
            <Modal
                as="form"
                size="small"
                open={open}
                onSubmit={handleSubmit}
                className="task-details-modal tab-modal"
                onClose={onClose}
            >
                <Modal.Header>
                    Role notification settings
                </Modal.Header>

                <Form as="section">
                    <Tab
                        panes={panes}
                        renderActiveOnly={false}
                        menu={menuOptions}
                    />
                </Form>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        className="button-flex-order1"
                        filled content="Save" loading={loading} />
                    <ModusButton
                        className="button-flex-order2"
                        grey type="button" content="Close" onClick={onClose} />
                </Modal.Actions>
            </Modal>
        );
    }
}
