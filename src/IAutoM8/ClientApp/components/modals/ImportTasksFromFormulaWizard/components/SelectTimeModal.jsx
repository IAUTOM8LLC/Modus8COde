import React, { Component } from 'react'
import { connect } from 'react-redux'
import { reduxForm } from 'redux-form'
import { Form, Modal } from 'semantic-ui-react'
import moment from 'moment'

import { required, onlyFutureDate } from '@utils/validators'

import { ModusButton, DateTimeInput } from '@components'

@connect(() => ({
    initialValues: {
        projectStartDateTime: moment().add(5, 'm').format('MMM D, YYYY h:mm A')
    }
}))
@reduxForm({
    form: 'selectProjectStartDateForm',
    enableReinitialize: true
})
export default class SelectTimeModal extends Component {
    render() {
        const {
            open,
            onClose,
            onBack,
            handleSubmit,
            loading,
            header,
        } = this.props;

        return (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="tab-modal"
                size="small"
                onClose={onClose}
            >
                <Modal.Header>{header}</Modal.Header>

                <Modal.Content>
                    <Form as="section">
                        <DateTimeInput
                            name="projectStartDateTime"
                            validate={[required, onlyFutureDate]}
                        />
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        className="button-flex-order1"
                        filled content="Add formula" type="submit" loading={loading} />
                    <ModusButton
                        className="button-flex-order2"
                        grey type="button" content="Cancel" onClick={onClose} />
                    <ModusButton
                        className="button-flex-order3"
                        grey content="Back" floated="left" onClick={onBack} />
                </Modal.Actions>
            </Modal>
        );
    }
}
