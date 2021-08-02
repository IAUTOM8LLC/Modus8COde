import React, { Component } from 'react'
import { reduxForm } from 'redux-form'
import { Modal, Form } from 'semantic-ui-react'

import { NotificationSettingsListInput, ModusButton } from '@components'

import editModalHoc from '../../common/editModalHoc'

@editModalHoc
@reduxForm({
    form: 'userNotificationSettigns',
    enableReinitialize: true
})
export default class UserNotificationSettingsModal extends Component {

    render() {
        const { open, loading, onClose, handleSubmit, fullName } = this.props;

        return (
            <Modal
                as="form"
                size="small"
                open={open}
                onSubmit={handleSubmit}
                className="task-details-modal"
                onClose={onClose}
            >
                <Modal.Header>
                    {fullName || "User's"} notification settings
                </Modal.Header>

                <Modal.Content>
                    <Form as="section">
                        <NotificationSettingsListInput
                            name="notificationSettings"
                        />
                    </Form>
                </Modal.Content>

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
