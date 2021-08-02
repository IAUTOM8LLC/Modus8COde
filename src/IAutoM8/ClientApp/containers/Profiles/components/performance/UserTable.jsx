import React from "react";
import { Icon, Table, Header } from "semantic-ui-react";

import CompanyUserAccordion from "./CompanyUserAccordion";

import UserItem from "./UserItem";

export default function UserTable({ companyUserData, onDelete, onEdit,
    handleCompanyWorker, performanceData, selectUser, selectedUsers, toggleAddButtonModal }) {
    const companyUsersData = companyUserData.map((p) => (
        <UserItem
            key={p.id}
            item={p}
            selectUser={selectUser}
            selectedUsers={selectedUsers}
            handleCompanyWorker={handleCompanyWorker}
            performanceData={performanceData}
            toggleAddButtonModal={toggleAddButtonModal}
            onEdit={onEdit}
            onDelete={onDelete}
        />
    ));

    return (
        <Table compact sortable>
            <Table.Header>
                <Table.Row>
                    <Table.HeaderCell>Name</Table.HeaderCell>
                    <Table.HeaderCell>Email</Table.HeaderCell>
                    <Table.HeaderCell>Profile</Table.HeaderCell>
                    <Table.HeaderCell>Role</Table.HeaderCell>
                    <Table.HeaderCell>Action</Table.HeaderCell>
                    {/* <Table.HeaderCell>Rating</Table.HeaderCell>
                    <Table.HeaderCell>Price</Table.HeaderCell>
                    <Table.HeaderCell colSpan="2" textAlign="center">Dwell</Table.HeaderCell>
                    <Table.HeaderCell colSpan="2" textAlign="center">CT</Table.HeaderCell>
                    <Table.HeaderCell colSpan="2" textAlign="center">TAT</Table.HeaderCell> */}
                </Table.Row>
            </Table.Header>
            <Table.Body>
                {companyUsersData}
                {companyUsersData.length === 0 && (
                    <Table.Row>
                        <Table.HeaderCell colSpan={10} style={{ height: 75 }}>
                            <Header as="h4" icon textAlign="center">
                                <Icon name="users" circular />
                                <Header.Content>
                                    No performance data available
                                </Header.Content>
                            </Header>
                        </Table.HeaderCell>
                    </Table.Row>
                )}
            </Table.Body>
        </Table>
    );
}
