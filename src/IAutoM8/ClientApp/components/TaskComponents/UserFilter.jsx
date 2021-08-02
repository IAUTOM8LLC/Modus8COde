import React, { Component } from 'react'
import { connect } from 'react-redux'

import { setFilterUserId } from '@store/users'

import { getUsers } from '@selectors/users'

import { ItemsFilter, WhenPermitted, SimpleSegment } from '@components'

@connect(
    (state, props) => ({
        filterUserId: state.users.filterUserId,
        users: getUsers(state)
    }),
    { setFilterUserId }
)
export default class UserFilter extends Component {
    state = {
        filterOptions: this.filterOptions()
    }

    componentDidUpdate(prevProps) {
        if (this.props.users !== prevProps.users) {
            this.setState({
                filterOptions: this.filterOptions()
            });
        }
    }

    filterOptions() {
        const defaultItem = {
            key: 'allUsers',
            value: 0,
            text: 'All'
        };

        const options = this.props.users.map(t => ({
            key: `user${t.id}`, value: t.id, text: t.fullName
        }));

        return [defaultItem, ...options];
    }

    render() {
        return (
            <WhenPermitted rule="filterTasksByUser">
                <SimpleSegment>
                    <ItemsFilter
                        by={this.state.filterOptions}
                        defaultValue={0}
                        filterValue={this.props.filterUserId}
                        setFilter={this.props.setFilterUserId}
                        label="USER:"
                    />
                </SimpleSegment>
            </WhenPermitted>
        )
    }
}
