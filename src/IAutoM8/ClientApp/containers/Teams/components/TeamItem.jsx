import React from 'react'
import { List, Table, Header } from 'semantic-ui-react'

import { ModusButton, TimeAgo, Inline } from '@components'

export default function ItemItem({ skill, onDelete, onEdit }) {

    const users = skill.users || [];

    return (
        <Table.Row>
            <Table.Cell>
                <Header size="small">
                    {skill.name}
                </Header>
            </Table.Cell>
            <Table.Cell>
                <List>
                    {users.map(u =>
                        <List.Item key={u.userId}>
                            <List.Content>
                                {u.userName}
                            </List.Content>
                        </List.Item>
                    )}
                </List>
            </Table.Cell>
            <Table.Cell>
                <TimeAgo date={skill.dateCreated} />
            </Table.Cell>
            <Table.Cell>
                <TimeAgo date={skill.lastUpdated} />
            </Table.Cell>
            <Table.Cell collapsing className="center aligned">
                <Inline>
                    <ModusButton
                        icon="iauto--edit"
                        circular
                        onClick={() => onEdit(skill.id)}
                        popup="Edit"
                        whenPermitted="editSkill"
                    />
                    <ModusButton                       
                        type="button"
                        circular
                        icon="rss"                        
                        className="published publishCss"
                        popup="Publish to all project task."
                    />
                    <ModusButton
                        circular
                        icon="iauto--remove"
                        popup={
                            skill.hasAssignedTasks
                                ? 'You cannot delete skill while it is assigned to any task'
                                : 'Delete'
                        }
                        style={skill.hasAssignedTasks ? { cursor: 'no-drop' } : {}}
                        onClick={!skill.hasAssignedTasks ? () => onDelete(skill.id) : null}
                        whenPermitted="deleteSkill"
                    />
                </Inline>
            </Table.Cell>
        </Table.Row>
    );

}
