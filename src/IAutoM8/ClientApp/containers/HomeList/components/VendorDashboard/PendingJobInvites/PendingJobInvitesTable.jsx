import React from "react";
import { Header, Icon, Table } from "semantic-ui-react";
import { v4 as uuidv4 } from "uuid";

import PendingJobInvitesHeader from "./PendingJobInvitesHeader";
import PendingJobInvitesItem from "./PendingJobInvitesItem";

const PendingJobInvitesTable = ({ 
    vendorJobInvites, 
    onPendingInviteRefresh,
    onAccepInvite,
    onDeclineInvite,
}) => {

    const pendingInviteItems = vendorJobInvites.map((invite) => (
        <PendingJobInvitesItem 
            key={uuidv4()} 
            pendingInvite={invite}
            onPendingInviteRefresh={onPendingInviteRefresh}
            onAccepInvite={onAccepInvite}
            onDeclineInvite={onDeclineInvite}
        />
    ));

    return (
        <Table className="home-list" sortable>
            <Table.Header>
                <PendingJobInvitesHeader />
            </Table.Header>

            <Table.Body>
                {pendingInviteItems}
                {vendorJobInvites.length === 0 && (
                    <Table.Row>
                        <Table.HeaderCell colSpan={10} style={{ height: 125 }}>
                            <Header as="h2" icon textAlign="center">
                                <Header.Content>
                                    No Pending Invites
                                </Header.Content>
                            </Header>
                        </Table.HeaderCell>
                    </Table.Row>
                )}
            </Table.Body>
        </Table>
    );
};

export default PendingJobInvitesTable;
