import React from 'react'
import { Link } from 'react-router-dom'
import { Button } from 'semantic-ui-react'

import { ItemsFilter, SimpleSegment, ModusButton, Inline } from '@components'

export default function SkillsHeader({ onAddSkillClick, filterBy, setFilterColumn }) {

    const filterOptions = [
        { key: 'name', value: 'name', text: 'Name' },
        { key: 'users', value: 'users', text: 'Users' }
    ];

    return (
        <SimpleSegment clearing className="iauto-projects__header" style={{ marginBottom: 30 }}>
            <Inline floated="right">
                <ItemsFilter
                    by={filterOptions}
                    filterValue={filterBy}
                    setFilter={setFilterColumn}
                />
                <ModusButton
                    wide
                    filled
                    icon="iauto--add-white"
                    content="New skill"
                    onClick={onAddSkillClick}
                    whenPermitted="createSkill"
                />
            </Inline>
        </SimpleSegment>
    );
}
