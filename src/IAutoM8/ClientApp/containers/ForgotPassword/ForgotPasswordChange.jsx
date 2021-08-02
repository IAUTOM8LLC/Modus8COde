import React, { Component } from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux'
import { withRouter } from 'react-router-dom'
import { error as nsError } from 'react-notification-system-redux'

import {
    forgotPasswordChange,
    pushUrlParams,
    loadPermissions
} from '@store/auth'

import { LoginLayout } from '@components'

import ForgotPasswordChangeForm from './components/ForgotPasswordChangeForm'

@withRouter
@connect(
    state => ({
        isLoggedIn: state.auth.loggedIn,
        isLoading: state.auth.loading,
        urlParams: state.auth.urlParams
    }),
    {
        forgotPasswordChange,
        pushState: push,
        pushUrlParams: pushUrlParams,
        loadPermissions,
        nsError
    }
)
export default class ForgotPasswordChange extends Component {

    componentWillMount() {
        if (this.props.isLoggedIn) {
            this.props.pushState('/');
        }
        else {
            const query = new URLSearchParams(this.props.location.search);
            const code = encodeURIComponent(query.get('code'));
            const email = encodeURIComponent(query.get('email'));
            this.props.pushUrlParams({ code, email });
        }
    }

    componentWillReceiveProps(nextProps) {
        // if the user logged in, redirect the user.
        if (!this.props.isLoggedIn && nextProps.isLoggedIn) {
            this.props.loadPermissions()
                .then(() => this.props.pushState('/'));
        }
    }

    handleSubmit = (state) => {
        if (this.props.isLoading)
            return;
        return this.props.forgotPasswordChange(state)
            .catch(error => this.props.nsError({
                title: 'Password restore failed',
                message: error.data.message
            }));
    }

    render() {
        const params = this.props.urlParams;
        return (
            <LoginLayout
                header="Recover your password"
                linkHeader="Don't have an account ?"
                linkLabel="Sign Up"
                linkTo="/signup"
                mainForm={
                    params.email !== "" && <ForgotPasswordChangeForm
                        initialValues={{ ...params }}
                        onSubmit={this.handleSubmit}
                    />
                }
            />
        );
    }
}
