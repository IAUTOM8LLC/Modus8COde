import React from 'react'
import { Table } from 'semantic-ui-react'

import UserItem from './UserItem'

export default function UsersTable({
    users,
    onRoleChange,
    onLockUser,
    onUnlockUser,
    onEditNotificationSettings,
    orderColumn,
    onSort,
    sortDirection,
    onResend,
    onDelete
}) {
    return (
        <Table sortable>
            <Table.Header>
                <Table.Row>
                    <Table.HeaderCell
                        sorted={orderColumn === 'user' ? sortDirection : null}
                        onClick={onSort('user')}
                    >
                        User
                    </Table.HeaderCell>
                    <Table.HeaderCell
                        sorted={orderColumn === 'email' ? sortDirection : null}
                        onClick={onSort('email')}
                    >
                        Email
                    </Table.HeaderCell>
                    <Table.HeaderCell
                        sorted={orderColumn === 'role' ? sortDirection : null}
                        onClick={onSort('role')}
                    >
                        Role
                    </Table.HeaderCell>
                    <Table.HeaderCell collapsing></Table.HeaderCell>
                </Table.Row>
            </Table.Header>

            <Table.Body>
                {
                    users.map((u, i) =>
                        <UserItem
                            key={u.id}
                            user={u}
                            upward={users.length === 1 || users.length - 1 === i}
                            onRoleChange={onRoleChange}
                            onLockUser={onLockUser}
                            onUnlockUser={onUnlockUser}
                            onEditNotificationSettings={onEditNotificationSettings}
                            onResend={onResend}
                            onDelete={onDelete}
                        />
                    )
                }
            </Table.Body>
        </Table>
    );
}
