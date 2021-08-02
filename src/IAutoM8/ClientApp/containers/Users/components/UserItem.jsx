import React from 'react'
import { Table, Dropdown } from 'semantic-ui-react'

import { WhenPermitted, ModusButton, Inline } from '@components'

export default function UserItem({
    user,
    upward,
    onRoleChange,
    onLockUser,
    onUnlockUser,
    onEditNotificationSettings,
    onResend,
    onDelete
}) {
    const { id, fullName, email, isLocked, roles } = user;

    const roleOptions = [
        { key: '1', value: 'Worker', text: 'Worker' },
        { key: '2', value: 'Manager', text: 'Manager' }
    ];

    return (
        <Table.Row>
            <Table.Cell width="5">
                <h4>{fullName}</h4>
            </Table.Cell>

            <Table.Cell width="5">
                {email}
            </Table.Cell>

            <Table.Cell width="3">
                <WhenPermitted rule="changeUsersRole">
                    {(permitted) =>
                        <Dropdown
                            compact
                            selection
                            options={roleOptions}
                            placeholder="User Role"
                            className="modus-users__edit-role"
                            upward={upward}
                            value={roles[0]}
                            disabled={!permitted}
                            onChange={(event, data) => onRoleChange(data, user)}
                        />
                    }
                </WhenPermitted>
            </Table.Cell>

            <Table.Cell collapsing className="right aligned">
                <Inline floated="right">
                    <ModusButton
                        circular
                        size="small"
                        popup="User notifications"
                        icon="iauto--notification"
                        whenPermitted="editUserNotificationSettings"
                        onClick={() => onEditNotificationSettings(id)}
                    />
                    <ModusButton
                        circular
                        popup={isLocked ? 'Unlock user' : 'Lock user'}
                        size="small"
                        icon={isLocked ? 'iauto--unlock' : 'iauto--lock'}
                        onClick={() => {
                            isLocked ? onUnlockUser(id) : onLockUser(id)
                        }}
                    />
                    <ModusButton
                        popup="Resend confirmation email"
                        onClick={() => onResend(id)}
                        content="Resend"
                    />
                    <ModusButton
                        icon="iauto--remove"
                        size="small"
                        circular
                        onClick={() => onDelete(id)}
                        popup="Delete user"
                        whenPermitted="deleteUser"
                    />
                    
                </Inline>
            </Table.Cell>
        </Table.Row>
    );
}
