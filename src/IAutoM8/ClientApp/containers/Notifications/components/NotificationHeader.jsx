import React from 'react'
import { Header } from 'semantic-ui-react'

import { SimpleSegment } from '@components'

export default function NotificationHeader({ content }) {

    return (
        <SimpleSegment clearing className="iauto-notifications__header" floated="left">
            <Header as="h2" size="large" content={content} />
        </SimpleSegment>
    );
}
