import React, { Component } from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux';
import { withRouter } from 'react-router-dom'

import { confirmEmail, pushUrlParams, loadPermissions } from '@store/auth'
import { error } from 'react-notification-system-redux'

import { LoginLayout } from '@components'

import ConfirmEmailForm from './components/ConfirmEmailForm'

@withRouter
@connect(
    state => ({
        isLoggedIn: state.auth.loggedIn,
        isLoading: state.auth.loading,
        urlParams: state.auth.urlParams
    }),
    {
        confirmEmail, pushState: push, pushUrlParams, loadPermissions,
        error
    }
)
export default class ConfirmEmail extends Component {

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
                .then(() => this.props.pushState('/'))
        }
    }

    handleSubmit = (state) => {
        if (this.props.isLoading)
            return;

        return this.props.confirmEmail(state)
            .catch(res => this.props.error({
                title: res.data.message
            }));
    }

    render() {
        const params = this.props.urlParams;
        return (
            <LoginLayout
                header="Please enter password for your account"
                linkHeader="Have an account ?"
                linkLabel="Log In"
                linkTo="/login"
                mainForm={
                    params.email !== "" && <ConfirmEmailForm
                        initialValues={{ ...params }}
                        onSubmit={this.handleSubmit}
                    />
                }
            />
        );
    }
}
