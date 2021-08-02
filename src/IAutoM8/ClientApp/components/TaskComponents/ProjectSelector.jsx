import React, { Component } from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux'

import { ALL_PROJECTS_ID } from '@constants/projectCostants'

import { ItemsFilter, SimpleSegment } from '@components'

@connect(
    (state) => ({
        projects: state.project.projects
    }),
    { pushState: push }
)
export default class ProjectSelector extends Component {

    state = {
        filterOptions: this.filterOptions()
    }

    componentDidUpdate(prevProps) {
        if (this.props.projects !== prevProps.projects) {
            this.setState({
                filterOptions: this.filterOptions()
            });
        }
    }

    filterOptions() {
        const allProjects = { key: ALL_PROJECTS_ID, value: ALL_PROJECTS_ID, text: 'All Projects' };
        const projects = this.props.projects.map(p => ({
            key: p.id,
            value: p.id,
            text: p.name
        }));

        return [allProjects, ...projects];
    }

    redirectToProject = (projectId) => {
        if (projectId !== ALL_PROJECTS_ID && this.props.shouldRedirect) {
            this.props.pushState(`/projects/${projectId}`);
        }
    }

    render() {
        return this.state.filterOptions.length > 0 && (
            <SimpleSegment>
                <ItemsFilter
                    scrolling
                    doNotReset
                    by={this.state.filterOptions}
                    defaultValue={this.props.projectId || 0}
                    filterValue={this.props.projectId}
                    setFilter={this.redirectToProject}
                    label="PROJECT:"
                />
            </SimpleSegment>
        )
    }
}
