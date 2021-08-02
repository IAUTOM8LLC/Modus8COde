import React, { Component } from 'react'
import { connect } from 'react-redux'

import { loadTasksHistory, loadTasksHistoryForAllProjects } from '@store/projectTasks'

import { ALL_PROJECTS_ID } from '@constants/projectCostants'

import { ProjectTasksHeader } from '@components'

import TasksFeed from './components/TasksFeed'

@connect(
    state => ({
        user: state.auth.user,
        history: state.projectTasks.tasksHistory
    }),
    {
        loadTasksHistory,
        loadTasksHistoryForAllProjects
    }
)
export default class ProjectTasksFeed extends Component {

    componentDidMount() {
        if (this.props.projectId === ALL_PROJECTS_ID) {
            this.props.loadTasksHistoryForAllProjects();
        } else {
            this.props.loadTasksHistory(this.props.projectId);
        }
    }

    render() {
        const {
            tasks,
            match,
            projectName,
            projectId,
            overdueCount,
            onShowResources
        } = this.props;

        return (
            <div>
                <ProjectTasksHeader
                    projectId={projectId}
                    path={match.path}
                    name={projectName}
                    taskCount={tasks.length}
                    overdueCount={overdueCount}
                    onShowResources={onShowResources}
                />

                <TasksFeed history={this.props.history} />
            </div>
        );
    }
}
