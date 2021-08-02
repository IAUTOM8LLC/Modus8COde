import React, { Component } from 'react'
import { connect } from 'react-redux'
import { success, error as nsError } from 'react-notification-system-redux'

import { forgotPassword } from '@store/auth'

import { LoginLayout } from '@components'

import ForgotPasswordForm from './components/ForgotPasswordForm'

@connect(
    state => ({
        isLoading: state.auth.loading
    }),
    { forgotPassword, success, nsError }
)
export default class ForgotPassword extends Component {

    handleSubmit = (state) => {
        if (this.props.isLoading)
            return;

        return this.props.forgotPassword(state)
            .then(() => this.props.success({
                title: 'Your password is restored',
                message: 'Please, check your email and follow a confirmation link'
            }))
            .catch(error => this.props.nsError({
                title: 'Password restore failed',
                message: error.data.message
            }));
    }

    render() {
        return (
            <LoginLayout
                header="Recover your password"
                linkHeader="Don't have an account ?"
                linkLabel="Sign Up"
                linkTo="/signup"
                mainForm={
                    <ForgotPasswordForm onSubmit={this.handleSubmit} />
                }
            />
        );
    }
}
