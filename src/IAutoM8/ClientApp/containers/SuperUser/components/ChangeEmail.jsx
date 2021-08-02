import React, { Component } from "react";
import { Button, Modal, Input, Popup } from "semantic-ui-react";
import Axios from "axios";
import { getAuthHeaders } from "@infrastructure/auth";
import { connect } from "react-redux";
import { success, error } from "react-notification-system-redux";

@connect((state) => ({}), {
    success,
    error,
})
export default class ChangeEmail extends Component {
    state = {
        email: this.props.userEmail,
        newEmail: "",
        errors: "",
        showModal: this.props.modalShow,
    };

    handleInputValue = (evt) => {
        this.setState({ errors: "" });
        if (evt.target.value === "") {
            this.setState({ errors: "cannot be empty" });
        } else if (
            !/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(evt.target.value)
        ) {
            this.setState({ errors: "INVALID EMAIL ADDRESS" });
        }
        this.setState({ newEmail: evt.target.value });
    };

    closeModal = (e) => {
        e.preventDefault();
        this.setState({ showModal: false, newEmail: "", errors: "" });
    };

    handleEmail = () => {
        // evt.preventDefault();
        if (this.state.newEmail === "") {
            this.setState({ errors: "cannot be empty" });
            return;
        }
        if (
            !/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(
                this.state.newEmail
            )
        ) {
            this.setState({ errors: "INVALID EMAIL ADDRESS" });
            return;
        }

        const ChangeEmail = {
            eMail: this.props.userEmail,
            newEmail: this.state.newEmail,
        };
        Axios.post(
            "/api/SuperAdmin/change-email",
            ChangeEmail,
            getAuthHeaders()
        )
            .then((response) => {
                this.props.success({
                    title: "Email have been updated successfully",
                });
                this.setState({
                    email: response.data.email,
                    newEmail: "",
                    showModal: false,
                    errors: "",
                });
                this.props.onUpdate();
            })
            .catch((error) => {
                this.props.error({
                    title: error.data.message,
                });
            });
    };

    render() {
        return (
            <Modal
                style={{ width: "720px", marginLeft: "-360px" }}
                trigger={
                    <Popup
                        content="Change Email"
                        trigger={
                            <Button
                                circular
                                className="iauto iauto--email"
                                icon="mail outline"
                                onClick={() =>
                                    this.setState({ showModal: true })
                                }
                            ></Button>
                        }
                    />
                }
                open={this.state.showModal}
            >
                <Modal.Header>Change Email</Modal.Header>
                <Modal.Content>
                    <label style={this.contentLabelStyle}>
                        Old Email: {this.props.userEmail}
                    </label>

                    <Modal.Description>
                        <Input
                            value={this.state.newEmail}
                            id="newEmail"
                            name="newEmail"
                            placeholder="Enter New Email..."
                            style={{ width: "640px" }}
                            onChange={(evt) => this.handleInputValue(evt)}
                        />
                        <span style={this.saveError}>{this.state.errors}</span>
                    </Modal.Description>
                    <div
                        className="actions modal-flex-actions"
                        style={{ paddingRight: "0px" }}
                    >
                        <Button
                            className="ui button iauto grey button-flex-order2"
                            onClick={this.closeModal}
                        >
                            Close
                        </Button>
                        <Button
                            className="ui button iauto purple button-flex-order1"
                            onClick={this.handleEmail}
                        >
                            Save
                        </Button>
                    </div>
                </Modal.Content>
            </Modal>
        );
    }
    contentLabelStyle = {
        display: "block",
        margin: "0 0 12px 2px",
        color: "#78909c",
        fontSize: ".92857143em",
        fontWeight: 700,
        textTransform: "none",
    };

    saveError = {
        // float: "right",
        textTransform: "uppercase",
        color: "#e6424b",
        fontSize: ".73em",
        fontWeight: "600",
        margin: "9px 1px 0 0",
    };
}
