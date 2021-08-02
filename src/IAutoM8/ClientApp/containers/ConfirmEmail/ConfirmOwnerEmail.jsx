import React, { Component } from 'react'
import { connect } from 'react-redux'
import { withRouter } from 'react-router-dom'
import { push } from 'react-router-redux'
import { error } from 'react-notification-system-redux'

import { confirmEmail, loadPermissions } from '@store/auth'

@withRouter
@connect(
    state => ({
        isLoggedIn: state.auth.loggedIn,
        isLoading: state.auth.loading
    }),
    {
        confirmEmail, pushState: push, loadPermissions,
        error
    }
)
export default class ConfirmOwnerEmail extends Component {

    componentWillMount() {
        if (this.props.isLoggedIn) {
            this.props.pushState('/');
        }
        else {
            const query = new URLSearchParams(this.props.location.search);
            const code = encodeURIComponent(query.get('code'));
            const email = encodeURIComponent(query.get('email'));
            this.props.confirmEmail({ code, email })
                .catch((e) => {
                    this.props.error({
                        title: e.data.message
                    });
                    this.props.pushState('/')
                });
        }
    }

    componentWillReceiveProps(nextProps) {
        // if the user logged in, redirect the user.
        if (!this.props.isLoggedIn && nextProps.isLoggedIn) {
            this.props.loadPermissions()
                .then(() => this.props.pushState('/'))
        }
    }
    render() {
        return (
            <div>
            </div>
        );
    }
}
