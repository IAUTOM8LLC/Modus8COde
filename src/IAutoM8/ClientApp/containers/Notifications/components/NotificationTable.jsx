import React from 'react'
import { Table, Header, Icon, Visibility } from 'semantic-ui-react'

import NotificationItem from './NotificationItem'

export default function NotificationTable({
    notifications,
    onDelete,
    onClick,
    nextPage
}) {
    const notificationItems = notifications.map(f =>
        <NotificationItem
            key={f.id}
            notification={f}
            onDelete={onDelete}
            onClick={onClick}
        />
    );

    return (
        <Table sortable>

            <Visibility
                as="tbody"
                continuous={false}
                once={false}
                onBottomVisible={() => nextPage && nextPage()}
            >
                {notificationItems}
                {
                    notificationItems.length === 0 &&
                    <Table.Row>
                        <Table.HeaderCell colSpan={5} style={{ height: 300 }}>
                            <Header as="h2" icon textAlign="center">
                                <Icon name="list layout" circular />
                                <Header.Content>
                                    No notifications created
                                </Header.Content>
                            </Header>
                        </Table.HeaderCell>
                    </Table.Row>
                }
            </Visibility>
        </Table>
    );
}
