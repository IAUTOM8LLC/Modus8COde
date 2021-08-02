import React, { Component } from 'react'
import axios from 'axios'
import { connect } from 'react-redux'
import { push } from 'react-router-redux'
import { error } from 'react-notification-system-redux'
import { Dimmer, Loader } from 'semantic-ui-react'

import { getAuthHeaders } from '@infrastructure/auth'

@connect(
    state => ({
        userRole: state.auth.user.roles[0]
    }),
    { pushState: push, error }
)
export default class Home extends Component {

    componentDidMount() {
        this.props.userRole === "Admin"
            ? this.props.pushState('/admin')
            : this.props.pushState('/projects/getTasksFromAllProjects/listView');
    }

    render() {
        const style = {
            'minHeight': 'calc(100vh - 100px)'
        }

        return (
            <Dimmer active inverted style={style}>
                <Loader size="large">Loading recent project</Loader>
            </Dimmer>
        )
    }
}
