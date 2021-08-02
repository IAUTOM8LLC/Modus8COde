import React, { Component } from 'react'
import { reduxForm } from 'redux-form'
import { Form } from 'semantic-ui-react'

import { email, companyname, fullName, passwordsMatch, passwordRequirements } from '@utils/validators'

import { TextInput, ModusButton } from '@components'

const validate = ({ password, passwordConfirm }) => {
    return {
        password: passwordRequirements(password),
        passwordConfirm: passwordsMatch(password)(passwordConfirm)
    }
}

@reduxForm({
    form: 'signup',
    validate,
    enableReinitialize: true
})
export default class SignupForm extends Component {
    componentWillMount() {
        this.props.initialize();
    }

    constructor(props) {
        super(props);
        this.state = { inputs: ['input-0'] };
    }

    appendInput() {
        const newInput = `input-${this.state.inputs.length}`;
        this.setState(prevState => ({ inputs: prevState.inputs.concat([newInput]) }));
    }

    render() {
        const { handleSubmit, submitting, showEmailDiv } = this.props;
        return (
            <Form
                size="large"
                onSubmit={handleSubmit}
            >

                <div className="profileDetail" style={{ float: "left" }}>
                    {
                        this.props.cVendor &&
                        <TextInput
                            required
                            // validate={[companyname]}
                            placeholder="Company Name"
                            label="Company Name"
                            name="companyname"
                        />
                    }
                    <TextInput
                        required
                        validate={[fullName]}
                        placeholder="Full name"
                        label="Full name"
                        name="fullName"
                    />
                    <TextInput
                        required
                        validate={[email]}
                        placeholder="Email address"
                        label="Email address"
                        name="email"
                    />
                    <TextInput
                        required
                        label="Password"
                        placeholder="Password"
                        type="password"
                        name="password"
                    />
                    <TextInput
                        required
                        label="Confirm Password"
                        placeholder="Confirm Password"
                        type="password"
                        name="passwordConfirm"
                    />
                    <input type="hidden" name="orderId" />
                </div>
                {showEmailDiv && showEmailDiv == true &&
                    <div className="profileDetail" style={{ float: "right" }}>
                        <div id="dynamicInput">
                            <TextInput
                                placeholder="Email"
                                label="Email"
                                name="Email"
                                required
                            />

                            <TextInput
                                placeholder="Email"
                                label="Email"
                                name="Email"
                                required
                            />

                            {this.state.inputs.map(input =>
                                <TextInput
                                    key={input}
                                    required
                                    validate={[fullName]}
                                    placeholder="Email"
                                    label="Email"
                                    name="Email"
                                />
                            )}
                        </div>
                        <div style={{ display: "flex", justifyContent: "center" }}>
                            <button className="ui fluid button iauto"
                                type="button"
                                style={{ marginTop: "20px", marginBottom: "15px", width: "80px" }}
                                onClick={() => this.appendInput()}>
                                ADD
                        </button>
                        </div>
                    </div>
                }
                <ModusButton
                    style={{ fontSize: "15px" }}
                    fluid
                    filled
                    type="submit"
                    content="Sign up"
                    loading={submitting}
                />
            </Form>
        )
    }
}
