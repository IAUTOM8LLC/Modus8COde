import React from 'react'
import { Input, Segment } from 'semantic-ui-react'

import { ModusButton, Inline } from '@components'

import NotificationHeader from './NotificationHeader.jsx';

export default function NotificationFilterHeader({
    search,
    filterSearch,
    readAll
}) {
    return (
        <div className="iauto-notifications-header">
            <Segment clearing basic>
                <NotificationHeader
                    content="MY NOTIFICATIONS"
                />
                <Inline floated="right">
                    <ModusButton
                        onClick={readAll}
                        content="MARK ALL AS READ"
                    />
                </Inline>
            </Segment>
            <Input
                className="iauto-notifications-header_search"
                iconPosition="left"
                icon="search"
                placeholder="Type here to search ..."
                onChange={search}
                value={filterSearch}
            />
        </div>
    );
}

