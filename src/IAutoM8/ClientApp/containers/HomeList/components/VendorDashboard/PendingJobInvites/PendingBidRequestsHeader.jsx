import React from "react";
import { Table } from "semantic-ui-react";

const PendingBidRequestsHeader = () => {
    return (
        <Table.Row>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                TASK
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                FORMULA
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                TEAM
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                SKILL
            </Table.HeaderCell>
            <Table.HeaderCell style={{ backgroundColor: "#f3f7fa" }}>
                CREATED
            </Table.HeaderCell>
            <Table.HeaderCell
                style={{ backgroundColor: "#f3f7fa" }}
            ></Table.HeaderCell>
        </Table.Row>
    );
};

export default PendingBidRequestsHeader;
