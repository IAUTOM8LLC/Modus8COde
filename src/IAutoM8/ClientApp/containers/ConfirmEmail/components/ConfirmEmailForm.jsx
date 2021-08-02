import React, { Component } from 'react'
import { reduxForm } from 'redux-form'
import { Form } from 'semantic-ui-react'

import { passwordsMatch, passwordRequirements } from '@utils/validators'

import { TextInput, ModusButton } from '@components'

const validate = ({ password, confirmPassword }) => {
    return {
        password: passwordRequirements(password),
        confirmPassword: passwordsMatch(password)(confirmPassword)
    }
}

@reduxForm({
    form: 'confirm',
    validate,
    enableReinitialize: true
})
export default class ConfirmEmailForm extends Component {
    componentWillMount() {
        this.props.initialize();
    }

    render() {
        const { handleSubmit, submitting } = this.props;

        return (
            <Form
                size="large"
                onSubmit={handleSubmit}
            >
                <TextInput
                    required
                    placeholder="Password"
                    label="Password"
                    type="password"
                    name="password"
                />
                <TextInput
                    required
                    placeholder="Confirm Password"
                    label="Confirm Password"
                    type="password"
                    name="confirmPassword"
                />

                <ModusButton
                    fluid
                    filled
                    type="submit"
                    content="Login"
                    loading={submitting}
                />
            </Form>
        );
    }
}
