import axios from 'axios';
import React, { Component } from 'react'
import { connect } from 'react-redux'
import { reset } from 'redux-form'
import { success, error, info } from 'react-notification-system-redux'
import sortBy from 'lodash/sortBy'
import autobind from 'autobind-decorator'

import {
    loadUsers, addUser, makeWorker, makeManager, lockUser, unlockUser,
    resend, deleteUser
} from '@store/users'
import { filterUsersByQuery } from '@selectors/users'
import { getAuthHeaders } from '@infrastructure/auth'
import { toggleDirection, userAccessor } from '@utils/sort'

import {
    Prompt,
    WhenPermitted,
    UserNotificationSettingsModal,
    RoleNotificationSettingsModal
} from '@components'

import UsersTable from './components/UsersTable'
import AddUserForm from './components/AddUserForm'
import EmptyUsersTable from './components/EmptyUsersTable'
import UsersHeader from './components/UsersHeader'

import './Users.less'

@connect(
    state => ({
        ...state.users,
        users: filterUsersByQuery(state),
        userPermissions: state.auth.permissions
    }),
    {
        loadUsers,
        addUser,
        lockUser,
        unlockUser,
        makeManager,
        makeWorker,
        reset,
        success, error, info,
        resend, deleteUser
    }
)
export default class Users extends Component {

    state = {
        orderColumn: null,
        orderDirection: null,
        orderedUsers: []
    };

    componentDidMount() {
        this.props.loadUsers();
    }

    componentWillReceiveProps(nextProps) {
        const { orderColumn, orderDirection } = this.state;
        if (orderColumn) {
            let users = nextProps.users ? sortBy(nextProps.user, userAccessor(orderColumn)) : [];
            if (orderDirection === 'descending') {
                users = users.reverse();
            }
            this.setState({
                orderedUsers: users
            });
        } else {
            this.setState({
                orderedUsers: nextProps.users.some(x => x.roles === undefined) ? [] : [...nextProps.users]
            });
        }
    }

    handleSubmit = (values) => {
        return this.props
            .addUser({
                fullName: values.fullName,
                email: values.email,
                role: values.role
            })
            .then(() => {
                this.props.reset('addAssignedUser');
                this.props.loadUsers();
                this.props.success({ title: 'User has been added' });
            })
            .catch(() => this.props.error({ title: 'User already exists' }));
    }

    @autobind
    async handleLockUser(userId) {
        if (!this.props.userPermissions.lockUser) {
            this.props.info({ title: 'You do not have permission to lock user' });
            return;
        }

        const confirmed = await Prompt.confirm(
            'If you lock user all his formulas and projects will lose owner. Are you sure?',
            'Confirm lock user',
            'user'
        )

        if (confirmed) {
            this.props.lockUser(userId)
                .then(() => this.props.success({ title: 'User has been locked' }))
        }
    }

    handleUnlockUser = (userId) => {
        if (!this.props.userPermissions.unlockUser) {
            this.props.info({ title: 'You do not have permission to unlock user' });
            return;
        }
        this.props
            .unlockUser(userId)
            .then(() => this.props.success({ title: 'User has been unlocked' }));
    }

    handleRoleChange = (data, user) => {
        if (!this.props.userPermissions.changeUsersRole) {
            this.props.info({ title: "You do not have permission to set user's role" });
            return;
        }
        if (data.value === 'Manager')
            this.props.makeManager(user);
        else
            this.props.makeWorker(user);
    }

    handleResend = (userId) => {
        if (!this.props.userPermissions.unlockUser) {
            this.props.info({ title: 'You do not have permission to unlock user' });
            return;
        }
        this.props
            .resend(userId)
            .then(() => this.props.success({ title: 'Confirmation email have been sended' }))
            .catch(() => this.props.info({
                title: 'Email was confirmed'
            }));
    }

