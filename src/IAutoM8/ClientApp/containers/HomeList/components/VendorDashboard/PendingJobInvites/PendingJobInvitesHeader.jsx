import React from "react";
import { Table } from "semantic-ui-react";

const PendingJobInvitesHeader = () => {
    return (
        <Table.Row>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                Status
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                Task
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                Formula
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                Team
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                Skill
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                Start Date
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                Due Date
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                Duration Hours
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                Time Left
            </Table.HeaderCell>
            <Table.HeaderCell
                style={{ backgroundColor: "#f3f7fa" }}
            ></Table.HeaderCell>
        </Table.Row>
    );
};

export default PendingJobInvitesHeader;
