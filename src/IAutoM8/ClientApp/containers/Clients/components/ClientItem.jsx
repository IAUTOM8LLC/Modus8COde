import React from 'react'
import { Table, Header } from 'semantic-ui-react'

import { ModusButton, TimeAgo, Inline } from '@components'

export default function ClientItem({ client, onDelete, onEdit }) {
    return (
        <Table.Row>
            <Table.Cell>
                <Header size="small">
                    {client.companyName}
                </Header>
            </Table.Cell>
            <Table.Cell>
                <span>{`${client.firstName} ${client.lastName}`}</span>
                <br />
                <span>{`(${client.email})`}</span>
            </Table.Cell>
            <Table.Cell>
                <span>{`${client.streetAddress1} ${client.city}`}</span>
                <br />
                <span>{`${client.state} ${client.zip}, ${client.country}`}</span>
            </Table.Cell>
            <Table.Cell>
                <TimeAgo date={client.dateCreated} />
            </Table.Cell>
            <Table.Cell>
                <TimeAgo date={client.lastUpdated} />
            </Table.Cell>
            <Table.Cell collapsing className="center aligned">
                <Inline>
                    <ModusButton
                        icon="iauto--edit"
                        circular
                        onClick={() => onEdit(client.id)}
                        popup="Edit"
                        whenPermitted="editClient"
                    />
                    <ModusButton
                        circular
                        icon="iauto--remove"
                        popup={
                            client.hasAssignedProjects ?
                                'You cannot delete client while it is assigned to any project'
                                : 'Delete'
                        }
                        style={client.hasAssignedProjects ? { cursor: 'no-drop' } : {}}
                        onClick={!client.hasAssignedProjects ? () => onDelete(client.id) : null}
                        whenPermitted="deleteClient"
                    />
                </Inline>
            </Table.Cell>
        </Table.Row>
    );
}