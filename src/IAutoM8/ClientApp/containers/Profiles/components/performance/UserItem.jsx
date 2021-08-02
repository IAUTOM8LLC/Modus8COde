import React, { ReactFragment } from "react";
import { Grid, Table, Rating, Icon } from "semantic-ui-react";

import { formatMoney } from "@utils/formatMoney";

import ConfirmDialog from "../../../SuperUser/components/ConfirmDialog";

import { ModusButton, Inline, UserAcronym, } from "@components";

import AccordionTable from './AccordionTable'



export default function UserItem({ item, onDelete, onEdit, handleCompanyWorker,
    performanceData, companyUserData,
    selectUser,
    selectedUsers, toggleAddButtonModal }) {
    const ctMessage = [
        "Completion Time(CT): ",
        "The amount of time it takes to complete a job from when ",
        "the job is started to when it is marked 'complete'",
    ].join("");

    // const tatMessage = [
    //     "Turnaround Time(TAT): ",
    //     "The amount of time it takes from when the job ",
    //     "is accepted to when it is completed",
    // ].join("");

    
    return (<React.Fragment>
        <Table.Row>
            {/* <Table.Cell >
                <CompanyUserAccordion
                    //companyUserData={companyUserData}
                    userName={item.name}
                    email={item.email}
                    performanceData={performanceData}
                /><span
                    style={{ cursor: "pointer" }}
                    onClick={handleCompanyWorkers}
                >


                </span></Table.Cell> */}
            <Table.Cell onClick={() => selectUser(item.id)}>
                <Icon name={`caret ${selectedUsers[item.id] ? 'down' : 'right'}`} />
                {item.fullName}
            </Table.Cell>
            <Table.Cell>{item.email}</Table.Cell>
            <Table.Cell><UserAcronym fullname={item.fullName}></UserAcronym></Table.Cell>
            <Table.Cell>{item.role}</Table.Cell>
            {/* <Table.Cell textAlign="center">{item.address.city}</Table.Cell> */}
            {/* <Table.Cell>
                <Rating
                    icon="star"
                    defaultRating={item.rating}
                    maxRating={5}
                    disabled
                />
            </Table.Cell> */}
            <Table.Cell>
                <Grid>
                    <Grid.Row>
                        {/* <Grid.Column width={6}>
                            <div style={{ marginTop: "8px" }}>
                                $ {formatMoney(item.price, 2, ".", " ")}
                            </div>
                        </Grid.Column> */}
                        <Grid.Column width={3}>
                            <Inline>
                                {/* <ModusButton
                                    circular
                                    icon="iauto--edit"
                                    popup="Edit"
                                    onClick={() => toggleAddButtonModal(item.formulaTaskId, item.price)}
                                /> */}
                                {/* <ModusButton
                                    className="btn-per-delete"
                                    circular
                                    size="mini"
                                    icon="iauto--remove-white"
                                    popup="Delete"
                                    onClick={() => onDelete(item.id)}
                                /> */}
                            <ConfirmDialog
                                title={`Do you want to login as ${item.fullName}?`}
                                message="Confirm login"
                                userEmail={item.email}
                                action="LogInAsUser"
                                btnProp="circular" 
                            />
                                            &nbsp; &nbsp;&nbsp;
                            <ConfirmDialog
                               title={`Do you want to send confirmation email for ${item.fullName}?`}
                               message="Confirm send"
                               userEmail={item.email}
                               action="ResendEmail"
                               popUpContent="Resend confirmation email"
                               mailConfirmed={
                                item.emailConfirmed
                               }      
                            />
                            </Inline>
                        </Grid.Column>
                    </Grid.Row>
                </Grid>
            </Table.Cell>
            {/* <Table.Cell textAlign="right">
                <Popup
                    trigger={
                        <span className="vendor-time">{item.dwellTime}</span>
                    }
                    size="tiny"
                    content={"Dwell Time: The amount of time that a job"}
                />
            </Table.Cell>
            <Table.Cell textAlign="right">
                <span className="mean-vendor-time">{item.avgDwellTime}</span>
            </Table.Cell>
            <Table.Cell textAlign="right">
                <Popup
                    trigger={
                        <span className="vendor-time">
                            {item.completionTime}
                        </span>
                    }
                    size="tiny"
                    content={ctMessage}
                />
            </Table.Cell>
            <Table.Cell textAlign="right">
                <span className="mean-vendor-time">
                    {item.avgCompletionTime}
                </span>
            </Table.Cell>
            <Table.Cell textAlign="right">
                <Popup
                    trigger={
                        <span className="vendor-time">
                            {item.turnaroundTime}
                        </span>
                    }
                    size="tiny"
                    content={tatMessage}
                />
            </Table.Cell>
            <Table.Cell textAlign="right">
                <span className="mean-vendor-time">
                    {item.avgTurnaroundTime}
                </span>
            </Table.Cell> */}
        </Table.Row>
        {selectedUsers[item.id] ? <Table.Row>
            <Table.Cell colSpan={5}>
                <AccordionTable

                    userData={item.usertaskData}
                    users={item}
                    onEdit={onEdit}
                    onDelete ={onDelete}
                    toggleAddButtonModal={toggleAddButtonModal}
                />
            </Table.Cell>
        </Table.Row> : null}
    </React.Fragment>
    );
}
