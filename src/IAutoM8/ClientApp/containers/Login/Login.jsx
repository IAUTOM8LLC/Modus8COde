import React, { Component } from 'react'
import { connect } from 'react-redux'
import { reset } from 'redux-form'
import { withRouter } from 'react-router-dom'
import { push } from 'react-router-redux';
import { error as nsError } from 'react-notification-system-redux'
import Cookies from 'js-cookie'

import { login, loadPermissions } from '@store/auth'
import { loadData } from '@store/infusionsoft'

import { LoginLayout } from '@components'

import LoginForm from './components/LoginForm'

import './Login.less';

const REMEMBER_ME = 'REMEMBER_ME';

@withRouter
@connect(
    state => ({
        isLoggedIn: state.auth.loggedIn,
        isLoading: state.auth.loading,
        error: state.auth.error,
        orderFormUrl: state.infusionsoft.orderFormUrl,
        affiliateCookieUrl: state.infusionsoft.affiliateCookieUrl
    }),
    {
        login, pushState: push, reset, nsError, loadPermissions,
        loadData
    }
)
export default class Login extends Component {

    state = {
        isLoading: false,
        src: ""
    }

    componentWillMount() {
        if (this.props.isLoggedIn) {
            this.props.pushState('/');
        }
        else {
            this.props.loadData();
        }
    }

    componentWillReceiveProps(nextProps) {
        // if the user logged in, redirect the user.
        if (!this.props.isLoggedIn && nextProps.isLoggedIn) {
            this.props.loadPermissions().then(() => {
                if (this.props.location.search) {
                    const query = new URLSearchParams(this.props.location.search);
                    const returnUrl = query.get('returnUrl');
                    this.props.pushState(returnUrl);
                } else {
                    this.props.pushState('/');
                }
            });
        }

        if (!this.props.error && nextProps.error) {
            this.props.nsError({
                title: 'Login failed!',
                message: nextProps.error.message
            });
        }
    }
    handleSubmit = (state) => {
        if (this.props.isLoading)
            return null;

        return this.props.login(state)
            .catch(() => this.props.reset('login'));
    }
    render() {
        const remember = Boolean(Cookies.get(REMEMBER_ME));
        const initialValues = __DEV__
            ? { email: 'antonf@byteant.com', password: 'Aa@12345', remember }
            : { remember };

        return (
            <LoginLayout
                header="Login to your account"
                linkHeader="Don't have an account ?"
                linkLabel="Sign Up"
                linkTo={this.props.orderFormUrl}
                mainForm={
                    <LoginForm
                        initialValues={initialValues}
                        onSubmit={this.handleSubmit}
                    />}
            />
        );
    }
}
