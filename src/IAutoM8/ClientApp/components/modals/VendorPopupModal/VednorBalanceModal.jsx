import React, { Component } from "react";
import { Modal, Table } from "semantic-ui-react";

import { TimeAgo, ModusButton } from "@components";

export default class VednorBalanceModal extends Component {

    constructor(props) {
        super(props);
        this.state = { amount: '' };
        this.handleChange = this.handleChange.bind(this);
    }

    handleChange(event) {
        this.setState({ amount: event.target.value });
    }

    clearAmountState(event) {
        this.setState({ amount: '' });
    }

    render() {
        const { open, onClose, requestFundTransfer, maxAmount, redirectToProfile } = this.props;        
        return (<Modal
            open={open}
            className="task-details-modal"
            size="small"
            onClose={onClose}
        >
            <Modal.Header>
                <div style={{ display: "inline-block" }}>Need Payoneer Email</div>
                <div style={{ display: "inline-block" }} onClick={onClose} >
                    <i className="close icon" style={{
                        margin: "0px 0px 0px 100%", cursor: "pointer",
                        display: "inline-block",
                        paddingLeft: "230px",
                        fontSize: "25px"
                    }}></i></div>
            </Modal.Header>
            <Modal.Content style={{ paddingBottom: "35px", fontSize:"15px",paddingTop:"5px" }}>
                <div>
                    Before you can transfer funds, you need to have an
                    activate Payoneer Email Address in your Vendor Profile.<br />                    
                    <a href="https://blog.payoneer.com/outsourcing/registration-identity-verification/"
                        target="_blank" rel="noopener noreferrer">Click here</a> for instructions.
                </div>
                <div style={{
                    textAlign: "center", paddingTop:
                        "20px", paddingBottom: "40px", paddingRight: "230px"
                }}>
                    <ModusButton
                        wide
                        filled
                        floated="right"
                        content="Redirect to Vendor Profile Page"
                        type="submit"
                        onClick={() =>  redirectToProfile()}
                    />
                </div>

                
            </Modal.Content>

            <div style={{display:"none"}}>
            <Modal.Header>
                <div style={{ display: "inline-block" }}>Request Amount</div>
                <div style={{ display: "inline-block" }} onClick={onClose} >
                    <i className="close icon" style={{
                        margin: "0px 0px 0px 100%", cursor: "pointer",
                        display: "inline-block",
                        paddingLeft: "250px",
                        fontSize: "25px"
                    }}></i></div>
            </Modal.Header>
            <Modal.Content style={{ paddingBottom: "35px" }}>
                <div><div style={{float:"left",paddingTop:"8px"}}>
                    Add Amount:</div>
                    <div className="ui fluid input" style={{
                        width: "70%", paddingLeft: "130px", float: "left"
                    }}>
                        <input className="userProfile.fullName" value={this.state.amount}
                            type="number" max={maxAmount} min={0}
                            onChange={this.handleChange}></input></div>
                </div>
                <div style={{textAlign:"center",paddingTop:"70px",paddingBottom:"40px",paddingRight:"230px"}}>
                    <ModusButton
                        wide
                        filled
                        floated="right"
                        content="Save"
                        type="submit"
                        onClick={() => { requestFundTransfer(this.state.amount); this.clearAmountState(); } }
                /></div>
            </Modal.Content></div>
        </Modal>)
    }
}