    handleNotificationSettingsEdit = (userId) => {
        if (!this.props.userPermissions.editUserNotificationSettings) {
            this.props.info({ title: 'You do not have permission to edit notification settings' });
            return;
        }
        axios.get(`/api/notificationSettings/user/${userId}`, getAuthHeaders())
            .then(({ data }) => {
                const user = this.props.users.find(u => u.id === userId);
                this.notificationsSettingsModal.show(
                    {
                        fullName: user.fullName,
                        initialValues: { notificationSettings: data }
                    }
                ).then(({ notificationSettings }) => {
                    axios.put(`/api/notificationSettings/user/${userId}`,
                        notificationSettings, getAuthHeaders()
                    ).then(() => this.props.success({
                        title: 'Settings were successfully saved'
                    })).catch(() => this.props.error({
                        title: 'Cannot update notification settings'
                    }));

                    this.props.reset('userNotificationSettigns');
                });
            });
    }

    handleRoleNotificationSettingsEdit = () => {
        if (!this.props.userPermissions.editRoleNotificationSettings) {
            this.props.info({
                title: 'You do not have permission to edit role notification settings'
            });
            return;
        }

        axios.get(`/api/notificationSettings/role`, getAuthHeaders())
            .then(({ data }) => this.roleNotificationsSettingsModal
                .show({ initialValues: data })
                .then((data) => {
                    axios.put(`/api/notificationSettings/role`,
                        data, getAuthHeaders()
                    ).then(() => this.props.success({
                        title: 'Settings were successfully saved'
                    })).catch(() => this.props.error({
                        title: 'Cannot update notification settings'
                    }));
                    this.props.reset('roleNotificationSettigns');
                })
            );
    }

    handleSort = clickedColumn => () => {
        const { orderColumn, orderDirection, orderedUsers } = this.state;

        if (orderColumn !== clickedColumn) {

            this.setState({
                orderColumn: clickedColumn,
                orderedUsers: sortBy(orderedUsers, userAccessor(clickedColumn)),
                orderDirection: 'ascending'
            });
            return;
        }

        this.setState({
            orderedUsers: orderedUsers.reverse(),
            orderDirection: toggleDirection(orderDirection)
        });
    }
    @autobind
    async handleDeleteUser(userId) {
        if (!this.props.userPermissions.deleteUser) {
            this.props.info({ title: 'You do not have permission to delete user' });
            return;
        }

        const confirmed = await Prompt.confirm(
            'Do you want delete user?',
            'Confirm delete user',
            'user'
        )

        if (confirmed) {
            this.props.deleteUser(userId)
                .then(() => this.props.success({ title: 'User has been deleted' }))
                .catch(() => this.props.error({ title: 'Cannot delete user since it has work in progress' }));
        }
    }

    render() {
        const {
            orderColumn,
            orderDirection,
            orderedUsers
        } = this.state;
        const usersWithoutOwner = orderedUsers.filter(p => p.roles.indexOf('Owner') === -1);

        return (
            <div className="modus-users">
                <UsersHeader onEditRoleNotififcationSettings={this.handleRoleNotificationSettingsEdit} />

                <WhenPermitted rule="createUser">
                    <AddUserForm onSubmit={this.handleSubmit} />
                </WhenPermitted>

                {
                    usersWithoutOwner.length > 0
                        ? <UsersTable
                            users={usersWithoutOwner}
                            onRoleChange={this.handleRoleChange}
                            onLockUser={this.handleLockUser}
                            onUnlockUser={this.handleUnlockUser}
                            onResend={this.handleResend}
                            onEditNotificationSettings={this.handleNotificationSettingsEdit}
                            orderColumn={orderColumn}
                            onSort={this.handleSort}
                            sortDirection={orderDirection}
                            onDelete={this.handleDeleteUser}
                        />
                        : <EmptyUsersTable />
                }

                <WhenPermitted rule="editUserNotificationSettings">
                    <UserNotificationSettingsModal ref={(c) => { this.notificationsSettingsModal = c; }} />
                </WhenPermitted>

                <WhenPermitted rule="editRoleNotificationSettings">
                    <RoleNotificationSettingsModal
                        ref={(c) => { this.roleNotificationsSettingsModal = c; }}
                    />
                </WhenPermitted>
            </div>
        );
    }
}
