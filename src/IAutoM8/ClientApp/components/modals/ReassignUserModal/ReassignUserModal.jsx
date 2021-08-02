import React from 'react';
import { reduxForm } from 'redux-form';
import { Modal, Form } from 'semantic-ui-react';

import { required, minUsersNumber } from '@utils/validators';

import {
    DropdownInput,
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
export default class ReassignUserModal extends React.Component {
    state = {
        filteredUsers: []
    };

    componentDidUpdate(prevProps) {
        if (prevProps !== this.props) {
            const { initialValues } = this.props;
            if (initialValues !== undefined) {
                const filterUsers = initialValues.assignedUsers
                .map(u => ({
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
        const { open, onClose } = this.props;

        return(
            <Modal
                as="form"
                open={open}
                //onSubmit={handleSubmit}
                className="task-details-modal"
                size="small"
                onClose={onClose}
            >
                <Modal.Header>
                    Change processing user
                </Modal.Header>

                <Modal.Content>
                    <Form as="section">
                        <DropdownInput
                            fluid
                            name="processingUserId"
                            label="Processin User"
                            options={this.state.filteredUsers}
                            validate={[required, minUsersNumber(1)]}
                        />
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        filled
                        className="button-flex-order1"
                        content="Save"
                        type="submit"
                        //loading={loading}
                    />
                    <ModusButton
                        grey
                        className="button-flex-order2" 
                        type="button" 
                        content="Close" 
                        onClick={onClose} 
                    />
                </Modal.Actions>
            </Modal>
        );
    }
}