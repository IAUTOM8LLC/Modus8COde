import React from "react";
import { Icon, Table, Rating } from "semantic-ui-react";



import UserTabItemTable from "./UserTabItemTable ";

export default function AccordionTable({users,userData, onDelete, onEdit, toggleAddButtonModal }) {

    const companyUsersTabItemData = userData.map((p) => (
        <UserTabItemTable
            key={p.id}
            item={p}
            toggleAddButtonModal={toggleAddButtonModal}
            users={users}
            onEdit={onEdit}
            onDelete={onDelete}
        />
    ));

    return (
        <Table compact sortable>
            <Table.Header>
                <Table.Row>
                    <Table.HeaderCell>Team</Table.HeaderCell>
                    <Table.HeaderCell>Task</Table.HeaderCell>
                    <Table.HeaderCell>Formula</Table.HeaderCell>
                    <Table.HeaderCell>Skill</Table.HeaderCell>
                    <Table.HeaderCell>Reviews</Table.HeaderCell>
                    <Table.HeaderCell>Rating</Table.HeaderCell>
                    <Table.HeaderCell>Price</Table.HeaderCell>
                    <Table.HeaderCell>Action</Table.HeaderCell>
                </Table.Row>
            </Table.Header>
            <Table.Body>
                {companyUsersTabItemData}
            </Table.Body>
        </Table>
    );
}