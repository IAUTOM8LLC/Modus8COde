import React from 'react'
import { Link } from 'react-router-dom'
import { Button } from 'semantic-ui-react'

import { ItemsFilter, SimpleSegment, ModusButton, Inline } from '@components'

export default function TeamsHeader({ onAddSkillClick, isAdmin, AddOnlyTeam }) {
    const filterOptions = [
        { key: 'name', value: 'name', text: 'Name' },
        { key: 'users', value: 'users', text: 'Users' }
    ];


    return (
        <SimpleSegment clearing className="iauto-projects__header TeamHeader" style={{ marginBottom: 30 }}>
            <Inline floated="right">
                <ModusButton
                    wide
                    filled
                    icon="iauto--add-white"
                    content="Team"
                    whenPermitted="createTeam"
                    onClick={AddOnlyTeam}
                />

                <ModusButton                    
                    wide
                    filled
                    icon="iauto--add-white"
                    content="Skill"
                    whenPermitted="createTeam"
                    onClick={onAddSkillClick}
                />
            </Inline>
        </SimpleSegment>
    )
}
