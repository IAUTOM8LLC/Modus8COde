import React, { Component } from 'react'
import { reduxForm } from 'redux-form'
import { Modal, Form } from 'semantic-ui-react'

import { required, alphaNumeric, minUsersNumber } from '@utils/validators'

import {
    TextInput,
    DropdownInput,
    WhenPermitted,
    ModusButton
} from '@components'

import editModalHoc from '../../common/editModalHoc'

const validate = ({ processingUserId }) => ({
    processingUserId: required(processingUserId)
})

@editModalHoc
@reduxForm({
    form: 'processingUserModal',
    enableReinitialize: true,
    validate
})
export default class ProcessingUserModal extends Component {
    state = {
        filteredUsers: []
    }

    componentDidUpdate(prevProps) {
        if (prevProps !== this.props) {
            const { users, initialValues } = this.props;
            if (initialValues !== undefined) {
                const filterUsers = users
                    .filter(t => initialValues.assignedUsers.some(x => x.userId === t.id)).map(u => ({
                        key: u.id,
                        value: u.id,
                        text: `${u.fullName} (${u.userName})`
                    }));

                this.setState({
                    filteredUsers: [...filterUsers]
                });
            }
        }
    }

    render() {
        const { open, loading, onClose, handleSubmit, users, initialValues } = this.props;

        return (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="task-details-modal"
                size="small"
                onClose={onClose}
            >
                <Modal.Header>
                    Change processing user
                </Modal.Header>

                <Modal.Content>
                    <Form as="section">
                        <TextInput
                            disabled
                            fluid
                            name="skillName"
                            label="Skill"
                        />
                        <DropdownInput
                            fluid
                            name="processingUserId"
                            label="Processing User"
                            options={this.state.filteredUsers}
                            validate={[required, minUsersNumber(1)]}
                        />
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        className="button-flex-order1"
                        filled
                        content="Save"
                        type="submit"
                        loading={loading}
                    />
                    <ModusButton
                        className="button-flex-order2"
                        grey type="button" content="Close" onClick={onClose} />
                </Modal.Actions>
            </Modal>
        );
    }
}
