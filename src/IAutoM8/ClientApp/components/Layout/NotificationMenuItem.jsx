import React, { Component, Fragment  } from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux';
import { Menu, Dropdown } from 'semantic-ui-react'
import ReactHtmlParser from 'react-html-parser';

import {
    newMessage, getAllUnread, readMessage,
    loadNotifications, notificationPage } from '@store/notification'

import { ModusButton, UserAcronym, TimeAgo } from '@components'

import ModusSocket from '@infrastructure/modusSocket'

import notificationIconStyle from '@utils/notificationIconStyle'

import './NotificationMenuItem.less'

@connect(
    state => ({
        ...state.notification,
        user: state.auth.user
    }),
    {
        newMessage, getAllUnread, readMessage, push,
        loadNotifications, notificationPage
    }
)
export default class NotificationMenuItem extends Component {

    componentWillMount() {
        this.props.getAllUnread();
    }

    componentDidMount() {
        const { user: { id } } = this.props;
        this.createSocketForUser(id);
    }

    componentDidUpdate(prevProps) {
        const { user: { id } } = this.props;
        if (id !== prevProps.user.id) {
            this.createSocketForUser(id);
        }
    }

    componentWillUnmount() {
        this.createSocketForUser();
    }

    createSocketForUser = (id) => {
        if (!id && id === '') {
            this.socket && this.socket.stop();
            return;
        }
        this.socket = new ModusSocket({
            path: 'notification-hub', enableLogging: __DEV__,
            events: [
                {
                    name: 'newMessageRecieved',
                    action: message => this.props.newMessage(message)
                }
            ],
            startEvent: { name: 'SubscribeToUser', args: [id] }
        });
        this.socket.start();
    }

    handleClick = (message) => {
        const { readMessage, loadNotifications, notificationPage,
            searchModel: { page } } = this.props;
        message.url && this.props.push(message.url);
        readMessage(message.id)
            .then(() => {
                if (page === 1)
                    loadNotifications();
                else
                    notificationPage(1);
            });
    }

    renderTrigger = () => {
        const { unreadMessageCount } = this.props;

        return (
            <Fragment>
                <ModusButton
                    icon="iauto--notification"
                    circular
                />
                {
                    unreadMessageCount !== 0 &&
                    <div className="iauto-notify--new-message">{unreadMessageCount}</div>
                }
            </Fragment>
        );
    }

    renderOptions = () => {
        return this.props.messages.filter(m => !m.isRead).map(message => {
            const iconStyle = notificationIconStyle(message.notificationType);
            return (<Dropdown.Item
                className="iauto-notify--option"
                key={message.id}
                content={
                    <Fragment>
                        <div className={`iauto-notify--option_detail-icon ${iconStyle}`}>
                        </div>
                        <div className="iauto-notify--option_detail">
                            <div className="iauto-notify--option_detail-header">
                                <div className="iauto-notify--option_detail-header_fn">
                                    {message.senderName}
                                </div>
                                <TimeAgo
                                    date={message.createDate}
                                    format="MMM DD"
                                    className="iauto-notify--option_detail-header_time"
                                />
                            </div>
                            <div className="iauto-notify--option_detail-header_message">
                                {ReactHtmlParser(message.message, { decodeEntities: false })}
                            </div>
                        </div>
                    </Fragment>}
                onClick={() => this.handleClick(message)}
            />)
        })
    }

    renderHeader = () => (
        <div className="iauto-notify--header">
            <div className="iauto-notify--header_name">your notifications</div>
            <div onClick={() => this.props.push('/notifications')}>
                <div className="iauto-notify--header_arrow_box" />
                <div className="iauto-notify--header_link-to">all</div>
            </div>
        </div>)

    render() {

        return (
            <Menu.Item>
                <Dropdown
                    pointing="top right"
                    scrolling={true}
                    trigger={this.renderTrigger()}
                    icon={null}
                >
                    <Dropdown.Menu>
                        <Dropdown.Header content={this.renderHeader()} />
                        {this.renderOptions()}
                    </Dropdown.Menu>
                </Dropdown>
            </Menu.Item>
        );
    }
}
