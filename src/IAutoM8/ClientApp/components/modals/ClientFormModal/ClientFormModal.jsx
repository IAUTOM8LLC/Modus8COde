import React, { Component } from 'react'
import { reduxForm } from 'redux-form'
import { Modal, Form } from 'semantic-ui-react'

import { required, alphaNumeric, maxLength, email } from '@utils/validators'

import { TextInput, WhenPermitted, ModusButton } from '@components'

import editModalHoc from '../../common/editModalHoc'

const validate = ({ companyName, firstName, lastName, email }) => ({
    companyName: required(companyName),
    firstName: required(firstName),
    lastName: required(lastName),
    email: required(email)
})

@editModalHoc
@reduxForm({
    form: 'clientFormModal',
    enableReinitialize: true,
    validate
})
export default class ClientFormModal extends Component {

    render() {
        const { open, loading, onClose, handleSubmit } = this.props;

        return (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="task-details-modal"
                onClose={onClose}
            >
                <Modal.Header>
                    Client details
                </Modal.Header>

                <Modal.Content>
                    <Form as="section">
                        <TextInput
                            fluid
                            required
                            name="companyName"
                            label="Company name"
                            validate={[alphaNumeric, maxLength(250)]}
                        />
                        <TextInput
                            fluid
                            required
                            name="firstName"
                            label="First name"
                            validate={[alphaNumeric, maxLength(50)]}
                        />
                        <TextInput
                            fluid
                            required
                            name="lastName"
                            label="Last name"
                            validate={[alphaNumeric, maxLength(50)]}
                        />
                        <TextInput
                            fluid
                            required
                            name="email"
                            label="Email"
                            validate={[email, maxLength(250)]}
                        />
                        <TextInput
                            fluid
                            name="country"
                            label="Country"
                            validate={[alphaNumeric, maxLength(50)]}
                        />
                        <TextInput
                            fluid
                            name="state"
                            label="State"
                            validate={[alphaNumeric, maxLength(50)]}
                        />
                        <TextInput
                            fluid
                            name="city"
                            label="City"
                            validate={[alphaNumeric, maxLength(50)]}
                        />
                        <TextInput
                            fluid
                            name="streetAddress1"
                            label="Street Address 1"
                            validate={[alphaNumeric, maxLength(100)]}
                        />
                        <TextInput
                            fluid
                            name="streetAddress2"
                            label="Street Address 2"
                            validate={[alphaNumeric, maxLength(100)]}
                        />
                        <TextInput
                            fluid
                            name="zip"
                            label="ZIP/Postal code"
                            validate={[alphaNumeric, maxLength(10)]}
                        />
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <WhenPermitted rule="editClient">
                        <ModusButton
                            className="button-flex-order1"
                            filled
                            content="Save"
                            loading={loading}
                        />
                    </WhenPermitted>
                    <ModusButton
                        className="button-flex-order2"
                        grey type="button" content="Close" onClick={onClose} />
                </Modal.Actions>
            </Modal>
        );
    }
}
