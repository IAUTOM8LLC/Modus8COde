import _ from "lodash";
import React, { useState } from "react";
import moment from 'moment'
import { Table, Button } from "semantic-ui-react";
import { v4 as uuidv4 } from "uuid";

import { ModusButton } from "@components";

export default function TeamTable({
    teamName,
    isGlobalTeam,
    skillName,
    isGlobalSkill,
    tableData,
    onOpenOutsourceInvites,
    publishFormula,
    isAdmin
}) {


    // const [modalData, setModalData] = useState({
    //     isVisible: false,
    //     data: [],
    // });
    // const openPopup = (data = []) => {
    //     if (modalData.isVisible) {
    //         setModalData({ ...modalData, isVisible: false, data: [] });
    //     } else {
    //         if (_.isArray(data) && data.length) {
    //             setModalData({ ...modalData, isVisible: true, data });
    //         }
    //     }
    // };

    return (
        <div style={{ border: "1px solid rgba(34, 36, 38, 0.1)" }}>
            <Table celled>
                <Table.Header style={{ lineHeight: "11px" }}>
                    <Table.Row>
                        <Table.HeaderCell style={{ width: "25%" }}>
                            Formula</Table.HeaderCell>
                        <Table.HeaderCell style={{ width: "30%" }}>
                            Task</Table.HeaderCell>
                        <Table.HeaderCell style={{ width: "15%" }}>Updated Date</Table.HeaderCell>
                        <Table.HeaderCell style={{ width: "25%" }}>Outsource</Table.HeaderCell>
                        <Table.HeaderCell style={{ width: "5%" }}></Table.HeaderCell>
                    </Table.Row>
                </Table.Header>
                <Table.Body>
                    {!!(_.isArray(tableData) && tableData.length) &&
                        tableData.map((data, index) => {
                            let outsourceName = "";
                            if (data.outsourcerName !== null) {
                                outsourceName = data.outsourcerName.split(',').map((name, index) => {
                                    return <a className="ui label"
                                        style={{ fontSize: "14px", marginTop: "3px" }}
                                        key={index}>{name}</a>;
                                });
                            }


                            return (
                                <Table.Row
                                    key={uuidv4()}
                                    //style={{
                                    //    backgroundColor:
                                    //        Number(data.formulaIsGlobal ||
                                    //            0) === 1
                                    //            ? "#ebbb10"
                                    //            : "#f5f5f5",
                                    //    color: "#0a0a0a",
                                    //    fontSize: "15px"
                                    //}}

                                    style={{                                        
                                        color: "#0a0a0a",
                                        fontSize: "15px"
                                    }}
                                >
                                    <Table.Cell>
                                        <div style={{ color: "black" }}>
                                            <i
                                                aria-hidden="true"
                                                className="icon formulas"
                                            />
                                            {data.formulaName}
                                        </div>
                                    </Table.Cell>
                                    <Table.Cell style={{ fontSize: "15px" }}>
                                        {data.taskName}</Table.Cell>
                                    <Table.Cell>
                                        {data.formulaUpdatedDate ? moment(
                                            data.formulaUpdatedDate
                                        ).format("MMM D, YYYY h:mm A") : ""}
                                    </Table.Cell>
                                    <Table.Cell>
                                        {outsourceName}
                                        <Button
                                            circular
                                            style={{
                                                backgroundColor:
                                                    "#7f569c",
                                                color: "#FFFFFF",
                                                height: "30px",
                                                paddingTop: "7px",
                                                fontSize: "11px",
                                                float: "right",
                                                display:"none"
                                            }}
                                            onClick={() =>
                                                onOpenOutsourceInvites(
                                                    data.taskID,
                                                    teamName,
                                                    isGlobalTeam,
                                                    skillName,
                                                    isGlobalSkill,
                                                    data.formulaName
                                                )
                                            }
                                        >
                                            INVITE
                                        </Button>
                                    </Table.Cell>
                                    <Table.Cell className="INVITE">
                                        <section
                                            style={{
                                                justifyContent: "space-between",
                                            }}
                                        >
                                            <div>
                                                {/* <div
                                                    style={{
                                                        display: "inline",
                                                    }}
                                                >
                                                    
                                                </div> */}
                                                {(Number(data.taskType || 0) !==
                                                    1 || isAdmin) && (
                                                        <div
                                                            style={{
                                                                display: "inline",
                                                            }}
                                                        >
                                                            <ModusButton
                                                                style={{ display: "none" }}
                                                                icon="iauto--edit"
                                                                circular
                                                                onClick={() => {
                                                                    console.log(
                                                                        "edit ==>"
                                                                    );
                                                                }}
                                                                popup="Edit"
                                                            />
                                                            {data.formulaStatus
                                                                && Number(data.formulaStatus) > 1 ?
                                                                <ModusButton
                                                                    type="button"
                                                                    circular
                                                                    icon="rss"
                                                                    popup="Publish to all Users"
                                                                    className={data.formulaStatus == 3
                                                                        ? "Published" : ""}
                                                                    onClick={() => {
                                                                        publishFormula(data.id,
                                                                            data.showFormulaPublish);
                                                                        //handleChangePublish(index, data.id);
                                                                    }}
                                                                /> : ""
                                                            }
                                                        </div>
                                                    )}
                                            </div>
                                        </section>
                                    </Table.Cell>
                                </Table.Row>
                            );
                        })}
                    {/* {!!(_.isArray(tableData) && tableData.length) &&
                        tableData.map((data) => {
                            return (
                                <Fragment
                                    key={`Skills-${data.skillId}-${data.skillName}`}
                                >
                                    {data.skillId > 0 ? (
                                        <Table.Row
                                            key={`${data.skillId}-${data.skillName}`}
                                        >
                                            <Table.Cell>
                                                {data.type === 1 ? (
                                                    <Icon style={{paddingRight:"25px"}} name="factory" />
                                                ) : (
                                                        <Icon name="copyright"
                                                            style={{ paddingRight: "25px" }} />
                                                )}
                                                {data.skillName}{" "}
                                            </Table.Cell>
                                            <Table.Cell>
                                                {!!(
                                                    _.isArray(data.users) &&
                                                    data.users.length
                                                ) &&
                                                    data.users.map((user) => {
                                                        if (user.userName) {
                                                            return (
                                                                <Label
                                                                    key={
                                                                        user.id
                                                                    }
                                                                >
                                                                    {
                                                                        user.userName
                                                                    }
                                                                </Label>
                                                            );
                                                        }
                                                    })}
                                            </Table.Cell>
                                            <Table.Cell></Table.Cell>
                                            <Table.Cell
                                                onClick={() =>
                                                    openPopup(data.formulas)
                                                }
                                                className={
                                                    _.isArray(data.formulas) &&
                                                    data.formulas.length
                                                        ? "teams-formulas-colum"
                                                        : ""
                                                }
                                            >
                                                {!!_.isArray(data.formulas) &&
                                                    data.formulas.length}
                                            </Table.Cell>
                                        </Table.Row>
                                    ) : (
                                        <Table.Row>
                                            <Table.Cell>
                                                <span
                                                    style={{ padding: "15px" }}
                                                >
                                                    No skills associated
                                                </span>
                                            </Table.Cell>
                                        </Table.Row>
                                    )}
                                </Fragment>
                            );
                        })} */}
                </Table.Body>
            </Table>
            {/* {!!modalData.isVisible && (
                <Modal
                    open={modalData.isVisible}
                    onClose={() => openPopup()}
                    size="small"
                >
                    <Modal.Header>
                        <div style={{ float: "left" }}>Formula List</div>
                        <div
                            style={{
                                float: "left",
                                marginLeft: "507px",
                                marginBottom: "25px",
                            }}
                        >
                            <i
                                className="close icon"
                                style={{
                                    margin: "0 0 0 82%",
                                    cursor: "pointer",
                                }}
                                onClick={() => openPopup()}
                            ></i>
                        </div>
                    </Modal.Header>
                    <Modal.Content>
                        <Table celled style={{ marginBotton: "7px" }}>
                            <Table.Header>
                                <Table.Row>
                                    <Table.HeaderCell>ID</Table.HeaderCell>
                                    <Table.HeaderCell>Name</Table.HeaderCell>
                                    <Table.HeaderCell>Date</Table.HeaderCell>
                                </Table.Row>
                            </Table.Header>
                            <Table.Body className="skillPopUp">
                                {modalData.data &&
                                    modalData.data.map((formula) => (
                                        <Table.Row
                                            key={`${formula.id}-${formula.formulaName}`}
                                        >
                                            <Table.Cell>
                                                {" "}
                                                {formula.id}{" "}
                                            </Table.Cell>
                                            <Table.Cell>
                                                {" "}
                                                {formula.formulaName}{" "}
                                            </Table.Cell>
                                            <Table.Cell>
                                                {moment(
                                                    formula.formulaCreatedDate
                                                ).format("MMM D, YYYY h:mm A")}
                                            </Table.Cell>
                                        </Table.Row>
                                    ))}
                            </Table.Body>
                        </Table>
                    </Modal.Content>
                </Modal>
            )} */}
        </div>
    );
}
