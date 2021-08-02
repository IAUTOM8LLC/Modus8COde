import React from "react";
import { Icon, Table, Header } from "semantic-ui-react";

import PerformanceItem from "./PerformanceItem";

export default function PerformanceTable({ performanceData, onDelete, onEdit }) {
    const performanceItems = performanceData.map((p) => (
        <PerformanceItem
            key={p.id}
            item={p}
            onDelete={onDelete}
            onEdit={onEdit}
        />
    ));

    return (
        <Table compact sortable>
            <Table.Header>
                <Table.Row>
                    <Table.HeaderCell>Teams</Table.HeaderCell>
                    <Table.HeaderCell>Skills</Table.HeaderCell>
                    <Table.HeaderCell>Formula</Table.HeaderCell>
                    <Table.HeaderCell>Task</Table.HeaderCell>
                    <Table.HeaderCell>Reviews</Table.HeaderCell>
                    <Table.HeaderCell>Rating</Table.HeaderCell>
                    <Table.HeaderCell>Price</Table.HeaderCell>
                    <Table.HeaderCell colSpan="2" textAlign="center">Dwell</Table.HeaderCell>
                    <Table.HeaderCell colSpan="2" textAlign="center">CT</Table.HeaderCell>
                    <Table.HeaderCell colSpan="2" textAlign="center">TAT</Table.HeaderCell>
                </Table.Row>
            </Table.Header>
            <Table.Body>
                {performanceItems}
                {performanceItems.length === 0 && (
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
