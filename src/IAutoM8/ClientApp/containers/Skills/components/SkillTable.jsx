import React from "react";
import { Table, Header, Icon } from "semantic-ui-react";

import SkillItem from "./SkillItem";

export default function SkillTable({
    skills,
    onDelete,
    onEdit,
    orderColumn,
    onSort,
    sortDirection,
    loggedInUserRole,
    showFormulaModal,
    showUserModal,
    showSkillFormulaModal
}) {

    

    const skillItems = skills.map((t) => (
        <SkillItem
            key={t.id}
            skill={t}
            onEdit={onEdit}
            onDelete={onDelete}
            loggedInUserRole={loggedInUserRole}
            showFormulaModal={showFormulaModal}
            showUserModal={showUserModal}
            showSkillFormulaModal={showSkillFormulaModal}
        />
    ));

    return (
        <Table sortable>
            <Table.Header>
                <Table.Row>
                    <Table.HeaderCell
                        sorted={orderColumn === "name" ? sortDirection : null}
                        onClick={onSort("name")}
                    >
                        Name
                    </Table.HeaderCell>
                    {loggedInUserRole !== "Admin"
                        && (
                            <Table.HeaderCell
                                sorted={
                                    orderColumn === "users" ? sortDirection : null
                                }
                                style={{ display: "none" }}
                                onClick={onSort("users")}
                            >
                                Users
                            </Table.HeaderCell>
                        )}

                    <Table.HeaderCell
                        sorted={
                            orderColumn === "users" ? sortDirection : null
                        }
                        onClick={onSort("users")}
                    >
                        Users Count
                        </Table.HeaderCell>

                    <Table.HeaderCell
                        sorted={
                            orderColumn === "teamName" ? sortDirection : null
                        }
                        onClick={onSort("teamName")}
                    >
                        Team Name
                        </Table.HeaderCell>
                    {/* <Table.HeaderCell
                        sorted={
                            orderColumn === "dateCreated" ? sortDirection : null
                        }
                        onClick={onSort("dateCreated")}
                    >
                        Created
                    </Table.HeaderCell> */}
                    <Table.HeaderCell
                        sorted={
                            orderColumn === "lastUpdated" ? sortDirection : null
                        }
                        onClick={onSort("lastUpdated")}
                    >
                        Last updated
                    </Table.HeaderCell>
                    <Table.HeaderCell style={{ display: "none" }}>
                        Used by Formula
                    </Table.HeaderCell>
                    <Table.HeaderCell collapsing />
                </Table.Row>
            </Table.Header>

            <Table.Body>
                {skillItems}
                {skillItems.length === 0 && (
                    <Table.Row>
                        <Table.HeaderCell colSpan={6} style={{ height: 300 }}>
                            <Header as="h2" icon textAlign="center">
                                <Icon name="users" circular />
                                <Header.Content>
                                    No skills created
                                </Header.Content>
                                <Header.Subheader>
                                    Try to create a new skill and assing it to
                                    task
                                </Header.Subheader>
                            </Header>
                        </Table.HeaderCell>
                    </Table.Row>
                )}
            </Table.Body>
        </Table>
    );
}
