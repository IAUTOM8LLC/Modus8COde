import React, { Component } from "react";
import { Modal, Table, Header } from "semantic-ui-react";

import {  TimeAgo } from "@components";

export default class SkillFormulaModal extends Component {

    render() {
        const { open, selectedSkill, onClose } = this.props;
        return (<Modal
            open={open}
            className="task-details-modal"
            size="small"
            onClose={onClose}
        >
            <Modal.Header>
                <div style={{ display: "inline-block" }}>Formula Details</div>                
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
                            <Table.HeaderCell>Formula Name</Table.HeaderCell>
                            <Table.HeaderCell>Created Date</Table.HeaderCell>
                            <Table.HeaderCell>Updated Date</Table.HeaderCell>
                           
                        </Table.Row>
                    </Table.Header>

                    <Table.Body>                    
                        {selectedSkill && selectedSkill.length > 0
                            && selectedSkill.map((f, index) => {
                            return (
                                <Table.Row key={index}>
                                    <Table.Cell>
                                        <Header as="h2">{f.name}</Header>
                                    </Table.Cell>
                                    <Table.Cell>
                                        <TimeAgo date={f.dateCreated} />
                                    </Table.Cell>
                                    <Table.Cell>
    <TimeAgo date={f.lastUpdated} /></Table.Cell>                                    
                                </Table.Row>
                            );
                            })}

                       

                    </Table.Body>
                </Table>
            </Modal.Content>
        </Modal>)
    }
}
