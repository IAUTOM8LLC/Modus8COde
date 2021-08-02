import React, { Component } from 'react'
import { Feed, Icon } from 'semantic-ui-react'
import { Link } from 'react-router-dom'

import { TimeAgo } from '@components'

export default class TasksFeed extends Component {

    render() {
        const feedItems = this.props.history.map(t =>
            <Feed.Event
                key={t.id}
                style={{ background: '#fff', margin: '5px 0 10px', borderBottom: '1px solid #ccc' }}
            >
                <Feed.Label>
                    <Icon name="tasks" />
                </Feed.Label>
                <Feed.Content>
                    <Feed.Summary>
                        <Feed.User as={Link} to={`/projects/${t.projectId}`}>
                            {t.taskName}
                        </Feed.User>
                        {' '}has changed its status to <strong>{t.status}</strong>
                    </Feed.Summary>
                    {t.conditionOption &&
                        <Feed.Extra text>
                            This task has been selected as option
                            {' '}<i>{t.conditionOption}</i>
                            {' '}for condition
                            <cite>{t.condition}</cite>
                        </Feed.Extra>
                    }
                    <Feed.Meta>
                        <Feed.Like as={Link} to={`/projects/${t.projectId}`}>
                            <Icon name="folder outline" />
                            {t.projectName},
                        </Feed.Like>
                        <Feed.Date style={{ marginLeft: '10px', display: 'inline-block' }}>
                            <TimeAgo date={t.historyTime} />
                        </Feed.Date>
                    </Feed.Meta>
                </Feed.Content>
            </Feed.Event>
        );
        return (
            <div>
                <Feed>
                    {feedItems}
                </Feed>
            </div>
        );
    }
}
