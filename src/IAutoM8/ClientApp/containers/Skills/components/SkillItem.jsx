import React from "react";
import { List, Table, Header } from "semantic-ui-react";

import { ModusButton, TimeAgo, Inline } from "@components";

export default function SkillItem({
    skill,
    onDelete,
    onEdit,
    loggedInUserRole,
    showFormulaModal,
    showUserModal,
}) {
    const users = skill.users || [];

    let combinedList = [];
    if (skill.devFormulas != undefined || skill.revFormulas != undefined) {
        // list = Array.prototype.push.apply(selectedSkill.devFormulas, selectedSkill.revFormulas);
        combinedList = [...skill.devFormulas, ...skill.revFormulas];
    }

    const distinctFormulas = combinedList.filter(
        (thing, i, arr) => arr.findIndex((t) => t.id === thing.id) === i
    );

    const formulaCount = distinctFormulas.length;

    // Manager is not allowed to add users to skill
    // He should be shown the lock icon for skill
    const isEditable =
        skill.isGlobal &&
        loggedInUserRole !== "Admin" &&
        loggedInUserRole !== "Owner";
    const isDeletable = skill.isGlobal && loggedInUserRole !== "Admin";

    return (
        <React.Fragment>
            <Table.Row>
                <Table.Cell>
                    <Header size="small">{skill.name}</Header>
                </Table.Cell>
                {loggedInUserRole !== "Admin" && (
                    <Table.Cell style={{ display: "none" }}>
                        <List>
                            {users.map((u) => (
                                <List.Item key={u.userId}>
                                    <List.Content>{u.userName}</List.Content>
                                </List.Item>
                            ))}
                        </List>
                    </Table.Cell>
                )}

                <Table.Cell>
                    {users.length > 0 ? (
                        <div
                            style={{
                                color: "#6f6feb",
                                fontWeight: "bold",
                                fontSize: "15px",
                                cursor: "pointer",
                            }}
                            onClick={() => showUserModal(users)}
                        >
                            {users.length}
                        </div>
                    ) : (
                        <div
                            style={{
                                // color: "#6f6feb", fontWeight: "bold",
                                fontSize: "15px",
                            }}
                            //    onClick={() => showUserModal(users)}
                        >
                            {users.length}
                        </div>
                    )}
                </Table.Cell>

                <Table.Cell>
                    <Header size="small">{skill.teamName}</Header>
                </Table.Cell>
                {/* <Table.Cell>
                    <TimeAgo date={skill.dateCreated} />
                </Table.Cell> */}
                <Table.Cell>
                    <TimeAgo date={skill.lastUpdated} />
                </Table.Cell>
                <Table.Cell style={{ display: "none" }}>
                    {formulaCount > 0 ? (
                        <div
                            style={{
                                color: "#6f6feb",
                                fontWeight: "bold",
                                fontSize: "15px",
                                cursor: "pointer",
                            }}
                            onClick={() => showFormulaModal(skill)}
                        >
                            {formulaCount}
                        </div>
                    ) : (
                        <div>{formulaCount}</div>
                    )}
                </Table.Cell>
                <Table.Cell collapsing className="center aligned">
                    <Inline>
                        <ModusButton
                            circular
                            icon={isEditable ? "iauto--unlock" : "iauto--edit"}
                            whenPermitted="editSkill"
                            popup={isEditable ? "A marketplace skill" : "Edit"}
                            style={isEditable ? { cursor: "no-drop" } : {}}
                            onClick={
                                !isEditable ? () => onEdit(skill.id) : null
                            }
                        />
                        {!isDeletable && (
                            <ModusButton
                                circular
                                icon="iauto--remove"
                                whenPermitted="deleteSkill"
                                popup={
                                    skill.hasAssignedTasks
                                        ? "You cannot delete skill while it is assigned to any task"
                                        : "Delete"
                                }
                                style={
                                    skill.hasAssignedTasks
                                        ? { cursor: "no-drop" }
                                        : {}
                                }
                                onClick={
                                    !skill.hasAssignedTasks
                                        ? () => onDelete(skill.id)
                                        : null
                                }
                            />
                        )}
                        {/* <ModusButton
                            circular
                            icon="iauto--remove"
                            whenPermitted="deleteSkill"
                            popup={
                                isMarketplaceSkill
                                    ? "You cannot delete a marketplace skill"
                                    : skill.hasAssignedTasks
                                        ? "You cannot delete skill while it is assigned to any task"
                                        : "Delete"
                            }
                            style={
                                isMarketplaceSkill
                                    ? { cursor: "no-drop" }
                                    : skill.hasAssignedTasks
                                        ? { cursor: "no-drop" }
                                        : {}
                            }
                            onClick={
                                isMarketplaceSkill
                                    ? null
                                    : !skill.hasAssignedTasks
                                        ? () => onDelete(skill.id)
                                        : null
                            }
                        /> */}
                    </Inline>
                </Table.Cell>
            </Table.Row>
        </React.Fragment>
    );
}
