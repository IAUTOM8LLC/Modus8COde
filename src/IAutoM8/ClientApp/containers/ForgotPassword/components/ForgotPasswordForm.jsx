import React from 'react'
import { reduxForm } from 'redux-form'
import { Form } from 'semantic-ui-react'

import { email } from '@utils/validators'

import { TextInput, ModusButton } from '@components'

const ForgotPasswordForm = ({ handleSubmit, submitting }) => (
    <Form
        size="large"
        onSubmit={handleSubmit}
    >
        <TextInput
            required
            validate={[email]}
            label="Email address"
            placeholder="Email address"
            name="email"
        />

        <ModusButton
            fluid
            filled
            type="submit"
            content="Submit"
            loading={submitting}
        />
    </Form>
);

export default reduxForm({
    form: 'forgotpassword',
    enableReinitialize: true
})(ForgotPasswordForm)
