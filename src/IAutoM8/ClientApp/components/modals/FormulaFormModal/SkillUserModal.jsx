import React, { Component } from "react";
import { Modal, Table, Header } from "semantic-ui-react";

import { UserAcronym } from "@components";

export default class SkillUserModal extends Component {

    render() {
        const { open, usersList, onClose } = this.props;
        return (<Modal
            open={open}
            className="task-details-modal"
            size="small"
            onClose={onClose}
        >
            <Modal.Header>
                <div style={{ display: "inline-block" }}>User Details</div>
                <div style={{ display: "inline-block" }} onClick={onClose}>
                    <i className="close icon" style={{
                        margin: "0px 0px 0px 100%", cursor: "pointer",
                        display: "inline-block",
                        paddingLeft: "250px",
                        fontSize: "25px"
                    }}></i></div>
            </Modal.Header>

            <Modal.Content style={{ paddingBottom: "35px" }}>
                <Table celled>
                    <Table.Header>
                        <Table.Row>
                            <Table.HeaderCell>User Name</Table.HeaderCell>
                            <Table.HeaderCell>Image</Table.HeaderCell>
                        </Table.Row>
                    </Table.Header>

                    <Table.Body>
                        {usersList && usersList.length > 0
                            && usersList.map((f, index) => {
                                return (
                                    <Table.Row key={index}>
                                        <Table.Cell>
                                            <Header as="h2">{f.userName}</Header>
                                        </Table.Cell>
                                        <Table.Cell>
                                            <Header as="h2">

                                                {f.userImage != null ?
                                                    <img src={f.userImage} width="500" height="600" />
                                                    : <UserAcronym fullname={f.userName} />
                                                }
                                            </Header>
                                        </Table.Cell>
                                    </Table.Row>
                                );
                            })}
                    </Table.Body>
                </Table>
            </Modal.Content>
        </Modal>)
    }
}
