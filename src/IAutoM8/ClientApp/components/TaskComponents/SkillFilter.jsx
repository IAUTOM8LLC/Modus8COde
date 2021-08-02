import React, { Component } from 'react'
import { connect } from 'react-redux'

import { setFilterSkillId } from '@store/skill'
import { selectSkillsAssignedToTasks } from '@selectors/skill'

import { ItemsFilter, WhenPermitted, SimpleSegment } from '@components'

@connect(
    (state, props) => ({
        filterSkillId: state.skill.filterSkillId,
        assignedSkills: selectSkillsAssignedToTasks(state, props),
    }),
    { setFilterSkillId }
)
export default class SkillFilter extends Component {

    state = {
        filterOptions: this.filterOptions()
    }

    componentDidUpdate(prevProps) {
        if (this.props.assignedSkills !== prevProps.assignedSkills) {
            this.setState({
                filterOptions: this.filterOptions()
            });
        }
    }

    filterOptions() {
        const defaultItem = {
            key: `allTasks`,
            value: 0,
            text: 'All'
        };

        const options = this.props.assignedSkills.map(t => ({
            key: `skill${t.id}`, value: t.id, text: t.name
        }));

        return [defaultItem, ...options];
    }

    render() {
        return (
            <WhenPermitted rule="filterTasksBySkill">
                <SimpleSegment>
                    <ItemsFilter
                        by={this.state.filterOptions}
                        defaultValue={0}
                        filterValue={this.props.filterSkillId}
                        setFilter={this.props.setFilterSkillId}
                        label="SKILL:"
                    />
                </SimpleSegment>
            </WhenPermitted>
        )
    }
}
