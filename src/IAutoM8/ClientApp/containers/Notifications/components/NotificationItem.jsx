import React from 'react'
import { Table } from 'semantic-ui-react'
import ReactHtmlParser from 'react-html-parser';

import { ModusButton, TimeAgo } from '@components'

import notificationIconStyle from '@utils/notificationIconStyle'

export default function NotificationItem({ notification, onClick, onDelete }) {
    const iconStyle = notificationIconStyle(notification.notificationType);
    return (
        <Table.Row>
            <Table.Cell>
                <div className="iauto-notify--cell" onClick={() => {
                    (!notification.isRead || notification.url) &&
                    onClick(notification)
                }}>
                    <div className={`iauto-notify--option_detail-icon ${iconStyle}`}>
                    </div>
                    {
                        !notification.isRead &&
                        <div className="iauto-notify--option_unread" />
                    }
                    <div className="iauto-notify--option_detail">
                        <div className="iauto-notify--option_detail-header">
                            <div className="iauto-notify--option_detail-header_fn">
                                {notification.senderName}
                            </div>,&nbsp;
                            <TimeAgo
                                date={notification.createDate}
                                className="iauto-notify--option_detail-header_time"
                            />
                        </div>
                        <div className="iauto-notify--option_detail-header_message">
                            {ReactHtmlParser(notification.message, { decodeEntities: false })}
                        </div>
                    </div>
                </div>
            </Table.Cell>
            <Table.Cell collapsing className="right aligned">
                <ModusButton
                    circular
                    size="mini"
                    icon="iauto--remove"
                    popup="Delete"
                    onClick={() => onDelete(notification.id)}
                />
            </Table.Cell>
        </Table.Row>
    );
}
