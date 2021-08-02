import React from 'react'
import { Table, Header, Icon } from 'semantic-ui-react'

import ClientItem from './ClientItem'

export default function ClientsTable({
    clients,
    onDelete,
    onEdit,
    orderColumn,
    onSort,
    sortDirection
}) {
    const clientItems = clients.map(c =>
        <ClientItem
            key={c.id}
            client={c}
            onDelete={onDelete}
            onEdit={onEdit}
        />
    );

    return (
        <Table sortable>
            <Table.Header>
                <Table.Row>
                    <Table.HeaderCell
                        sorted={orderColumn === 'companyName' ? sortDirection : null}
                        onClick={onSort('companyName')}
                    >
                        Company Name
                    </Table.HeaderCell>
                    <Table.HeaderCell
                        sorted={orderColumn === 'representative' ? sortDirection : null}
                        onClick={onSort('representative')}
                    >
                        Representative
                    </Table.HeaderCell>
                    <Table.HeaderCell
                        sorted={orderColumn === 'address' ? sortDirection : null}
                        onClick={onSort('address')}
                    >
                        Address
                    </Table.HeaderCell>
                    <Table.HeaderCell
                        sorted={orderColumn === 'dateCreated' ? sortDirection : null}
                        onClick={onSort('dateCreated')}
                    >
                        Created
                    </Table.HeaderCell>
                    <Table.HeaderCell
                        sorted={orderColumn === 'lastUpdated' ? sortDirection : null}
                        onClick={onSort('lastUpdated')}
                    >
                        Last updated
                    </Table.HeaderCell>
                    <Table.HeaderCell collapsing />
                </Table.Row>
            </Table.Header>

            <Table.Body>
                {clientItems}
                {
                    clientItems.length === 0 &&
                    <Table.Row>
                        <Table.HeaderCell colSpan={5} style={{ height: 300 }}>
                            <Header as="h2" icon textAlign="center">
                                <Icon name="users" circular />
                                <Header.Content>
                                    No clients added
                                </Header.Content>
                                <Header.Subheader>
                                    Try to added a new client and assign it to project
                                </Header.Subheader>
                            </Header>
                        </Table.HeaderCell>
                    </Table.Row>
                }
            </Table.Body>
        </Table>
    );
}
