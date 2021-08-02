import React, { Component } from 'react'
import { withRouter } from 'react-router-dom'
import { connect } from 'react-redux'

export default function withAuth(BaseComponent) {

    @connect(
        state => ({ isLoggedIn: state.auth.loggedIn })
    )
    class Restricted extends Component {

        componentWillMount() {
            this.checkAuthentication(this.props);
        }

        componentWillReceiveProps(nextProps) {
            if (nextProps.location !== this.props.location || nextProps.isLoggedIn === false) {
                this.checkAuthentication(nextProps);
            }
        }

        checkAuthentication({ history, location, isLoggedIn }) {
            if (isLoggedIn === false)
                history.replace(
                    `/login?returnUrl=${encodeURIComponent(location.pathname + location.search)}`
                );
        }

        render() {
            if (this.props.isLoggedIn) {
                return <BaseComponent {...this.props} />
            }

            return null;
        }
    }

    return withRouter(Restricted);
}