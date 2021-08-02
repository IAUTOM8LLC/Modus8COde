import React, { Component } from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux'
import { Dimmer, Loader } from 'semantic-ui-react'

@connect(
    state => ({
        isLoggedIn: state.auth.loggedIn
    }),
    {
        pushState: push
    }
)
export default class StartWork extends Component {

    componentDidMount() {
        const { isLoggedIn, pushState,
            match:
            {
                params: {
                    projectId,
                    taskId
                }
            }
        } = this.props;
        if (isLoggedIn) {
            pushState(`/projects/${projectId}/task/${taskId}`);
        }
        else {
            pushState('/');
        }
    }
    render() {
        return (
            <Dimmer
                page
            >
                <Loader>Start work!</Loader>
            </Dimmer>
        );
    }
}
