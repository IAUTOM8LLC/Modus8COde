import axios from 'axios'
import React, { Component } from 'react'
import { connect } from 'react-redux'
import { reset } from 'redux-form'
import { push } from 'react-router-redux'
import { success, error } from 'react-notification-system-redux'
import autobind from 'autobind-decorator'
import { Dimmer, Loader } from 'semantic-ui-react'

import { getAuthHeaders } from '@infrastructure/auth'
import {
    deleteMessage, readMessage, loadNotifications,
    nextPage, notificationSearch, notificationPage,
    readAllMessage
} from '@store/notification'

import { Prompt, FormulaFormModal, WhenPermitted, FormulaToProjectWizard } from '@components'

import NotificationFilterHeader from './components/NotificationFilterHeader'
import NotificationTable from './components/NotificationTable'

import './Notifications.less'

@connect(
    state => ({
        ...state.notification
    }),
    {
        deleteMessage,
        readMessage,
        loadNotifications,
        pushState: push,
        success, error,
        notificationSearch,
        notificationPage,
        nextPage,
        readAllMessage
    }
)
export default class Formulas extends Component {
    state = {  };

    componentWillReceiveProps(nextProps) {
        const {
            searchModel: {
                page,
                filterSearch
            }
        } = this.props;
        if (page !== nextProps.searchModel.page ||
            filterSearch !== nextProps.searchModel.filterSearch)
            this.props.loadNotifications();
    }

    componentDidMount() {
        this.props.loadNotifications(true);
    }

    @autobind
    async handleDeleteNotification(notificationId) {
        const confirmed = await Prompt.confirm(
            `Do you want to delete notification
                ${this.props.infinityMessages
                .find(x => x.id === notificationId)
                .message.replace(/<[^>]+>/g, '')}?`,
            'Confirm delete notification',
            'lab'
        )

        if (confirmed) {
            const { deleteMessage, success, error,
                loadNotifications, notificationPage,
                searchModel: { page } } = this.props;
            deleteMessage(notificationId)
                .then(() => {
                    success({ title: 'Notification was deleted successfully' });
                    if (page === 1)
                        loadNotifications();
                    else
                        notificationPage(1);
                })
                .catch((exception) => {
                    let message = 'Cannot delete notification';
                    if (exception) {
                        message = exception.data.message
                    }
                    this.setState({ message});
                    error({ title: message });
                });
        }
    }

    handleClick = (notification) => {
        const { readMessage, success, error,
            loadNotifications, notificationPage,
            searchModel: { page },
            pushState } = this.props;
        if (notification.isRead) {
            notification.url && pushState(notification.url);
        }
        else {
            readMessage(notification.id)
                .then(() => {
                    if (notification.url) {
                        pushState(notification.url);
                    }
                    else {
                        if (page === 1)
                            loadNotifications();
                        else
                            notificationPage(1);
                    }
                });
        }
    }

    handleReadAll = () => {
        const { readAllMessage, loadNotifications, notificationPage,
            searchModel: { page } } = this.props;
        readAllMessage()
            .then(() => {
                if (page === 1)
                    loadNotifications();
                else
                    notificationPage(1);
            });
    }
    
    render() {
        const {
            searchModel: {
                filterSearch
            }, loading,
            infinityMessages: notifications,
            notificationSearch: search,
            notificationPage,
            nextPage,
            canNextPage
        } = this.props;
        const { message } = this.state;
        return (
            <div className="iauto-notifications">
                <Dimmer page active={loading}>
                    <Loader>{message}</Loader>
                </Dimmer>
                <NotificationFilterHeader
                    search={search}
                    filterSearch={filterSearch}
                    readAll={this.handleReadAll}
                />
                <NotificationTable
                    notifications={notifications}
                    onDelete={this.handleDeleteNotification}
                    onClick={this.handleClick}
                    nextPage={canNextPage ? nextPage : null}
                />
            </div>
        );
    }
}

