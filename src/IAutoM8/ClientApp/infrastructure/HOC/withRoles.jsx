import React, { Component } from 'react'
import { withRouter } from 'react-router-dom'
import { connect } from 'react-redux'
import { push } from 'react-router-redux'
import { error } from 'react-notification-system-redux'

const withRoles = (allowedRoles) => (WrappedComponent) => {

    @connect(
        state => ({ roles: state.auth.user.roles }),
        { push, error }
    )
    class WithRoleGuard extends Component {

        hasAccess = () => {
            const roles = this.props.roles || [];
            const role = roles[0];

            return allowedRoles.indexOf(role) !== -1;
        }

        componentWillMount() {
            if (!this.hasAccess()) {
                this.props.error({ title: 'Access denied' });
                this.props.history.replace('/');
            }
        }

        render() {
            if (this.hasAccess()) {
                return <WrappedComponent {...this.props} />
            }

            return null;
        }
    }

    return withRouter(WithRoleGuard);
}

export default withRoles;
