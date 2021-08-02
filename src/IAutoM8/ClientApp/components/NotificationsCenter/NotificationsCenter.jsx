import React, { Component } from 'react'
import { connect } from 'react-redux'
import Notifications from 'react-notification-system-redux'

const notificationStyle = {
    NotificationItem: { // Override the notification item
        DefaultStyle: { // Applied to every notification, regardless of the notification level
            margin: '10px 5px 2px 1px'
        }
    }
};

@connect(
    state => ({
        notifications: state.notifications
    })
)
export default class NotificationsCenter extends Component {

    getConfiguredNotifications() {
        return this.props.notifications.map(n => {
            n.position = 'tr';
            n.autoDismiss = 5;
            return n;
        });
    }

    render() {
        return (
            <Notifications
                style={notificationStyle}
                notifications={this.getConfiguredNotifications()}
            />
        );
    }
}
