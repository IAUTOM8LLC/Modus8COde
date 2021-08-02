import React from 'react'
import { reduxForm } from 'redux-form'
import { Form, Segment } from 'semantic-ui-react'
import { Link } from 'react-router-dom'

import { email } from '@utils/validators'

import { TextInput, CheckboxInput, ModusButton } from '@components'

const LoginForm = ({ handleSubmit, submitting }) => (
    <Form
        size="large"
        onSubmit={handleSubmit}
    >
        <TextInput
            required
            validate={[email]}
            label="Email Address"
            placeholder="E-mail address"
            name="email"
        />
        <TextInput
            required
            placeholder="Password"
            label="Password"
            type="password"
            name="password"
        />
        <Segment basic clearing floated="left" style={{ padding: 0 }}>
            <CheckboxInput name="remember" label="Remember me" />
        </Segment>
        <Segment basic clearing floated="right" style={{ padding: 0 }}>
            <Link to="/forgot-password">Forgot password ?</Link>
        </Segment>

        <ModusButton
            fluid
            filled
            type="submit"
            content="Log in"
            loading={submitting}
        />
    </Form>
);

export default reduxForm({
    form: 'login'
})(LoginForm)
