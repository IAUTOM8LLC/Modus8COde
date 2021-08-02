import React from "react";
import { Link } from "react-router-dom";
import { Icon, Table, Header, Popup, Radio } from "semantic-ui-react";

import { ModusButton, TimeAgo, WhenPermitted, Inline } from "@components";

export default function FormulaItem({
    formula,
    onDelete,
    onEdit,
    onCopy,
    onCreateProject,
    onUnlock,
    onLock,
    onAddStar,
    onRemoveStar,
    loggedInUserRole,
    changeFormulaStatus,
    // onOpenFormulaOverview,
    // isAdmin
}) {
    const isAdmin = loggedInUserRole.toLowerCase().trim() === "Admin".toLowerCase();
    return (
        <Table.Row>
            <Table.Cell>
                <Icon
                    as="i"
                    size="huge"
                    className={formula.isStarred ? "star" : "star empty"}
                    {...(formula.isStarred && { color: "yellow" })}
                    onClick={() => {
                        formula.isStarred
                            ? onRemoveStar(formula.id)
                            : onAddStar(formula.id);
                    }}
                />
            </Table.Cell>
            <Table.Cell style={{ width: "2%" }}>
                <Popup
                    trigger={<Icon
                        as="i"
                        size="large"
                        className="video camera"
                        // onClick={() => onOpenFormulaOverview(
                        //     formula.id,
                        //     formula.name,
                        //     false,
                        //     true
                        // )}
                        onClick={() =>
                            onCreateProject(
                                formula.id,
                                formula.name
                            )
                        }
                    />}
                    content="Click to open Formula Overview"
                />
            </Table.Cell>
            <Table.Cell className="wrap-word">
                <WhenPermitted rule="accessToFormulaUiEditor">
                    {(permitted) =>
                        permitted ? (
                            <Header
                                size="small"
                                as={Link}
                                to={`/formulas/${formula.id}`}
                            >
                                {formula.name}
                            </Header>
                        ) : (
                            <h4>{formula.name}</h4>
                        )
                    }
                </WhenPermitted>
                <br />
                <Popup
                    flowing
                    hoverable
                    trigger={
                        <div className="iauto-formulas-item-category">
                            {formula.categories.join(", ")}
                        </div>
                    }
                >
                    {formula.categories.map((category, i, arr) => {
                        const divider = i < arr.length - 1 && ", ";
                        return (
                            <div
                                key={i}
                                className="iauto-formulas-hover-item-divider"
                            >
                                <div className="iauto-formulas-hover-item-category">
                                    {category}
                                </div>
                                {divider}
                            </div>
                        );
                    })}
                </Popup>
            </Table.Cell>
            {/* Commenting the code, as per the client feedback dated: August 27, 2020 */}
            {/* <Table.Cell>{formula.owner.fullName}</Table.Cell>
            <Table.Cell>
                <TimeAgo date={formula.dateCreated} />
            </Table.Cell> */}
            <Table.Cell width={2} textAlign="center">
                {formula.outsourceR_TAT} days
            </Table.Cell>
            <Table.Cell width={2} textAlign="center">
                {/*  {formula.totaL_TAT} days */}
            </Table.Cell>
            <Table.Cell collapsing className="center aligned">
                <Inline>
                    <ModusButton
                        size="mini"
                        content="Run Formula"
                        onClick={() =>
                            onCreateProject(
                                formula.id,
                                formula.name
                            )
                        }
                        whenPermitted="createProjectFromFormula"
                        style={{
                            display:
                                loggedInUserRole === "Admin" ? "none" : "block",
                        }}
                    />
                    <div
                        style={{
                            float: "right",
                            marginRight: "20px",
                            display:
                                loggedInUserRole === "Admin" ? "block" : "none",
                        }}
                        onClick={() => changeFormulaStatus(formula.id)}
                    >
                        <label
                            style={{ marginRight: "10px", cursor: "pointer" }}
                        >
                            Draft
                        </label>
                        <Radio
                            toggle
                            label="Ready to Publish"
                            checked={
                                Number(formula.status) === 1 ? false : true
                            }
                        />
                    </div>

                    {((isAdmin && formula.type === "public") || (formula.type === "custom")) && (
                        <ModusButton
                            circular
                            icon="iauto--edit"
                            popup="Edit"
                            onClick={() => onEdit(formula.id)}
                            whenPermitted="formulaModalWindow"
                        />
                    )}
                    <ModusButton
                        circular
                        icon="copy"
                        popup={"Copy & Customize"}
                        onClick={() => onCopy(formula.id)}
                        whenPermitted="formulaModalWindow"
                    />
                    <WhenPermitted rule="lockFormula">
                        <ModusButton
                            circular
                            popup={
                                formula.isLocked
                                    ? "Unlock formula"
                                    : "Lock formula"
                            }
                            size="small"
                            icon={
                                formula.isLocked
                                    ? "iauto--unlock"
                                    : "iauto--lock"
                            }
                            onClick={() => {
                                formula.isLocked
                                    ? onUnlock(formula.id)
                                    : onLock(formula.id);
                            }}
                        />
                    </WhenPermitted>
                    {formula.type === "custom" && (
                        <ModusButton
                            circular
                            size="mini"
                            icon="iauto--remove"
                            popup="Delete"
                            onClick={() => onDelete(formula.id)}
                            whenPermitted="deletFormula"
                        />
                    )}
                </Inline>
            </Table.Cell>
        </Table.Row>
    );
}
