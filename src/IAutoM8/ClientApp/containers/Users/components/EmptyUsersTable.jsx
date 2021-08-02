import React from 'react'
import { Header, Icon, Table } from 'semantic-ui-react'

export default function EmptyUsersTable() {
    return (
        <Table striped>
            <Table.Body>
                <Table.Row>
                    <Table.Cell>
                        <Header as="h2" icon textAlign="center" >
                            <Icon name="users" circular />
                            <Header.Content>
                                No users added
                            </Header.Content>
                            <Header.Subheader>
                                Try to add user with email and specific roles
                            </Header.Subheader>
                        </Header>
                    </Table.Cell>
                </Table.Row>
            </Table.Body>
        </Table>
    );
}
